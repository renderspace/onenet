using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using One.Net.BLL.Scaffold.Model;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using MsSqlDBUtility;
using System.Data;

namespace One.Net.BLL.Scaffold
{
    public class PhysicalSchema
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PhysicalSchema));

        public static bool CheckDatabaseConfiguration()
        {
            // check if we have connection string
            if (String.IsNullOrEmpty(Schema.ConnectionString) || Schema.ConnectionString.Length < 20)
            {
                log.Info("CheckDatabaseConfiguration: no database connection string or string is too short to be viable.");
                return false;
            }
                

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
            if (physicalTables.Where(t => SqlHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_col").Count() < 1)
                return false;
            if (physicalTables.Where(t => SqlHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_relation").Count() < 1)
                return false;
            if (physicalTables.Where(t => SqlHelper.GetTableQualifier(t.StartingPhysicalTable) == "_virtual_table").Count() < 1)
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
                        var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, query);
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
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure, "[dbo].[sp_fkeys]",
                new SqlParameter("@fktable_name", SqlHelper.GetTableQualifier(foreginKeyVirtualTable.StartingPhysicalTable))))
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
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure, "[dbo].[sp_fkeys]",
                        new SqlParameter("@pktable_name", SqlHelper.GetTableQualifier(primaryKeyVirtualTable.StartingPhysicalTable))))
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
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure,
                                                        "sp_columns", new SqlParameter("@table_name", SqlHelper.GetTableQualifier(virtualTable.StartingPhysicalTable))))
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
                    var typeName = (string)reader["TYPE_NAME"];
                    virtualColumn.IsIdentity = typeName.Contains("identity");
                    if (virtualColumn.IsIdentity)
                    {
                        typeName = typeName.Replace("identity", "").Trim();
                    }
                    //var dbTypeId = reader.GetInt16(4);
                    virtualColumn.DbType = GetDbType(typeName);
                    result.Add(virtualColumn);
                }
            }

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure,
                                                        "[dbo].[sp_fkeys]", new SqlParameter("@fktable_name", SqlHelper.GetTableQualifier(virtualTable.StartingPhysicalTable))))
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


        public static DataType GetDbType(string typeName)
        {
            switch (typeName)
            {
                case "int":
                case "bigint":
                case "smallint":
                case "tinyint":
                    return DataType.Int;
                case "money":
                case "smallmoney":
                case "decimal":
                    return DataType.Decimal;
                case "numeric":
                case "float":
                case "real":
                    return DataType.Double;

                case "date":
                    return DataType.Date;
                case "time":
                    return DataType.Time;
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return DataType.DateTime;

                case "bit":
                    return DataType.Boolean;
                case "binary":
                case "varbinary":
                    return DataType.Binary;

                case "char":
                case "varchar":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "sysname":
                case "uniqueidentifier":
                    return DataType.String;
                default:
                    throw new Exception("Unknown SQL data type: " + typeName);
            }
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
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure,
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

        internal static string GetForeignKeyColumnName(string primaryKeySourceTableName, string foreignKeyTableName)
        {
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure, "[dbo].[sp_fkeys]",
               new SqlParameter("@pktable_name", SqlHelper.GetTableQualifier(primaryKeySourceTableName))))
            {
                while (reader.Read())
                {

                    if ((string)reader["FKTABLE_NAME"] == SqlHelper.GetTableQualifier(foreignKeyTableName))
                        return "[" + (string)reader["FKTABLE_OWNER"] + "].[" + (string)reader["FKTABLE_NAME"] + "].[" + (string)reader["FKCOLUMN_NAME"];
                }
            }
            return null;
        }
    }
}
