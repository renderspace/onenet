using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MsSqlDBUtility;


namespace One.Net.BLL.DAL
{
    public class DbCategorization 
    {
        private const string SQLPART_CATEGORIES = @"SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
                                    c.principal_modified_by, c.date_modified, c.votes, c.score, c.id, uc.id,
                                    uc.ucategories_fk_id,  uc.is_selectable, uc.ucategorie_type,
                                    CASE  
			                            WHEN CHARINDEX('tree', uc.ucategorie_type) > 0 THEN 
                                            (SELECT count(id) FROM ucategories WHERE ucategories_fk_id=uc.id) 
			                            ELSE NULL
		                            END AS ChildCount, uc.is_private
                            FROM [dbo].[ucategories] uc
                            INNER JOIN [dbo].[content] c ON c.id = uc.content_fk_id ";


        public void ChangeCategory(BOCategory category)
        {
            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = new SqlParameter("@ContentID", category.ContentId.Value);
            paramsToPass[1] = new SqlParameter("@CategoryType", category.Type);
            paramsToPass[2] = SqlHelper.GetNullable("ParentID", category.ParentId);
            paramsToPass[3] = new SqlParameter("@IsSelectable", category.IsSelectable);
            paramsToPass[5] = new SqlParameter("@IsPrivate", category.IsPrivate);

            string sql;

            if (!category.Id.HasValue)
            {
                paramsToPass[4] = new SqlParameter("@id", DBNull.Value);
                paramsToPass[4].Direction = ParameterDirection.Output;
                paramsToPass[4].DbType = DbType.Int32;

                sql = @"INSERT INTO [dbo].[ucategories]     
                        (ucategories_fk_id, content_fk_id, is_selectable, ucategorie_type, is_private)
                        VALUES
                        (@ParentId, @ContentId, @IsSelectable, @CategoryType, @IsPrivate); 
                        SET @Id=SCOPE_IDENTITY();";
            }
            else
            {
                paramsToPass[4] = new SqlParameter("@Id", category.Id);
                sql = @"UPDATE [dbo].[ucategories]
                        SET ucategories_fk_id=@ParentId,
                            content_fk_id=@ContentId,
                            is_selectable=@IsSelectable,
                            is_private=@IsPrivate
                        WHERE id=@Id";
            }

            int affectedRows = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            if (category.Id.HasValue && affectedRows != 1)
            {
                paramsToPass[4].Direction = ParameterDirection.InputOutput;
                paramsToPass[4].DbType = DbType.Int32;
                sql = @"SET identity_insert [dbo].[ucategories] ON; INSERT INTO [dbo].[ucategories]     
                        (id, ucategories_fk_id, content_fk_id, is_selectable, ucategorie_type, is_private)
                        VALUES
                        (@Id, @ParentId, @ContentId, @IsSelectable, @CategoryType, @IsPrivate); 
                        SET @Id=SCOPE_IDENTITY(); SET identity_insert [dbo].[ucategories] OFF;";
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
                // since it did't exist, we erase it (and allow for the next statement to load it)
                category.Id = null;
            }

            if ( !category.Id.HasValue )
            {
                category.Id = Int32.Parse(paramsToPass[4].Value.ToString());
            }
        }

        public void SwapCategorizedItems(BOCategory category, int item1Id, int item2Id)
        {
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@categoryId", category.Id.Value);
            paramsToPass[1] = new SqlParameter("@fk1Id", item1Id);
            paramsToPass[2] = new SqlParameter("@fk2Id", item2Id);
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure,
                "[dbo].[SwapOrderOfCategorizedItems]", paramsToPass);
        }

