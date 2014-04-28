using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using One.Net.BLL.Scaffold.Model;
using System.Data.SqlClient;
using One.Net.BLL.Scaffold.DAL;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using MsSqlDBUtility;
using System.Data;

namespace One.Net.BLL.Scaffold
{
    public class PhysicalSchema
    {
        public static bool CheckDatabaseConfiguration()
        {
            // check if we have connection string
            if (String.IsNullOrEmpty(DbHelper.ConnectionString))
                return false;

            List<VirtualTable> physicalTables = null;

            try
            {
                physicalTables = ListPhysicalTables();
            }
            catch (SqlException sex)
            {
                switch (sex.Number)
                {
                    case 4060:
                        // can't open database, probably doesn't exist or user has no permissions to access it.
                        break;
                    default:
                        break;
                }
            }

            if (physicalTables == null)
                return false;
            if (physicalTables.Where(t => DbHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_col").Count() < 1)
                return false;
            if (physicalTables.Where(t => DbHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_relation").Count() < 1)
                return false;
            if (physicalTables.Where(t => DbHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_table").Count() < 1)
                return false;

            return true;
        }

        public static bool CreateConfiguration(out List<string> errors)
        {
            errors = new List<string>();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("One.Net.BLL.Scaffold.DAL.Scaffold.sql");
            string contents = "";
            using (var streamReader = new StreamReader(stream))
            {
                contents = streamReader.ReadToEnd();
            }

            var splitter = new Regex("GO");
            string[] queryArray = splitter.Split(contents);

            bool success = true;

            foreach (string query in queryArray)
            {
                if (!string.IsNullOrEmpty(query.Trim()))
                {
                    try
                    {
                        var result = SqlHelper.ExecuteNonQuery(DbHelper.ConnectionString, CommandType.Text, query);
                    }
                    catch (SqlException sex)
                    {
                        errors.Add(sex.Message);
                        //logger.Error("ERROR : Failed to update from file: " + fullFileName + " from version " + rawDbVersion + " to version: " + rawFileVersion, ex);
                        success = false;
                    }
                }
            }
            return success;
        }

        public static List<VirtualTable> ListPrimaryKeyCandidates(int foreginKeyVirtualTableId)
        {
            var foreginKeyVirtualTable = Schema.GetVirtualTable(foreginKeyVirtualTableId);
            var virtualTables = Schema.ListVirtualTables();

            var result = new List<VirtualTable>();
            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.StoredProcedure, "[dbo].[sp_fkeys]",
                new SqlParameter("@fktable_name", DbHelper.GetTableQualifier(foreginKeyVirtualTable.StartingPhysicalTable))))
            {
                while (reader.Read())
                {
                    var foreignKeyTable = "[" + (string)reader["PKTABLE_OWNER"] + "].[" + (string)reader["PKTABLE_NAME"] + "]";

                    var vtables = (from v in virtualTables
                                   where v.StartingPhysicalTable == foreignKeyTable
                                   select v);
                    foreach (var vt in vtables)
                    {
                        result.Add(vt);
                    }
                }
            }
            return result;
        }

        public static List<VirtualTable> ListForeignKeyCandidates(int primaryKeyVirtualTableId)
        {
            var primaryKeyVirtualTable = Schema.GetVirtualTable(primaryKeyVirtualTableId);

            var virtualTables = Schema.ListVirtualTables();

            var result = new List<VirtualTable>();
            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.StoredProcedure, "[dbo].[sp_fkeys]",
                        new SqlParameter("@pktable_name", DbHelper.GetTableQualifier(primaryKeyVirtualTable.StartingPhysicalTable))))
            {
                while (reader.Read())
                {
                    var foreignKeyTable = "[" + (string)reader["FKTABLE_OWNER"] + "].[" + (string)reader["FKTABLE_NAME"] + "]";

                    var vtables = (from v in virtualTables
                                   where v.StartingPhysicalTable == foreignKeyTable
                                   select v);

                    foreach (var vt in vtables)
                    {
                        result.Add(vt);
                    }
                }
            }
            return result;
        }

        internal static List<VirtualColumn> ListPhysicalColumns(VirtualTable virtualTable)
        {
            var result = new List<VirtualColumn>();
            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.StoredProcedure,
                                                        "sp_columns", new SqlParameter("@table_name", DbHelper.GetTableQualifier(virtualTable.StartingPhysicalTable))))
            {
                while (reader.Read())
                {
                    var virtualColumn = new VirtualColumn();
                    virtualColumn.VirtualTableId = virtualTable.Id;
                    virtualColumn.VirtualTableName = virtualTable.StartingPhysicalTable;
                    virtualColumn.Name = reader.GetString(3);
                    virtualColumn.Precision = (int)reader["PRECISION"];
                    virtualColumn.DefaultValue = reader["COLUMN_DEF"] == DBNull.Value ? null : (string)reader["COLUMN_DEF"];
                    virtualColumn.IsPartOfPrimaryKey = virtualTable.PrimaryKeys.Contains(virtualColumn.Name);
                    virtualColumn.IsNullable = ((short)reader["NULLABLE"]) > 0;
                    virtualColumn.IsIdentity = ((string)reader["TYPE_NAME"]).Contains("identity");
                    virtualColumn.DbType = DbHelper.GetDbType(reader.GetInt16(4));
                    result.Add(virtualColumn);
                }
            }

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.StoredProcedure,
                                                        "[dbo].[sp_fkeys]", new SqlParameter("@fktable_name", DbHelper.GetTableQualifier(virtualTable.StartingPhysicalTable))))
            {
                while (reader.Read())
                {
                    var name = (string)reader["FKCOLUMN_NAME"];

                    var virtualColumn = ((from p in result where p.Name == name select p)).First();
                    virtualColumn.PrimaryKeyTableName =
                        "[" + (string)reader["PKTABLE_OWNER"] + "].[" + (string)reader["PKTABLE_NAME"] + "]";
                }
            }
            return result;
        }

        public static List<VirtualColumn> ListPhysicalColumns(int virtualTableId)
        {
            var virtualTable = Schema.GetVirtualTable(virtualTableId);

            if (virtualTable != null)
            {
                return ListPhysicalColumns(virtualTable);
            }
            return new List<VirtualColumn>();
        }

        public static List<VirtualTable> ListPhysicalTables()
        {
            var result = new List<VirtualTable>();
            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.StoredProcedure,
                                                        "sp_tables", new SqlParameter("@table_type", "'TABLE'")))
            {
                while (reader.Read())
                {
                    var virtualTable = new VirtualTable();
                    virtualTable.StartingPhysicalTable = "[" + (string)reader["TABLE_OWNER"] + "].[" + (string)reader["TABLE_NAME"] + "]";
                    //virtualList.PrimaryDbEntityType = (string)reader["TABLE_TYPE"];
                    result.Add(virtualTable);
                }
            }
            return result;
        }
    }
}
