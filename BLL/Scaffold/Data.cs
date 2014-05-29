﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using log4net;
using MsSqlDBUtility;
using System.Threading;
using One.Net.BLL.Web;
using One.Net.BLL.Scaffold.Model;
using One.Net.BLL.Scaffold.DAL;
using System.Configuration;

namespace One.Net.BLL.Scaffold
{
    public static class Data
    {
        static readonly BInternalContent intContentB = new BInternalContent();
        static readonly ILog log = LogManager.GetLogger(typeof(Data));

        public static DataTable ListItems(int virtualTableId, ListingState state)
        {
            var virtualTable = Schema.GetVirtualTable(virtualTableId);
            var table = (virtualTable == null) ? new DataTable() : new DataTable(virtualTable.StartingPhysicalTable);
            table.ExtendedProperties.Add("AllRecords", 0);
            if (virtualTable == null)
                return table;

            // we need to clone this because we're changeing the list below
            var virtualColumns = new List<VirtualColumn>(virtualTable.VirtualColumns);

            if (virtualTable.VirtualColumns.Count == 0)
                return table;

            var mainSql = "";
            var cteFieldsList = "";
            var innerJoinSql = " ";
            var primaryKey = "";

            int i = 0;

            var dataKeyNames = new List<string>();

            foreach (var field in virtualColumns)
            {
                var column = new DataColumn { ColumnName = field.FQName, Caption = field.FriendlyName, ReadOnly = true };

                // TODO: probably exclude this one from select, too
                column.ExtendedProperties.Add("ShowOnList", field.ShowOnList);

                if (field.IsMultiLanguageContent && field.IsPartOfUserView && field.ShowOnList)
                {
                    field.Ordinal = i++;
                    var contentAlias = "cds" + field.Ordinal;

                    table.Columns.Add(column);
                    mainSql += contentAlias + ".[html], ";
                    cteFieldsList += field.Name + ", ";

                    var contentDbBuilder = new SqlConnectionStringBuilder(MsSqlDBUtility.SqlHelper.ConnStringMain);

                    innerJoinSql += "\n    LEFT JOIN [" + contentDbBuilder.InitialCatalog + "].[dbo].[content_data_store] AS " + contentAlias + " ON " + field.FQName + " =  " + contentAlias + ".[content_fk_id] AND " + contentAlias + ".[language_fk_id] = " + Thread.CurrentThread.CurrentCulture.LCID;
                }
                else if (field.IsPartOfPrimaryKey && !field.IsPartOfForeignKey)
                {
                    field.Ordinal = i++;
                    // always select PK
                    column.ReadOnly = true;
                    mainSql += field.FQName + ", ";
                    cteFieldsList += field.Name + ", ";
                    primaryKey += field.FQName + ", ";
                    dataKeyNames.Add(field.FQName);
                    table.Columns.Add(column);
                }
                else if (field.IsPartOfUserView && field.ShowOnList)
                {
                    field.Ordinal = i++;
                    table.Columns.Add(column);
                    mainSql += field.FQName + ", ";
                    cteFieldsList += field.Name + ", ";
                }
            }

            if (primaryKey.Length == 0)
                throw new Exception("Tables without primary keys are not supported. Table:" + virtualTable.StartingPhysicalTable);

            var relations = Schema.ListRelations(virtualTableId);

            foreach (var relation in relations)
            {
                // TODO: depending on if isnullable of FK column, we should use either LEFT or INNER join
                innerJoinSql += "\n    LEFT JOIN " + relation.PrimaryKeySourceTableName + " ON ";

                var foreignKeys = from e in virtualColumns
                                  where
                                      e.IsPartOfForeignKey &&
                                      DbHelper.GetTableQualifier(e.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.PrimaryKeySourceTableName)
                                  select e;

                if (foreignKeys.Count() == 0)
                    throw new Exception("Relation found, but enforced foreign key is probably missing in main table " + virtualTable.StartingPhysicalTable);
                else if (foreignKeys.Count() != 1)
                    throw new Exception("Multiple foreign key relation currently not supported.");

                innerJoinSql += " " + relation.PrimaryKeySourceTableName + "." + relation.PrimaryKeyName + "=" + foreignKeys.First().FQName + " AND ";

                if (foreignKeys.Count() > 0)
                    innerJoinSql = innerJoinSql.Substring(0, innerJoinSql.Length - 4);

                mainSql += relation.PrimaryKeySourceTableName + ".[" + relation.PrimaryKeyDisplayColumn + "], ";

                var foreignKeyDisplayColumn = new VirtualColumn { Ordinal = i++, VirtualTableName = relation.PrimaryKeySourceTableName, Name = relation.PrimaryKeyDisplayColumn, IsMultiLanguageContent = relation.IsMultiLanguageContent };
                cteFieldsList += foreignKeyDisplayColumn.Name + "_" + foreignKeyDisplayColumn.Ordinal + ", ";

                virtualColumns.Add(foreignKeyDisplayColumn);
                var column = new DataColumn
                {
                    ColumnName = foreignKeyDisplayColumn.FQName,
                    Caption = "FK Disp:" + relation.PrimaryKeyDisplayColumn,
                    ReadOnly = true
                };
                // TODO: this was a hack
                column.ExtendedProperties.Add("ShowOnList", true);
                column.ExtendedProperties.Add("Relation", true);
                table.Columns.Add(column);
            }


            if (mainSql.Length == 0)
                return table;
            i++;

            mainSql = mainSql.Substring(0, mainSql.Length - 2);
            cteFieldsList += "RowNumber";
            primaryKey = primaryKey.Substring(0, primaryKey.Length - 2);

            var sql = "WITH ItemsCTE(" + cteFieldsList + @")  AS 
( 
    SELECT " + mainSql + @", ROW_NUMBER() 
    OVER (";
            if (state.UsesSorting && mainSql.Contains(state.SortField))
                sql += "ORDER BY " + state.SortField + " " + state.DbSortDirection;
            else
                sql += "ORDER BY " + primaryKey;

            sql += @") AS RowNumber 
    FROM " + virtualTable.StartingPhysicalTable;
            sql += innerJoinSql;
            sql += "\n    WHERE 1=1 " + virtualTable.Condition;
            sql += @"
) 
SELECT *, (SELECT MAX(RowNumber) FROM ItemsCTE) AllRecords 
FROM ItemsCTE 
WHERE RowNumber BETWEEN @fromRecordIndex AND @toRecordIndex ";

            var debugSql = sql + "\n--------------\n";

            table.ExtendedProperties["DataKeyNames"] = dataKeyNames.ToArray();
            var prm = new SqlParameter[2];
            prm[0] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            prm[1] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);

            //try
            //{
            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, sql, prm))
            {
                while (reader.Read())
                {
                    DataRow row = table.NewRow();
                    foreach (var field in virtualColumns)
                    {
                        if (field.Ordinal >= 0)
                        {
                            var f = reader[field.Ordinal];
                            row[field.FQName] = f;
                            if (table.Columns[field.Ordinal].ExtendedProperties["Relation"] != null && field.IsMultiLanguageContent)
                            {
                                var contentId = int.Parse(row[field.FQName].ToString());
                                row[field.FQName] = intContentB.Get(contentId).Html;
                            }
                        }
                    }
                    table.ExtendedProperties["AllRecords"] = int.Parse(reader[i].ToString());
                    table.Rows.Add(row);
                }
            }

