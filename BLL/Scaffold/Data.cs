using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NLog;
using MsSqlDBUtility;
using System.Threading;
using One.Net.BLL.Web;
using One.Net.BLL.Scaffold.Model;
using System.Configuration;

namespace One.Net.BLL.Scaffold
{
    public static class Data
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        static readonly BInternalContent intContentB = new BInternalContent();

        public static DataTable ListItems(int virtualTableId, ListingState state, ForeignKeyFilter filter = null, bool typedOutput = false, string searchTerm = "")
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
                if (typedOutput && field.DbType == DataType.DateTime)
                {
                    column.DataType = typeof(DateTime);
                }
                column.ExtendedProperties.Add("EnableSearch", field.EnableSearch);

                if (field.IsMultiLanguageContent && ((field.IsPartOfUserView && field.ShowOnList) || typedOutput))
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
                    column.ExtendedProperties.Add("PK", true);
                    column.ReadOnly = true;
                    mainSql += field.FQName + ", ";
                    cteFieldsList += field.Name + ", ";
                    primaryKey += field.FQName + ", ";
                    dataKeyNames.Add(field.FQName);
                    table.Columns.Add(column);
                }
                else if ((field.IsPartOfUserView && field.ShowOnList) || typedOutput)
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
                                      SqlHelper.GetTableQualifier(e.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.PrimaryKeySourceTableName)
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
                    // Caption = "FK Disp:" + relation.PrimaryKeyDisplayColumn,
                    Caption = relation.FriendlyName,
                    ReadOnly = true
                };
                // TODO: this was a hack
                column.ExtendedProperties.Add("ShowOnList", relation.ShowOnList);

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
            var numberOfSearchTerms = 0;
            if (searchTerm.Length > 0 && virtualTable.HasSearchColumns)
            {
                foreach (var vc in virtualColumns.Where(vc => vc.EnableSearch && !vc.IsMultiLanguageContent))
                {
                    sql += (numberOfSearchTerms++ == 0 ? "AND " : "OR ") + vc.FQName + " LIKE @searchTerm ";
                }
            }

            if (filter != null)
            {
                var foreignKeyColumnName = PhysicalSchema.GetForeignKeyColumnName(filter.PrimaryKeySourceTableName, virtualTable.StartingPhysicalTable);
                if (!string.IsNullOrWhiteSpace(foreignKeyColumnName))
                {
                    sql += "\n AND " + foreignKeyColumnName + "=@PrimaryKeyValue";
                }
                else
                {
                    throw new Exception("foreignKeyColumnName not found. Probably FK is missing in database. PK table: " + filter.PrimaryKeySourceTableName + " FK table: " + virtualTable.StartingPhysicalTable);
                }
            }
            sql += @"
) 
SELECT *, (SELECT MAX(RowNumber) FROM ItemsCTE) AllRecords 
FROM ItemsCTE 
WHERE RowNumber BETWEEN @fromRecordIndex AND @toRecordIndex ";

            var debugSql = sql + "\n--------------\n";

            table.ExtendedProperties["DataKeyNames"] = dataKeyNames.ToArray();


            var prm = new List<SqlParameter>();
            prm.Add(new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex));
            prm.Add(new SqlParameter("@toRecordIndex", state.DbToRecordIndex));
            if (filter != null)
            {
                prm.Add(new SqlParameter("@PrimaryKeyValue", filter.PrimaryKeyValue));
            } 
            if (searchTerm.Length > 0 && virtualTable.HasSearchColumns)
            {
                prm.Add(new SqlParameter("@searchTerm", "%" + searchTerm + "%"));
            }

            //try
            //{
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, sql, prm.ToArray()))
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
                                row[field.FQName] = GetMultilanguageContent(field.FQName, row);
                            }
                            if (field.DbType == DataType.DateTime)
                            {
                                if (f == DBNull.Value && field.IsNullable)
                                {
                                    row[field.FQName] = "";
                                }
                                else
                                {
                                    var d = (DateTime)f;
                                    
                                    if (typedOutput)
                                    {
                                        row[field.FQName] = d; //.UnixTicks().ToString();
                                    }
                                    else
                                    {
                                        var formattedDate = "";
                                        formattedDate = d.ToShortDateString();
                                        if (d.ToShortTimeString() != "00:00")
                                            formattedDate += " " + d.ToShortTimeString();
                                        row[field.FQName] = formattedDate;
                                    }
                                    
                                }
                            }
                            if (field.DbType == DataType.Date)
                            {
                                if (typedOutput)
                                {
                                    row[field.FQName] = ((DateTime)f);
                                }
                                else
                                {
                                    row[field.FQName] = ((DateTime)f).ToShortDateString();
                                }
                                
                            }
                            if (field.DbType == DataType.Time)
                            {
                                if (typedOutput)
                                {
                                    row[field.FQName] = ((TimeSpan)f);
                                }
                                else
                                {
                                    row[field.FQName] = ((TimeSpan)f).ToString(@"hh\:mm");
                                }
                            }
                        }
                    }
                    table.ExtendedProperties["AllRecords"] = int.Parse(reader[i].ToString());
                    table.Rows.Add(row);
                }
            }

            table.ExtendedProperties.Add("sql", debugSql);
            return table;
        }

        public static string GetMultilanguageContent(string fqname, object rawRow)
        {
            DataRow row = null;
            if (rawRow is DataRow)
                row = (DataRow)rawRow;
            if (rawRow is DataRowView)
                row = ((DataRowView)rawRow).Row;

            if (row[fqname] == null || row[fqname] == DBNull.Value)
                return "";

            var contentId = 0;
            int.TryParse(row[fqname].ToString(), out contentId);
            if (contentId < 1)
                return "";
            var content = intContentB.Get(contentId);
            if (content == null || content.Html == null)
                return "";
            return content.Html;
        }

        public static Dictionary<int, string> GetManyToMany(int relationId, int dataKey)
        {
            var result = new Dictionary<int, string>();
            var relation = Schema.GetRelation(relationId);
            if (!relation.IsManyToMany)
                return result;


            var foreignKeyColumns = Schema.ListVirtualColumns(relation.ForeignKeyTableId);

            var filterColumns = foreignKeyColumns.Where(c => SqlHelper.GetTableQualifier(c.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.XrefPrimaryKeyTableName));
            if (filterColumns.Count() != 1)
                throw new Exception("We can't do a N:M join if xref table doesn't contain exactly one primary key.");

            var joinColumns = foreignKeyColumns.Where(c => SqlHelper.GetTableQualifier(c.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));
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

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, sql, p))
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

            var foreignKeys = virtualColumns.Where(c => c.IsPartOfForeignKey && SqlHelper.GetTableQualifier(c.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));

            if (foreignKeys.Count() != 1)
                throw new Exception("Can't do a 1:n join if we don't have a singular foreign key.");

            var sql = "SELECT TOP(1) " + relation.FQPrimaryKeyDisplayColumn +
                      " FROM " + relation.ForeignKeyTableName +
                      " LEFT JOIN " + relation.PrimaryKeySourceTableName + " ON " + relation.FQPrimaryKeyName + "=" + foreignKeys.First().FQName +
                      " WHERE " + foreignKeyTablePrimaryKeys.First().FQName + "= @mainFilter";

            var result = SqlHelper.ExecuteScalar(Schema.ConnectionString, CommandType.Text, sql,
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
            item.FriendlyName= virtualTable.FriendlyName;

            foreach (var column in virtualColumns)
            {
                var editableColumn = (VirtualColumn)column;
                if (column.IsPartOfPrimaryKey && !column.IsPartOfForeignKey)
                {
                    editableColumn.Ordinal = i++;
                    editableColumn.BackendType = FieldType.Display;
                    mainSql += editableColumn.FQName + ", ";
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
                else if (column.IsCreatedField || (column.IsModifiedField))
                {
                    column.Ordinal = i++;
                    mainSql += column.FQName + ", ";
                }
            }

            var relations = Schema.ListRelations(virtualTableId);
            foreach (var relation in relations)
            {
                var foreignKeys = from e in virtualColumns
                                  where
                                      e.IsPartOfForeignKey &&
                                      SqlHelper.GetTableQualifier(e.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.PrimaryKeySourceTableName)
                                  select e;

                if (foreignKeys.Count() != 1)
                    throw new Exception("Multiple foreign key relation currently not supported.");

                mainSql += foreignKeys.First().FQName + ", ";

                var editableColumn = foreignKeys.First();
                editableColumn.PartOfRelationId = relation.Id;
                editableColumn.BackendType = FieldType.OneToMany;
                editableColumn.Ordinal = i++;
                editableColumn.FriendlyName = relation.FriendlyName;
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
                manyToManyColumn.BackendType = relation.IsReverse ? FieldType.ToMany : FieldType.ManyToMany;
                manyToManyColumn.Ordinal = i++;
                manyToManyColumn.IsPartOfUserView = false;
                manyToManyColumn.Name = relation.PrimaryKeyName;
                manyToManyColumn.VirtualTableName = relation.ForeignKeyTableName;
                manyToManyColumn.VirtualTableId = relation.ForeignKeyTableId;

                var keyTable = Schema.GetVirtualTable(relation.ForeignKeyTableId);

                //manyToManyColumn.FriendlyName = relation.FriendlyName;
                manyToManyColumn.FriendlyName = keyTable.FriendlyName;
                if (relation.IsReverse)
                {
                    manyToManyColumn.ForeignKeyColumnName = PhysicalSchema.GetForeignKeyColumnName(virtualTable.StartingPhysicalTable, relation.ForeignKeyTableName);
                    manyToManyColumn.ForeignTableName = relation.ForeignKeyTableName;
                }
                item.Columns.Add(manyToManyColumn.FQName, manyToManyColumn);
            }

            mainSql = mainSql.Substring(0, mainSql.Length - 2);

            if (dataKeys == null)
                return item;

            var sql = "SELECT TOP(2) " + mainSql + " FROM " + virtualTable.StartingPhysicalTable + " WHERE 1=1";
            var parameters = SqlHelper.PreparePrimaryKeyParameters(dataKeys, ref sql);

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, sql, parameters))
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
                                if (item.Columns.ContainsKey(field.FQName))
                                {
                                    item.Columns[field.FQName].ValueIsNull = true;
                                }
                            }
                            else if (field.IsCreatedField || field.IsModifiedField)
                            { 
                                if (field.Name == "principal_created")
                                {
                                    item.PrincipalCreated = (string)f;
                                }
                                if (field.Name == "principal_modified")
                                {
                                    item.PrincipalModified = (string)f;
                                }
                                if (field.Name == "date_modified")
                                {
                                    item.DateModified = (DateTime)f;
                                }
                                if (field.Name == "date_created")
                                {
                                    item.DateCreated = (DateTime)f;
                                }
                            }
                            else
                            {
                                switch (field.DbType)
                                {
                                    case DataType.Int:
                                        item.Columns[field.FQName].ValueInteger = (int)f;
                                        break;
                                    case DataType.DateTime:
                                        item.Columns[field.FQName].ValueDateTime = (DateTime)f;
                                        break;
                                    case DataType.Time:
                                        item.Columns[field.FQName].ValueTime = (TimeSpan)f;
                                        break;
                                    case DataType.Date:
                                        item.Columns[field.FQName].ValueDateTime = (DateTime)f;
                                        break;
                                    case DataType.Decimal:
                                        item.Columns[field.FQName].ValueDecimal = (decimal)f;
                                        break;
                                    case DataType.String:
                                        item.Columns[field.FQName].ValueString = (string)f;
                                        break;
                                    case DataType.Boolean:
                                        item.Columns[field.FQName].ValueBoolean = (bool)f;
                                        break;
                                    case DataType.Double:
                                        item.Columns[field.FQName].ValueDouble = (double)f;
                                        break;
                                    default:
                                        throw new Exception("Unsupported datatype on read: " + item.Columns[field.FQName].DbType);
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

                    var filterColumns = foreignKeyColumns.Where(c => SqlHelper.GetTableQualifier(c.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.XrefPrimaryKeyTableName));
                    if (filterColumns.Count() != 1)
                        throw new Exception("We can't do a N:M join if xref table doesn't contain exactly one primary key.");

                    var joinColumns = foreignKeyColumns.Where(c => SqlHelper.GetTableQualifier(c.PrimaryKeyTableName) == SqlHelper.GetTableQualifier(relation.PrimaryKeySourceTableName));
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

                foreach (var column in virtualTable.VirtualColumns)
                {
                    if (column.IsCreatedField && column.Name == "principal_created")
                    {
                        columnsSql += column.FQName + ", ";
                        valuesSql += "@V" + column.Name + ", ";
                        parameters.Add(new SqlParameter("@V" + column.Name, Thread.CurrentPrincipal.Identity.Name));
                    }
                }

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

                foreach (var column in virtualTable.VirtualColumns)
                {
                    if (column.IsModifiedField)
                    {
                        sql += column.FQName + " = " + "@V" + column.Name + ", ";
                        if (column.Name == "principal_modified")
                        {
                            parameters.Add(new SqlParameter("@V" + column.Name, Thread.CurrentPrincipal.Identity.Name));
                        }
                        else if (column.Name == "date_modified")
                        {
                            parameters.Add(new SqlParameter("@V" + column.Name, DateTime.Now));
                        }
                        else
                        {
                            throw new Exception("wrong modified fields");
                        }
                    }
                }

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
                var primaryKeyParameters = SqlHelper.PreparePrimaryKeyParameters(primaryKeys, ref sql);
                parameters.AddRange(primaryKeyParameters);
            }
            var paramsToPass = parameters.ToArray();
            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, sql, paramsToPass);

            if (isInsert)
            {
                var id = int.Parse(paramsToPass[parameters.Count - 1].Value.ToString());
                primaryKeys[0] = id;
            }
            return result > 0;
        }

        public static DataTable ListItemsForRelation(int relationId, int primaryKey)
        {
            var result = new DataTable();
            var relation = Schema.GetRelation(relationId);

            if (!relation.IsReverse)
                return null;
            var virtualTableId = relation.ForeignKeyTableId;
            var tempState = new ListingState { FirstRecordIndex = 0, RecordsPerPage = 20 };

            var fkTable = Schema.GetVirtualTable(relation.ForeignKeyTableId);

            return ListItems(virtualTableId, tempState, new ForeignKeyFilter { PrimaryKeySourceTableName = relation.PrimaryKeySourceTableName, PrimaryKeyValue = primaryKey });
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

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, sql, new SqlParameter("@Search", search + "%")))
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

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, sql))
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
            var parameters = SqlHelper.PreparePrimaryKeyParameters(dataKeys, ref sql);

            return SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, sql, parameters) > 0;
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

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text, selectSql, p[0]))
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
                SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, insertSql, p);
                //currentXrefs.Remove(foreignKeyValue);
            }

            foreach (var leftOver in deleteXrefs)
            {
                p[1] = new SqlParameter("@join", leftOver);
                SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, deleteSql, p);
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
                        var result = SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text,
                            "SELECT TOP(1) content_fk_id FROM [dbo].[content_data_store] WHERE content_fk_id = @id_content", 
                            new SqlParameter("@id_content", column.ValueInteger));

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
                        c = (c != null ? c : null); //  && !string.IsNullOrEmpty(c.Html)
                        if (c != null) // && !string.IsNullOrEmpty(c.Html)) why are you not letting people blank out a content?
                        {
                            var p = new[]
                                    {
                                        new SqlParameter("@id_content", column.ValueInteger),
                                        new SqlParameter("@id_language", l),
                                        new SqlParameter("@html", c.Html)
                                    };
                            var sql = "SELECT TOP(1) content_fk_id FROM [dbo].[content_data_store] WHERE content_fk_id = @id_content AND language_fk_id = @id_language";
                            var result = SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text, sql, p);

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
