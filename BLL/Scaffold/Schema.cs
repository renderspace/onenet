using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using One.Net.BLL.Scaffold.Model;
using MsSqlDBUtility;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;

namespace One.Net.BLL.Scaffold
{
    public static class Schema
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Bamboo"].ConnectionString; }
        }
        
        public static void InternationalizeColumn(int columnId)
        {
            var virtualTable = Schema.GetVirtualColumnTable(columnId);
            if (virtualTable != null)
            {
                var virtualColumn = GetVirtualColumn(columnId, virtualTable);
                if (virtualColumn != null)
                {
                    var tableName = virtualTable.StartingPhysicalTable;
                    if (!virtualColumn.IsMultiLanguageContent && virtualColumn.DbType.ToString() == "System.String")
                    {
                        var oldColumnName = virtualColumn.Name.ToLower();

                        if (oldColumnName.StartsWith("id_"))
                        {
                            int index = oldColumnName.IndexOf("id_");
                            oldColumnName = oldColumnName.Remove(index, "id_".Length);
                        }

                        var newColumnName = "id_" + oldColumnName;

                        var sql = @"SELECT COUNT(*) ct FROM sys.columns 
                                WHERE [name] = N'" + newColumnName + @"' AND [object_id] = OBJECT_ID(N'" + tableName + @"')";

                        // Check if a physical id_column_name column exists. If not, create it.
                        var columnCount = Int32.Parse((SqlHelper.ExecuteScalar(Schema.ConnectionString, CommandType.Text, sql)).ToString());
                        if (columnCount == 0)
                        {
                            sql = @"ALTER TABLE " + tableName + @"
                                ADD " + newColumnName + @" INT NULL;";

                            SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql);
                        }

                        // Once a physical column is created, check that the virtual column is created too

                        var newVirtualColumn = new VirtualColumn();
                        var existingVirtualColumn = Schema.FindVirtualColumn(newColumnName, virtualTable);
                        if (existingVirtualColumn == null)
                        {
                            var physicalColumn = ((from p in PhysicalSchema.ListPhysicalColumns(virtualTable.Id) where p.Name == newColumnName select p)).First();

                            newVirtualColumn.Name = newColumnName;
                            newVirtualColumn.VirtualTableId = virtualTable.Id;
                            newVirtualColumn.DbType = physicalColumn.DbType;
                            newVirtualColumn.IsWysiwyg = virtualColumn.IsWysiwyg;
                            newVirtualColumn.IsMultiLanguageContent = true;
                            newVirtualColumn.ShowOnList = virtualColumn.ShowOnList;

                            Schema.ChangeVirtualColumn(newVirtualColumn);
                        }
                        else
                        {
                            newVirtualColumn = existingVirtualColumn;
                        }

                        var columns = Schema.ListVirtualColumns(virtualTable.Id);

                        // Re-enter the internationalized content. Make sure to only create content if the new column entry is not set yet, and then store the newly created content id in the new column entry.
                        sql = @"SELECT * FROM " + tableName;
                        
                        var intContentB = new BInternalContent();

                        var primaryKeyColumn = "";
                        foreach (var column in columns)
                        {
                            if (column.IsPartOfPrimaryKey)
                            {
                                primaryKeyColumn = column.Name;
                            }
                        }

                        if (string.IsNullOrEmpty(primaryKeyColumn))
                            throw new Exception("Cannot internationalize column of table that does not have a primary key.");
                        
                        using (var reader = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql))
                        {
                            while (reader.Read())
                            {
                                var primaryKeyColumnValue = reader[primaryKeyColumn].ToString();

                                // check if a content id is already entered in the new column and only proceed if not.
                                var newContentId = reader[newColumnName] == DBNull.Value ? 0 : (int)reader[newColumnName];
                                if (newContentId == 0)
                                {
                                    var c = new BOInternalContent();
                                    c.Html = reader[oldColumnName] == DBNull.Value ? "" : reader[oldColumnName].ToString();
                                    c.ContentId = null;
                                    c.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                    intContentB.Change(c);

                                    var prms = new SqlParameter[2];
                                    prms[0] = new SqlParameter("@newValue", c.ContentId.Value);
                                    prms[1] = new SqlParameter("@primaryKeyColumnValue", primaryKeyColumnValue);
                                    sql = @"UPDATE " + tableName + " SET " + newColumnName + "=@newValue WHERE " + primaryKeyColumn + "=@primaryKeyColumnValue";

                                    SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql, prms);
                                }
                            }
                        }

                        // finally, remove old column from virtual columns
                        Schema.RemoveVirtualColumn(columnId);

                    }
                }
            }
        }

        public static List<VirtualTable> ListVirtualTables()
        {
            var result = new List<VirtualTable>();
            using (var reader = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text,
                "SELECT id, starting_table, order_col, show_on_menu, condition, friendly_name, has_pager FROM _virtual_table"))
            {
                while (reader.Read())
                {
                    var virtualTable = new VirtualTable();
                    virtualTable.Id = (int)reader["id"];
                    virtualTable.StartingPhysicalTable = (string)reader["starting_table"];
                    virtualTable.OrderColumn = (string)reader["order_col"];
                    virtualTable.ShowOnMenu = (bool)reader["show_on_menu"];
                    virtualTable.Condition = (string)reader["condition"];
                    virtualTable.FriendlyName = (string)reader["friendly_name"];
                    virtualTable.HasPager = (bool)reader["has_pager"];
                    result.Add(virtualTable);
                }
            }
            return result;
        }

        public static VirtualTable GetVirtualTable(int virtualTableId)
        {
            VirtualTable virtualTable = null;
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                                   "SELECT starting_table, show_on_menu, order_col, condition, friendly_name, has_pager FROM _virtual_table WHERE id = @virtualTableId",
                                   new SqlParameter("@virtualTableId", virtualTableId)))
            {
                if (reader.Read())
                {
                    virtualTable = new VirtualTable
                    {
                        Id = virtualTableId,
                        StartingPhysicalTable = (string)reader["starting_table"],
                        OrderColumn = (string)reader["order_col"],
                        ShowOnMenu = (bool)reader["show_on_menu"],
                        Condition = (string)reader["condition"],
                        FriendlyName = (string)reader["friendly_name"],
                        HasPager = (bool)reader["has_pager"]
                    };
                    virtualTable.PrimaryKeys = ListPrimaryKeys(virtualTable.StartingPhysicalTable);
                }
            }

            if (virtualTable != null)
            {
                virtualTable.VirtualColumns = PhysicalSchema.ListPhysicalColumns(virtualTable);
                ListVirtualColumns(virtualTable);
                virtualTable.Relations = ListRelations(virtualTable.Id);
            }
            return virtualTable;
        }

        private static List<string> ListPrimaryKeys(string tableIdentifier)
        {
            var primaryKeys = new List<string>();

            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.StoredProcedure,
                "sp_pkeys", new SqlParameter("@table_name", SqlHelper.GetTableQualifier(tableIdentifier))))
            {
                while (reader.Read())
                {
                    primaryKeys.Add(reader.GetString(3));
                }
            }
            return primaryKeys;
        }

        // expects that virtual table is already populated with physical columns
        private static void ListVirtualColumns(VirtualTable virtualTable)
        {
            if (virtualTable.VirtualColumns.Count() > 0)
            {
                var physicalColumns = virtualTable.VirtualColumns;
                using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                    "SELECT id, col_name, form_type, friendly_name, is_multilanguage_content, show_on_list, is_wysiwyg FROM _virtual_col WHERE virtual_table_id = @Id", new SqlParameter("@Id", virtualTable.Id)))
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["id"];
                        var name = (string)reader["col_name"];

                        var virtualColumns = ((from p in physicalColumns where p.Name == name select p));
                        if (virtualColumns.Count() != 1)
                            throw new Exception("Physical column " + name + " id[" + id + "] in table " + virtualTable.FriendlyName + " missing. TODO: autmatically delete it from list.");

                        var virtualColumn = virtualColumns.First();
                        virtualColumn.Id = id;
                        virtualColumn.FriendlyName = (string)reader["friendly_name"];
                        virtualColumn.IsMultiLanguageContent = bool.Parse(reader["is_multilanguage_content"].ToString());
                        virtualColumn.IsWysiwyg = bool.Parse(reader["is_wysiwyg"].ToString());
                        // virtualColumn.Name = (string)reader["form_type"];
                        // if we have a column in above select, then it is part of userview:
                        virtualColumn.IsPartOfUserView = true;

                        virtualColumn.ShowOnList = (bool)reader["show_on_list"];
                    }
                }
            }
        }
        
        private static VirtualColumn FindVirtualColumn(string virtualColumnName, VirtualTable virtualTable)
        {
            ListVirtualColumns(virtualTable);
            var foundColumns = ((from vc in virtualTable.VirtualColumns where vc.Name == virtualColumnName && vc.IsPartOfUserView select vc));
            return foundColumns.Count<VirtualColumn>() > 0 ? foundColumns.First() : null;
        }

        public static VirtualColumn GetVirtualColumn(int virtualColumnId, VirtualTable virtualTable)
        {
            ListVirtualColumns(virtualTable);
            var foundColumns = ((from vc in virtualTable.VirtualColumns where vc.Id == virtualColumnId select vc));
            return foundColumns.FirstOrDefault();
        }
        
        public static VirtualTable GetVirtualColumnTable(int virtualColumnId)
        {
            VirtualTable virtualTable = null;
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                    @"  SELECT virtual_table_id
                        FROM _virtual_col 
                        WHERE id = @virtualColumnId", new SqlParameter("@virtualColumnId", virtualColumnId)))                                   
            {
                if (reader.Read())
                {
                    virtualTable = GetVirtualTable((int)reader["virtual_table_id"]);
                }
            }

            return virtualTable;
        }

        /// TODO: redundant!!!!
        internal static List<VirtualColumn> ListVirtualColumns(int virtualTableId)
        {
            var physicalColumns = PhysicalSchema.ListPhysicalColumns(virtualTableId);
            if (physicalColumns.Count() > 0)
            {
                using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                    "SELECT id, col_name, form_type, friendly_name, is_multilanguage_content, show_on_list, is_wysiwyg FROM _virtual_col WHERE virtual_table_id = @Id", new SqlParameter("@Id", virtualTableId)))
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["id"];
                        var name = (string)reader["col_name"];
                        var virtualColumn = ((from p in physicalColumns where p.Name == name select p)).First();
                        virtualColumn.Id = id;
                        virtualColumn.FriendlyName = (string)reader["friendly_name"];
                        virtualColumn.IsMultiLanguageContent = bool.Parse(reader["is_multilanguage_content"].ToString());
                        virtualColumn.IsWysiwyg = bool.Parse(reader["is_wysiwyg"].ToString());
                        // virtualColumn.Name = (string)reader["form_type"];
                        // if we have a column in above select, then it is part of userview:
                        virtualColumn.IsPartOfUserView = true;

                        virtualColumn.ShowOnList = (bool)reader["show_on_list"];
                    }
                }
            }
            return physicalColumns;
        }

        public static bool DeleteVirtualTable(int id)
        {
            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text,
                                                   "DELETE FROM _virtual_table WHERE id = @Id",
                                                   new SqlParameter("@Id", id));
            return result > 0;
        }

        public static bool ChangeVirtualTable(VirtualTable virtualTable)
        {
            var p = new[]
			{
                new SqlParameter("@Id", virtualTable.Id),
				new SqlParameter("@StartingTable", virtualTable.StartingPhysicalTable),
				new SqlParameter("@OrderColumn", virtualTable.OrderColumn),
                new SqlParameter("@ShowOnMenu", virtualTable.ShowOnMenu),
                new SqlParameter("@Condition", virtualTable.Condition),
                new SqlParameter("@FriendlyName", virtualTable.FriendlyName),
                new SqlParameter("@HasPager", virtualTable.HasPager)
			};

            if (virtualTable.Id == 0)
                p[0].Direction = ParameterDirection.Output;

            var sql = virtualTable.Id > 0
                          ? "UPDATE _virtual_table SET starting_table = @StartingTable, order_col = @OrderColumn, show_on_menu = @ShowOnMenu, condition = @Condition, friendly_name = @FriendlyName, has_pager = @HasPager  WHERE id = @Id"
                          : "INSERT INTO _virtual_table (starting_table,order_col,show_on_menu,condition, friendly_name, has_pager) VALUES (@StartingTable, @OrderColumn,@ShowOnMenu,@Condition,@FriendlyName,@HasPager); SET @Id = (SELECT @@IDENTITY) ";

            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, sql, p);

            if (virtualTable.Id == 0)
                virtualTable.Id = int.Parse(p[0].Value.ToString());

            return result > 0;
        }

        public static bool ChangeVirtualColumn(VirtualColumn virtualColumn)
        {
            var p = new[]
			{
                new SqlParameter("@Id", virtualColumn.Id),
				new SqlParameter("@VirtualTableId", virtualColumn.VirtualTableId),
				new SqlParameter("@Name", virtualColumn.Name),
                // TODO: look below
				new SqlParameter("@FormType", virtualColumn.DbType.ToString()),
                new SqlParameter("@IsMultilanguageContent", virtualColumn.IsMultiLanguageContent),
                new SqlParameter("@ShowOnList", virtualColumn.ShowOnList),
                new SqlParameter("@FriendlyName", virtualColumn.FriendlyName),
                new SqlParameter("@Wysiwyg", virtualColumn.IsWysiwyg)
			};

            if (virtualColumn.Id == 0)
                p[0].Direction = ParameterDirection.Output;

            var sql = virtualColumn.Id > 0
                          ? "UPDATE _virtual_col SET is_wysiwyg = @Wysiwyg, friendly_name = @FriendlyName, virtual_table_id = @VirtualTableId, col_name = @Name, form_type = @FormType, is_multilanguage_content = @IsMultilanguageContent, show_on_list=@ShowOnList WHERE id = @Id"
                          : "INSERT INTO _virtual_col (is_wysiwyg, friendly_name, virtual_table_id,col_name,form_type,is_multilanguage_content, show_on_list) VALUES (@Wysiwyg, @FriendlyName, @VirtualTableId, @Name, @FormType, @IsMultilanguageContent, @ShowOnList); SET @Id = (SELECT @@IDENTITY) ";

            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text, sql, p);

            if (virtualColumn.Id == 0)
                virtualColumn.Id = int.Parse(p[0].Value.ToString());

            return result > 0;
        }

        public static bool ChangeRelation(Relation relation)
        {
            var p = new[]
			{
                new SqlParameter("@Id", relation.Id),
				new SqlParameter("@FriendlyName", relation.FriendlyName),
                new SqlParameter("@ShowOnList", relation.ShowOnList)
			};

            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text,
             "UPDATE _virtual_relation SET friendly_name = @FriendlyName, show_on_list = @ShowOnList WHERE id = @Id", p);

            return result > 0;
        }

        public static bool AddRelation(int primaryTableId, int foreignKeySourceTableId, string foreignKeyDisplayColumn)
        {
            if (foreignKeySourceTableId == primaryTableId)
                throw new Exception("Can to relation to oneself (at the moment).");

            var p = new[]
			{
                new SqlParameter("@PrimaryTableId", primaryTableId),
				new SqlParameter("@ForeignKeySourceTableId", foreignKeySourceTableId),
                new SqlParameter("@ForeignKeyDisplayColumn", foreignKeyDisplayColumn)
			};

            if (((int)SqlHelper.ExecuteScalar(Schema.ConnectionString, CommandType.Text,
                "SELECT COUNT(*) FROM _virtual_relation WHERE pk_virtual_table_id = @PrimaryTableId AND fk_virtual_table_id = @ForeignKeySourceTableId", p)) > 0)
            {
                // relationship already exists
                return false;
            }

            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text,
               @"INSERT INTO _virtual_relation (pk_virtual_table_id, fk_virtual_table_id, pk_display_col) 
                VALUES (@PrimaryTableId, @ForeignKeySourceTableId, @ForeignKeyDisplayColumn) ",
               p);

            return result > 0;
        }

        public static bool RemoveRelation(int primaryTableId, int foreignKeySourceTableId)
        {
            var p = new[]
			{
                new SqlParameter("@PrimaryTableId", primaryTableId),
				new SqlParameter("@ForeignKeySourceTableId", foreignKeySourceTableId)
			};

            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text,
               "DELETE FROM _virtual_relation WHERE pk_virtual_table_id = @PrimaryTableId AND fk_virtual_table_id = @ForeignKeySourceTableId",
               p);

            return result > 0;
        }

        public static bool RemoveVirtualColumn(int virtualColumnId)
        {
            var result = SqlHelper.ExecuteNonQuery(Schema.ConnectionString, CommandType.Text,
               "DELETE FROM [dbo].[_virtual_col] WHERE id = @virtualColumnId",
               new SqlParameter("@virtualColumnId", virtualColumnId));
            return result > 0;
        }

        public static Relation GetRelation(int relationId)
        {
            Relation result = null;
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                @"SELECT vr.id, vr.fk_virtual_table_id, vt1.starting_table AS fk_virtual_table, vr.pk_virtual_table_id, vt2.starting_table AS pk_virtual_table, pk_display_col, vt3.starting_table AS xref_pk_virtual_table, vr.pk_xref_virtual_table_id, vr.is_multilanguage_content, is_reverse, vr.friendly_name, vr.show_on_list
                FROM _virtual_relation vr
                INNER JOIN _virtual_table vt1 ON  vt1.id = vr.fk_virtual_table_id
                INNER JOIN _virtual_table vt2 ON  vt2.id = vr.pk_virtual_table_id
                LEFT JOIN _virtual_table vt3 ON  vt3.id = vr.pk_xref_virtual_table_id
                WHERE vr.id = @RelationId", new SqlParameter("@RelationId", relationId)))
            {
                if (reader.Read())
                {
                    result = PopulateRelation(reader);
                }
            }
            return result;
        }

        public static List<Relation> ListManyToManyRelations(int xrefPrimaryKeyVirtualTableId)
        {
            var result = new List<Relation>();
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                @"SELECT vr.id, vr.fk_virtual_table_id, vt1.starting_table AS fk_virtual_table, vr.pk_virtual_table_id, vt2.starting_table AS pk_virtual_table, pk_display_col, vt2.starting_table AS xref_pk_virtual_table, vr.pk_xref_virtual_table_id, vr.is_multilanguage_content, is_reverse, vr.friendly_name, vr.show_on_list
                FROM _virtual_relation vr
                INNER JOIN _virtual_table vt1 ON  vt1.id = vr.fk_virtual_table_id
                INNER JOIN _virtual_table vt2 ON  vt2.id = vr.pk_virtual_table_id
                LEFT JOIN _virtual_table vt3 ON  vt3.id = vr.pk_xref_virtual_table_id
                WHERE (vr.pk_xref_virtual_table_id = @PrimaryTableId AND is_reverse = 0) OR (vr.pk_virtual_table_id = @PrimaryTableId AND is_reverse = 1) ", new SqlParameter("@PrimaryTableId", xrefPrimaryKeyVirtualTableId)))
            {
                while (reader.Read())
                {
                    var relation = PopulateRelation(reader);
                    result.Add(relation);
                }
            }
            return result;
        }

        public static List<Relation> ListRelations(int primaryVirtualTableId)
        {
            var result = new List<Relation>();
            using (var reader = SqlHelper.ExecuteReader(Schema.ConnectionString, CommandType.Text,
                @"SELECT vr.id, vr.fk_virtual_table_id, vt1.starting_table AS fk_virtual_table, vr.pk_virtual_table_id, vt2.starting_table AS pk_virtual_table, pk_display_col, NULL AS xref_pk_virtual_table, NULL AS pk_xref_virtual_table_id, vr.is_multilanguage_content, is_reverse, vr.friendly_name, vr.show_on_list
                FROM _virtual_relation vr
                INNER JOIN _virtual_table vt1 ON  vt1.id = vr.fk_virtual_table_id
                INNER JOIN _virtual_table vt2 ON  vt2.id = vr.pk_virtual_table_id
                WHERE vr.fk_virtual_table_id = @PrimaryTableId  AND vr.[pk_xref_virtual_table_id] IS NULL", new SqlParameter("@PrimaryTableId", primaryVirtualTableId)))
            {
                while (reader.Read())
                {
                    var relation = (Relation)PopulateRelation(reader);
                    result.Add(relation);
                }
            }
            return result;
        }

        private static Relation PopulateRelation(IDataRecord reader)
        {
            var relation = new Relation
            {
                Id = ((int)reader["id"]),
                ForeignKeyTableId = ((int)reader["fk_virtual_table_id"]),
                PrimaryKeySourceTableId = ((int)reader["pk_virtual_table_id"]),
                ForeignKeyTableName = ((string)reader["fk_virtual_table"]),
                PrimaryKeySourceTableName = ((string)reader["pk_virtual_table"]),
                PrimaryKeyDisplayColumn = (string)reader["pk_display_col"],
                XrefPrimaryKeyTableName = reader["xref_pk_virtual_table"] == DBNull.Value ? null : (string)reader["xref_pk_virtual_table"],
                XrefPrimaryKeyTableId = reader["pk_xref_virtual_table_id"] == DBNull.Value ? 0 : ((int)reader["pk_xref_virtual_table_id"]),
                IsMultiLanguageContent = (bool)reader["is_multilanguage_content"],
                IsReverse = (bool)reader["is_reverse"],
                FriendlyName = (string)reader["friendly_name"],
                ShowOnList = (bool)reader["show_on_list"]
            };


            var primaryKeysOnForeignKeySourcTable = ListPrimaryKeys(relation.PrimaryKeySourceTableName);

            if (primaryKeysOnForeignKeySourcTable.Count == 0)
                throw new Exception("No primary key found in primary key source table. " + relation.PrimaryKeySourceTableName ?? "And null PrimaryKeySourceTableName.");
            else if (primaryKeysOnForeignKeySourcTable.Count != 1)
                throw new Exception("Multiple primary key relations not supported.");
            else
                relation.PrimaryKeyName = primaryKeysOnForeignKeySourcTable[0];
            return relation;
        }
    }
}