            /* }
             catch (SqlException ex)
             {
                 log.Error(ex);
                 debugSql += "\n--------------\n";
                 debugSql += ex.Message + "\n";
                 debugSql += ex.LineNumber + "\n";
                 debugSql += ex.Source;
             }*/

            table.ExtendedProperties.Add("sql", debugSql);
            return table;
        }

        public static Dictionary<int, string> GetManyToMany(int relationId, int dataKey)
        {
            var result = new Dictionary<int, string>();
            var relation = Schema.GetRelation(relationId);
            if (!relation.IsManyToMany)
                return result;


            var foreignKeyColumns = Schema.ListVirtualColumns(relation.ForeignKeyTableId);

            var filterColumns = foreignKeyColumns.Where(c => DbHelper.GetTableQualifier(c.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.XrefPrimaryKeyTableName));
            if (filterColumns.Count() != 1)
                throw new Exception("We can't do a N:M join if xref table doesn't contain exactly one primary key.");

            var joinColumns = foreignKeyColumns.Where(c => DbHelper.GetTableQualifier(c.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));
            if (joinColumns.Count() != 1)
                throw new Exception("We can't do a N:M join if primary key table doesn't contain exactly one primary key.");


            var sql = "SELECT " + relation.FQPrimaryKeyName + ", " + relation.FQPrimaryKeyDisplayColumn + "\n";
            sql += " FROM " + relation.ForeignKeyTableName + "\n";

            foreach (var joinColumn in joinColumns)
            {
                sql += " INNER JOIN " + relation.PrimaryKeySourceTableName +
                    "ON " + relation.FQPrimaryKeyName + "=" + joinColumn.FQName + "\n";
            }
            sql += " WHERE " + filterColumns.First().FQName + " = @mainFilter";


            var p = new SqlParameter("@mainFilter", dataKey);

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, sql, p))
            {
                while (reader.Read())
                {
                    var key = reader.GetInt32(0);
                    var display = reader.GetString(1);

                    result.Add(key, display);
                }
            }

            return result;
        }
        
        public static string GetForeignKeySelectedOption(int relationId, int foreignKeyTablePrimaryKeyValue)
        {
            var relation = Schema.GetRelation(relationId);

            // TODO: can we use BOVirtualTable.VirtualColumns?
            var virtualColumns = PhysicalSchema.ListPhysicalColumns(relation.ForeignKeyTableId);

            var foreignKeyTablePrimaryKeys = virtualColumns.Where(c => c.IsPartOfPrimaryKey && !c.IsPartOfForeignKey);

            if (foreignKeyTablePrimaryKeys.Count() != 1)
                throw new Exception("Table which contains foreign key doesn't have singular primary key.");

            var foreignKeys = virtualColumns.Where(c => c.IsPartOfForeignKey && DbHelper.GetTableQualifier(c.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));

            if (foreignKeys.Count() != 1)
                throw new Exception("Can't do a 1:n join if we don't have a singular foreign key.");

            var sql = "SELECT TOP(1) " + relation.FQPrimaryKeyDisplayColumn +
                      " FROM " + relation.ForeignKeyTableName +
                      " LEFT JOIN " + relation.PrimaryKeySourceTableName + " ON " + relation.FQPrimaryKeyName + "=" + foreignKeys.First().FQName +
                      " WHERE " + foreignKeyTablePrimaryKeys.First().FQName + "= @mainFilter";

            var result = SqlHelper.ExecuteScalar(DbHelper.ConnectionString, CommandType.Text, sql,
                new SqlParameter("@mainFilter", foreignKeyTablePrimaryKeyValue));
            if (result == DBNull.Value)
                return "";
            return relation.IsMultiLanguageContent ? intContentB.Get((int)result).Html : result.ToString();
        }
        
        public static EditableItem GetItem(int virtualTableId, IOrderedDictionary dataKeys)
        {
            var virtualTable = Schema.GetVirtualTable(virtualTableId);
            if (virtualTable == null)
                return null;

            // we need to clone this because we're changeing the list below
            var virtualColumns = new List<VirtualColumn>(virtualTable.VirtualColumns);

            var mainSql = "";

            int i = 0;

            var item = new EditableItem();

            foreach (var column in virtualColumns)
            {
                var editableColumn = (VirtualColumn)column;
                if (column.IsPartOfPrimaryKey && !column.IsPartOfForeignKey)
                {
                    editableColumn.Ordinal = i++;
                    editableColumn.BackendType = FieldType.Display;
                    mainSql += column.FQName + ", ";
                    item.Columns.Add(column.FQName, editableColumn);
                }
                else if (column.IsPartOfUserView)
                {
                    editableColumn.BackendType = FieldType.SingleText;
                    editableColumn.Ordinal = i++;
                    mainSql += editableColumn.FQName + ", ";
                    if (column.IsMultiLanguageContent)
                    {
                        editableColumn.BackendType = FieldType.MultiLanguageText;
                    }
                    item.Columns.Add(editableColumn.FQName, editableColumn);
                }
            }

            var relations = Schema.ListRelations(virtualTableId);
            foreach (var relation in relations)
            {
                var foreignKeys = from e in virtualColumns
                                  where
                                      e.IsPartOfForeignKey &&
                                      DbHelper.GetTableQualifier(e.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.PrimaryKeySourceTableName)
                                  select e;

                if (foreignKeys.Count() != 1)
                    throw new Exception("Multiple foreign key relation currently not supported.");

                mainSql += foreignKeys.First().FQName + ", ";

                var editableColumn = foreignKeys.First();
                editableColumn.PartOfRelationId = relation.Id;
                editableColumn.BackendType = FieldType.OneToMany;
                editableColumn.Ordinal = i++;
                if (!editableColumn.IsPartOfUserView)
                {
                    editableColumn.IsPartOfUserView = true;
                    item.Columns.Add(editableColumn.FQName, editableColumn);
                }
            }
            var manyToManyRelations = Schema.ListManyToManyRelations(virtualTableId);
            foreach (var relation in manyToManyRelations)
            {
                var manyToManyColumn = new VirtualColumn();
                manyToManyColumn.PartOfRelationId = relation.Id;
                manyToManyColumn.BackendType = FieldType.ManyToMany;
                manyToManyColumn.Ordinal = i++;
                manyToManyColumn.IsPartOfUserView = true;
                manyToManyColumn.Name = relation.PrimaryKeyName;
                manyToManyColumn.VirtualTableName = relation.ForeignKeyTableName;
                manyToManyColumn.VirtualTableId = relation.ForeignKeyTableId;
                item.Columns.Add(manyToManyColumn.FQName, manyToManyColumn);
            }

            mainSql = mainSql.Substring(0, mainSql.Length - 2);

            if (dataKeys == null)
                return item;

            var sql = "SELECT TOP(2) " + mainSql + " FROM " + virtualTable.StartingPhysicalTable + " WHERE 1=1";
            var parameters = DbHelper.PreparePrimaryKeyParameters(dataKeys, ref sql);

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    foreach (var field in virtualColumns)
                    {
                        if (field.Ordinal >= 0)
                        {
                            var f = reader[field.Ordinal];
                            if (f == DBNull.Value)
                            {
                                item.Columns[field.FQName].ValueIsNull = true;
                            }
                            else
                            {
                                switch (item.Columns[field.FQName].DbType.ToString())
                                {
                                    case "System.Int32":
                                        item.Columns[field.FQName].ValueInteger = (int)f;
                                        break;
                                    case "System.DateTime":
                                        item.Columns[field.FQName].ValueDateTime = (DateTime)f;
                                        break;
                                    case "System.Decimal":
                                        item.Columns[field.FQName].ValueDecimal = (decimal)f;
                                        break;
                                    case "System.String":
                                        item.Columns[field.FQName].ValueString = (string)f;
                                        break;
                                    case "System.Boolean":
                                        item.Columns[field.FQName].ValueBoolean = (bool)f;
                                        break;
                                    case "System.Double":
                                        item.Columns[field.FQName].ValueDouble = (double)f;
                                        break;
                                    default:
                                        item.Columns[field.FQName].ValueString = f.ToString();
                                        break;
                                }
                            }
                        }

                    }
                }
                if (reader.Read())
                    throw new Exception("Recieved multiple results when we should get only one.");
            }

            return item;
        }


        public static void UpdateItemManyToManyFields(int virtualTableId, EditableItem item, IOrderedDictionary primaryKeys)
        {
            foreach (var column in item.Columns.Values)
            {
                if (column.BackendType == FieldType.ManyToMany)
                {
                    var relation = Schema.GetRelation(column.PartOfRelationId);
                    if (!relation.IsManyToMany)
                        throw new Exception("Trying to update n:m relation, but there is a mismatch between field type and relation type.");

                    var foreignKeyColumns = Schema.ListVirtualColumns(relation.ForeignKeyTableId);

                    var filterColumns = foreignKeyColumns.Where(c => DbHelper.GetTableQualifier(c.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.XrefPrimaryKeyTableName));
                    if (filterColumns.Count() != 1)
                        throw new Exception("We can't do a N:M join if xref table doesn't contain exactly one primary key.");

                    var joinColumns = foreignKeyColumns.Where(c => DbHelper.GetTableQualifier(c.PrimaryKeyTableName) == DbHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));
                    if (joinColumns.Count() != 1)
                        throw new Exception("We can't do a N:M join if primary key table doesn't contain exactly one primary key.");

                    UpdateOrInsertToXrefJoinTable(relation.ForeignKeyTableName, filterColumns.First().FQName,
                                                      joinColumns.First().FQName, primaryKeys[0], column.NewValueIntegerList);
                }
            }
        }

        public static bool ChangeItem(int virtualTableId, EditableItem item, ref IOrderedDictionary primaryKeys)
        {
            var isInsert = primaryKeys == null;

            // TODO: add support for sequencer
            var virtualTable = Schema.GetVirtualTable(virtualTableId);
            var sql = "";
            var parameters = new List<SqlParameter>();
            if (isInsert)
            {
                if (!virtualTable.CanInsert)
                    throw new Exception("trying to insert to virtual table that has no support for inserting.");

                primaryKeys = new OrderedDictionary();
                sql += "INSERT INTO " + virtualTable.StartingPhysicalTable + " (";

                var columnsSql = "";
                var valuesSql = "";

                foreach (var column in item.Columns.Values)
                {
                    if (column.IsPartOfPrimaryKey && !column.IsPartOfForeignKey)
                    {
                        primaryKeys.Add(column.Name, null);
                        // skip primary keys of this table on insert
                    }
                    else if (column.BackendType == FieldType.ManyToMany)
                    {
                        // skip n:m 
                        // we have diffrent system for storing these values
                    }
                    else if (column.IsMultiLanguageContent || column.BackendType == FieldType.MultiLanguageText)
                    {
                        columnsSql += column.FQName + ", ";
                        valuesSql += "@V" + column.Name + ", ";
                        parameters.Add(new SqlParameter("@V" + column.Name, column.NewValueInteger));
                    }
                    else if (column.IsPartOfForeignKey || column.IsPartOfUserView)
                    {
                        columnsSql += column.FQName + ", ";
                        valuesSql += "@V" + column.Name + ", ";
                        parameters.Add(new SqlParameter("@V" + column.Name, column.NewValue));
                    }
                }
                sql += columnsSql.Substring(0, columnsSql.Length - 2) + ") VALUES (";
                sql += valuesSql.Substring(0, valuesSql.Length - 2) + "); SET @PrimaryKeyOutput = SCOPE_IDENTITY();";

                var output = new SqlParameter("@PrimaryKeyOutput", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;
                parameters.Add(output);
            }
            else
            {
                sql += "UPDATE " + virtualTable.StartingPhysicalTable + " SET ";

                foreach (var column in item.Columns.Values)
                {
                    if (column.IsPartOfPrimaryKey && !column.IsPartOfForeignKey)
                    {
                        // skip primary keys of this table on insert
                    }
                    else if (column.BackendType == FieldType.ManyToMany)
                    {
                        // skip n:m 
                        // we have diffrent system for storing these values
                    }
                    else if (column.IsMultiLanguageContent || column.BackendType == FieldType.MultiLanguageText)
                    {
                        if (column.NewValueInteger != column.ValueInteger)
                        {
                            sql += column.FQName + " = " + "@V" + column.Name + ", ";
                            parameters.Add(new SqlParameter("@V" + column.Name, column.NewValueInteger));
                        }
                    }
                    else if (column.IsPartOfForeignKey || column.IsPartOfUserView)
                    {
                        //if (!column.ValueIsNull)
                        //{
                            sql += column.FQName + " = " + "@V" + column.Name + ", ";
                            parameters.Add(new SqlParameter("@V" + column.Name, column.NewValue));
                        //}
                    }
                }
                sql = sql.Substring(0, sql.Length - 2) + " WHERE 1=1";
                var primaryKeyParameters = DbHelper.PreparePrimaryKeyParameters(primaryKeys, ref sql);
                parameters.AddRange(primaryKeyParameters);
            }
            var paramsToPass = parameters.ToArray();
            var result = SqlHelper.ExecuteNonQuery(DbHelper.ConnectionString, CommandType.Text, sql, paramsToPass);

            if (isInsert)
            {
                var id = int.Parse(paramsToPass[parameters.Count - 1].Value.ToString());
                primaryKeys[0] = id;
            }
            return result > 0;
        }


        public static Dictionary<int, string> GetForeignKeyOptions(int relationId, string search, int limit)
        {
            var result = new Dictionary<int, string>();
            var relation = Schema.GetRelation(relationId);

            var contentDbBuilder = new SqlConnectionStringBuilder(MsSqlDBUtility.SqlHelper.ConnStringMain);

            var sql = relation.IsMultiLanguageContent ?
                         "SELECT TOP(" + limit + ") p." + relation.PrimaryKeyName + ", c.html as DisplayColumn " +
                      " FROM " + relation.PrimaryKeySourceTableName + " p" +
                      " INNER JOIN [" + contentDbBuilder.InitialCatalog + "].[dbo].[content_data_store] c ON c.content_fk_id = " + relation.PrimaryKeyDisplayColumn + " AND c.language_fk_id = " + Thread.CurrentThread.CurrentCulture.LCID +
                      " WHERE c.html LIKE @Search"
                        :
                        "SELECT TOP(" + limit + ") " + relation.PrimaryKeyName + ", " + relation.PrimaryKeyDisplayColumn + " as DisplayColumn " +
                      " FROM " + relation.PrimaryKeySourceTableName +
                      " WHERE LOWER(" + relation.PrimaryKeyDisplayColumn + ") LIKE LOWER(@Search)" +
                      " ORDER BY " + relation.PrimaryKeyDisplayColumn;

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, sql, new SqlParameter("@Search", search + "%")))
            {
                while (reader.Read())
                {
                    var key = reader.GetInt32(0);
                    var display = reader.GetValue(1).ToString();
                    result.Add(key, display);
                }
            }
            return result;
        }

        public static Dictionary<int, string> GetForeignKeyOptions(int relationId, int limit)
        {
            var result = new Dictionary<int, string>();
            var relation = Schema.GetRelation(relationId);

            var sql = limit > 0 ? "SELECT TOP(" + limit + ") " : "SELECT ";
            sql += relation.PrimaryKeyName + ", " + relation.PrimaryKeyDisplayColumn + " as DisplayColumn FROM " + relation.PrimaryKeySourceTableName + " ORDER BY " + relation.PrimaryKeyDisplayColumn;

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, sql))
            {
                while (reader.Read())
                {
                    var key = reader.GetInt32(0);
                    var display = reader.GetValue(1).ToString();

                    result.Add(key, display);
                }
            }
            return result;
        }
        
        public static bool DeleteItem(int virtualTableId, IOrderedDictionary dataKeys)
        {
            var virtualTable = Schema.GetVirtualTable(virtualTableId);
            // TODO: add delete of depndent stuff, especially content table.
            var sql = "DELETE FROM " + virtualTable.StartingPhysicalTable + " WHERE 1=1";
            var parameters = DbHelper.PreparePrimaryKeyParameters(dataKeys, ref sql);

            return SqlHelper.ExecuteNonQuery(DbHelper.ConnectionString, CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// Creates or updates n:m join, deletes old joins.
        /// Assumes that table has primary or unique key over the join fields.
        /// </summary>
        private static void UpdateOrInsertToXrefJoinTable(string tableName, string xrefKeyName, string foreignKeyName, object xrefValue, List<int> foreignKeyValues)
        {
            var p = new SqlParameter[2];
            p[0] = new SqlParameter("@mainFilter", xrefValue);

            var selectSql = "SELECT " + foreignKeyName + " FROM " + tableName + " WHERE " + xrefKeyName + "=@mainFilter";
            //var currentXrefs = new List<int>();
            var deleteXrefs = new List<int>();
            var insertXrefs = new List<int>(foreignKeyValues);

            using (var reader = SqlHelper.ExecuteReader(DbHelper.ConnectionString, CommandType.Text, selectSql, p[0]))
            {
                while (reader.Read())
                {
                    var value = (int)reader[0];
                    insertXrefs.Remove(value);

                    if (!foreignKeyValues.Contains(value))
                        deleteXrefs.Add(value);
                }
            }

            var insertSql = "INSERT INTO " + tableName + " (" + xrefKeyName + ", ";
            insertSql += foreignKeyName + ", ";
            insertSql = insertSql.Substring(0, insertSql.Length - 2);
            insertSql += ") VALUES ( @mainFilter, ";
            insertSql += "@join )";

            var deleteSql = "DELETE FROM " + tableName + " WHERE " + xrefKeyName + "=@mainFilter AND " + foreignKeyName + "=@join";

            foreach (var foreignKeyValue in insertXrefs)
            {
                p[1] = new SqlParameter("@join", foreignKeyValue);
                SqlHelper.ExecuteNonQuery(DbHelper.ConnectionString, CommandType.Text, insertSql, p);
                //currentXrefs.Remove(foreignKeyValue);
            }

            foreach (var leftOver in deleteXrefs)
            {
                p[1] = new SqlParameter("@join", leftOver);
                SqlHelper.ExecuteNonQuery(DbHelper.ConnectionString, CommandType.Text, deleteSql, p);
            }
        }

        public static void ChangeMultiLanguageContent(EditableItem item)
        {
            foreach (var column in item.Columns.Values)
            {
                if (column.IsMultiLanguageContent && 
                    column.NewValueContent != null &&
                    column.NewValueContent.Count > 0)
                {
                    var isInsert = false;

                    if (column.ValueInteger <= 1)
                    {
                        var contentDbBuilder = new SqlConnectionStringBuilder(MsSqlDBUtility.SqlHelper.ConnStringMain);

                        // check if content exists
                        var result = SqlHelper.ExecuteScalar(DbHelper.ConnectionString, CommandType.Text,
                            "SELECT TOP(1) content_fk_id FROM [" + contentDbBuilder.InitialCatalog + "].[dbo].[content_data_store] WHERE content_fk_id = @id_content", new SqlParameter("@id_content", column.ValueInteger));

                        isInsert = result == null;
                        if (!isInsert)
                        {
                            var contentId = (int)result;
                            isInsert = contentId <= 0;
                        }
                    }

                    foreach (var l in column.NewValueContent.Keys)
                    {
                        var c = column.NewValueContent[l];
                        c.ContentId = column.ValueInteger > 0 ? (int?)column.ValueInteger : null;
                        c = (c != null && !string.IsNullOrEmpty(c.Html) ? c : null);
                        if (c != null) // && !string.IsNullOrEmpty(c.Html)) why are you not letting people blank out a content?
                        {
                            var p = new[]
                                    {
                                        new SqlParameter("@id_content", column.ValueInteger),
                                        new SqlParameter("@id_language", l),
                                        new SqlParameter("@html", c.Html)
                                    };

                            var contentDbBuilder = new SqlConnectionStringBuilder(MsSqlDBUtility.SqlHelper.ConnStringMain);

                            var sql = "SELECT TOP(1) content_fk_id FROM [" + contentDbBuilder.InitialCatalog + "].[dbo].[content_data_store] WHERE content_fk_id = @id_content AND language_fk_id = @id_language";
                            var result = SqlHelper.ExecuteScalar(DbHelper.ConnectionString, CommandType.Text, sql, p);

                            if (isInsert && result != null)
                                throw new Exception("This should be insert.");

                            if (c.ContentId == 0)
                                c.ContentId = null;

                            intContentB.Change(c);
                            column.ValueInteger = c.ContentId.Value;
                        }
                    }
                    // make sure we update the ID now
                    if (isInsert)
                    {
                        column.NewValueInteger = column.ValueInteger;
                        column.ValueInteger = -1;
                    }
                }
            }
        }
    }
}