        public void Categorize(BOCategory category, int categorizedItemId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@categoryId", category.Id);
            paramsToPass[1] = new SqlParameter("@fkId", categorizedItemId);
            paramsToPass[2] = new SqlParameter("@tableName", DBNull.Value);

            switch(category.Type)
            {
                case BOFile.FOLDER_CATEGORIZATION_TYPE:
                    paramsToPass[2].Value = "[dbo].[files]";
                    break;
            }
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, 
                "[dbo].[ChangeUCategorieBelongsTo]", paramsToPass);
        }

        public void RemoveCategorizationFromItem(string categoryType, int categorizedItemId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@categoryType", categoryType);
            paramsToPass[1] = new SqlParameter("@itemID", categorizedItemId);

            string sql = @"DELETE FROM [dbo].[ucategorie_belongs_to]
                           WHERE fkid=@itemID AND ucategories_fk_id IN 
                           (SELECT id FROM [dbo].[ucategories] WHERE ucategorie_type=@categoryType)";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public void RemoveCategorizationFromItem(BOCategory category, int categorizedItemId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@categoryID", category.Id);
            paramsToPass[1] = new SqlParameter("@itemID", categorizedItemId);

            string sql = @"DELETE FROM [dbo].[ucategorie_belongs_to]
                           WHERE fkid=@itemID AND ucategories_fk_id=@categoryID";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public void DeleteCategory(int categoryId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@categoryID", categoryId);

            string sql = @"DELETE FROM [dbo].[ucategories] WHERE id=@categoryID";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public BOCategory Get(int categoryId, bool showUntranslated, int languageId)
        {
            BOCategory category = null;
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@categoryID", categoryId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = SQLPART_CATEGORIES;
            sql += showUntranslated ? "LEFT" : "INNER ";
            sql += " JOIN [dbo].[content_data_store] cds ON c.id= cds.content_fk_id AND language_fk_id =  @languageId";
            sql += @" WHERE uc.id = @categoryID";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    category = PopulateBOCategory(reader, languageId);
                    if (category.IsTreeNode)
                    {
                        category.ChildCount = reader.GetInt32(15);
                    }
                }
            }

            return category;
        }

        public List<BOCategory> ListAll(string categoryType, bool showUntranslated, int languageId)
        {
            List<BOCategory> cats = new List<BOCategory>();

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@categoryType", categoryType);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = SQLPART_CATEGORIES;
            sql += showUntranslated
                       ? "LEFT"
                       : "INNER ";
            sql += " JOIN [dbo].[content_data_store] cds ON c.id= cds.content_fk_id AND language_fk_id =  @languageId";
            sql +=  " WHERE ucategorie_type = @categoryType";
            // sql += " ORDER BY uc.id DESC";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    cats.Add(PopulateBOCategory(reader, languageId));
                }
            }

            return cats;
        }

        public List<BOCategory> ListChildren(int folderId, bool showUntranslated, int languageId)
        {
            List<BOCategory> cats = new List<BOCategory>();

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@folderId", folderId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = SQLPART_CATEGORIES;
            sql += showUntranslated
                       ? "LEFT"
                       : "INNER ";
            sql += " JOIN [dbo].[content_data_store] cds ON c.id= cds.content_fk_id AND language_fk_id =  @languageId";
            sql += " WHERE uc.ucategories_fk_id=@folderId";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    cats.Add(PopulateBOCategory(reader, languageId));
                }
            }

            return cats;
        }

        public List<BOCategory> ListAssignedToItem(string categoryType, int categorizedItemId, bool showUntranslated, int languageId)
        {
            List<BOCategory> cats = new List<BOCategory>();

            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@categoryType", categoryType);
            paramsToPass[1] = new SqlParameter("@categorizedItemID", categorizedItemId);
            paramsToPass[2] = new SqlParameter("@languageId", languageId);

            string sql = SQLPART_CATEGORIES;
            sql +=
                @" INNER JOIN [dbo].[ucategorie_belongs_to] cbt 
                           ON cbt.ucategories_fk_id = uc.id AND cbt.fkid=@categorizedItemID ";
            sql += showUntranslated
                       ? "LEFT"
                       : "INNER ";
            sql += " JOIN [dbo].[content_data_store] cds ON c.id= cds.content_fk_id AND language_fk_id =  @languageId";
            sql += " WHERE ucategorie_type = @categoryType";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    cats.Add(PopulateBOCategory(reader, languageId));
                }
            }
            return cats;
        }

        private static BOCategory PopulateBOCategory(IDataReader reader, int languageId)
        {
            BOCategory category = new BOCategory();
            category.ContentId = reader.GetInt32(10);
            DbHelper.PopulateContent(reader, category, languageId);
            category.Id = reader.GetInt32(11);
            category.ParentId = reader.GetValue(12) != DBNull.Value ? reader.GetInt32(12) : (int?)null;
            category.IsSelectable = reader.GetBoolean(13);
            category.Type = reader.GetString(14);
            category.ChildCount = reader.GetInt32(15);
            category.IsPrivate = reader.GetBoolean(16);
            return category;
        }

        public List<int> ListCategorizedItems(BOCategory category)
        {
            List<int> itemIDs = new List<int>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@categoryID", category.Id);

            string sql = @"SELECT fkid FROM [dbo].[ucategorie_belongs_to] 
                           WHERE ucategories_fk_id = @categoryID ORDER BY idx";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    itemIDs.Add(reader.GetInt32(0));
                }
            }

            return itemIDs;
        }
    }
}
