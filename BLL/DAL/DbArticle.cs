using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using MsSqlDBUtility;
using NLog;


namespace One.Net.BLL.DAL
{
    public class DbArticle
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public const string REGULAR_SELECT_PART =
            DbHelper.CONTENT_SELECT_PART + @" r.content_fk_id, r.id, (SELECT Count(article_fk_id) FROM regular_has_articles WHERE regular_fk_id=r.id AND article_fk_publish=@publishFlag) ArticleCount
                        FROM [dbo].[regular] r 
                        INNER JOIN [dbo].[content] c ON c.id = r.content_fk_id ";

        public void DeleteArticle(int id, bool publishFlag)
        {
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@id", id);
            paramsToPass[1] = new SqlParameter("@publishFlag", publishFlag);

            // NB: data from regular_has_articles is delete by ON DELETE CASCADE
            string sql = @"DELETE FROM article 
                           WHERE id=@id 
                           AND publish=@publishFlag";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public void ChangeArticle(BOArticle article)
        {
            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = SqlHelper.GetNullable("@id", article.Id); 
            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].SqlDbType = SqlDbType.Int;

            paramsToPass[1] = new SqlParameter("@publishFlag", article.PublishFlag);
            paramsToPass[2] = new SqlParameter("@changed", article.IsChanged);
            paramsToPass[3] = new SqlParameter("@contentId", article.ContentId.Value);
            paramsToPass[4] = new SqlParameter("@markedForDeletion", article.MarkedForDeletion);
            paramsToPass[5] = new SqlParameter("@displayDate", article.DisplayDate);

            string sql = @"[dbo].[ChangeArticle]";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, sql, paramsToPass);

            article.Id = Int32.Parse(paramsToPass[0].Value.ToString());

            paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@articleId", article.Id.Value);
            paramsToPass[1] = new SqlParameter("@publishFlag", article.PublishFlag);

            sql = @"DELETE FROM [dbo].[regular_has_articles] 
                    WHERE article_fk_id=@articleId AND article_fk_publish=@publishFlag";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            foreach (BORegular regular in article.Regulars)
            {
                paramsToPass = new SqlParameter[3];
                paramsToPass[0] = new SqlParameter("@articleId", article.Id.Value);
                paramsToPass[1] = new SqlParameter("@publishFlag", article.PublishFlag);
                paramsToPass[2] = new SqlParameter("@regularId", regular.Id.Value);

                sql = @"INSERT INTO [dbo].[regular_has_articles] 
                        (regular_fk_id, article_fk_id, article_fk_publish)
                        VALUES 
                        (@regularId, @articleId, @publishFlag)";

                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            }
        }

        public BOArticle GetArticle(int id, bool publishFlag, int languageId, bool showUntranslated)
        {
            BOArticle article = null;

            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@id", id);
            paramsToPass[1] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[2] = new SqlParameter("@languageId", languageId);

            string sql = DbHelper.CONTENT_SELECT_PART + @" a.id, a.publish, a.content_fk_id, a.display_date, a.marked_for_deletion, a.changed,  ";
            if (publishFlag)
                sql += " 1, ";
            else
                sql += @" ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished ";
            sql += @"   FROM [dbo].[article] a
                        INNER JOIN [dbo].[content] c ON c.id = a.content_fk_id ";
            sql += showUntranslated ? "LEFT" : "INNER";
            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = c.id AND cds.language_fk_id=@languageId 
                      WHERE a.id=@id AND a.publish=@publishFlag";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                }
            }

            return article;
        }

        public List<BOArticleMonth> ListArticleMonths(bool publishFlag, List<int> regularIDs, bool showArticleCount, int languageId)
        {
            List<BOArticleMonth> results = new List<BOArticleMonth>();
            string regularIdString = StringTool.RenderAsString(regularIDs);
            string sql = @"SELECT DISTINCT month(a.display_date) date_month, year(a.display_date) date_year";

            if (showArticleCount)
            {
                sql += @",
(SELECT Count(a2.id) FROM [dbo].[article] a2
INNER JOIN [dbo].[content] c ON c.id=a2.content_fk_id
INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a2.id and ra.article_fk_publish=a2.publish
INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId 
WHERE a2.publish = @publishFlag ";

                if (!string.IsNullOrEmpty(regularIdString))
                    sql += @" AND ra.regular_fk_id IN (" + regularIdString + @") ";
                sql += @" AND Month(a2.display_date)=Month(a.display_date) AND Year(a2.display_date)=Year(a.display_date)) cnt ";
            }
		    
            sql += @"      FROM [dbo].[article] a
		                   INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		                   INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish
		                   INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId 
		                   WHERE a.publish = @publishFlag ";
            
            if (!string.IsNullOrEmpty(regularIdString))
                sql += " AND ra.regular_fk_id IN (" + regularIdString + ")";

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    int month = (int)reader["date_month"];
                    int year = (int)reader["date_year"];

                    DateTime date = new DateTime(year, month, 1);
                    BOArticleMonth m = new BOArticleMonth();
                    m.Date = date;

                    if (showArticleCount)
                        m.ArticleCount = (int)reader["cnt"];
                    results.Add(m);
                }
            }

            return results;
        }

        private static PagedList<BOArticle> ListPagedArticles(bool publishFlag, DateTime? from, DateTime? to, string regularIds, ListingState state, int languageId, bool showUntranslated, bool? changed)
        {
            PagedList<BOArticle> list = new PagedList<BOArticle>();

            SqlParameter[] paramsToPass = new SqlParameter[12];
            paramsToPass[0] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            // TODO: remove this from SP, since ListingState calculates from and to, the offset is redundant
            paramsToPass[2] = new SqlParameter("@offSet", 0);
            paramsToPass[3] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            paramsToPass[4] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);
            paramsToPass[5] = state.SortField.Length < 2 ? new SqlParameter("@sortByColumn", DBNull.Value) : new SqlParameter("@sortByColumn", state.SortField);
            paramsToPass[6] = new SqlParameter("@sortOrder", state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            paramsToPass[7] = new SqlParameter("@regularIds", regularIds);
            paramsToPass[8] = from.HasValue && from.Value != DateTime.MinValue ? new SqlParameter("@dateFrom", from.Value) : new SqlParameter("@dateFrom", DBNull.Value);
            paramsToPass[9] = to.HasValue && to.Value != DateTime.MinValue ? new SqlParameter("@dateTo", to.Value) : new SqlParameter("@dateTo", DBNull.Value);
            paramsToPass[10] = new SqlParameter("@showUntranslated", showUntranslated);
            paramsToPass[11] = SqlHelper.GetNullable("@changed", changed);

            string sql = @"[dbo].[ListPagedArticles]";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.StoredProcedure, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BOArticle article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                    list.Add(article);

                    if (list.AllRecords < 1)
                        list.AllRecords = reader.GetInt32(19);
                }
            }

            return list;
        }

        private static void PopulateArticle(IDataReader reader, BOArticle article, int languageId)
        {
            DbHelper.PopulateContent(reader, article, languageId);
            article.Id = reader.GetInt32(10);
            article.PublishFlag = reader.GetBoolean(11);
            article.ContentId = reader.GetInt32(12);
            article.DisplayDate = reader.GetDateTime(13);
            article.MarkedForDeletion = reader.GetBoolean(14);
            article.IsChanged = reader.GetBoolean(15);
            article.IsNew = reader.GetInt32(16) == 0;
        }

        public PagedList<BOArticle> ListUnpublishedArticles(ListingState state, int languageId)
        {
            return ListPagedArticles(false, null, null, "", state, languageId, false, true);
        }

        public PagedList<BOArticle> ListFilteredArticles(bool publishFlag, DateTime? from, DateTime? to, ListingState state, int languageId, bool showUntranslated, string titleSearch, List<int> regulars)
        {
            // TODO: regularIds
            PagedList<BOArticle> list = new PagedList<BOArticle>();

            List<SqlParameter> paramsToPass = new List<SqlParameter>();
            paramsToPass.Add(new SqlParameter("@publishFlag", publishFlag));
            paramsToPass.Add(new SqlParameter("@languageId", languageId));
            paramsToPass.Add(new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex));
            paramsToPass.Add(new SqlParameter("@toRecordIndex", state.DbToRecordIndex));
            paramsToPass.Add(new SqlParameter("@showUntranslated", showUntranslated));

            var searchParam = new SqlParameter("@titleSearch", SqlDbType.NVarChar, 255);
            searchParam.Value = "%" + titleSearch + "%";
            paramsToPass.Add(searchParam);
            
            if (from.HasValue && from.Value != DateTime.MinValue)
                paramsToPass.Add(new SqlParameter("@dateFrom", from));

            if ( to.HasValue && to.Value != DateTime.MinValue )
                paramsToPass.Add(new SqlParameter("@dateTo", to));

            string sql =
                @"
                    ;WITH ArticleCTE(title, subtitle, teaser, html, principal_created_by, date_created, principal_modified_by, 
                    date_modified, votes, score, article_id, publish, content_id, display_date, marked_for_deletion, changed, countPublished, commentCount, RowNumber)
                    AS
                    (
		                    SELECT	cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				                    c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				                    a.display_date, a.marked_for_deletion, a.changed, 
                                    ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished,
                                    ( select count(ct.id) FROM [dbo].[comments] ct WHERE ct.content_fk_id=a.content_fk_id AND ct.publish=a.publish) commentCount,
                                    ROW_NUMBER() OVER (";
            if (state.SortField.Length > 1)
                sql += "  ORDER BY " + state.SortField + " " + (state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            else
                sql += " ORDER BY a.id";

            sql += @") AS RowNumber
		                    FROM [dbo].[article] a
		                    INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
                ";

            if (showUntranslated)
                sql += " LEFT ";
            else
                sql += " INNER ";

            sql += @" JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId ";

            if (regulars.Count > 0)
                sql += @" INNER JOIN [dbo].[regular_has_articles] rha ON rha.article_fk_id = a.id AND rha.article_fk_publish = @publishFlag ";

            sql += @" WHERE a.publish=@publishFlag ";

            #warning TODO: from and to don't work particulary well with other things
            
            if (from.HasValue && to.HasValue)
                sql += @" AND a.display_date BETWEEN @dateFrom AND @dateTo ";
            else if (from.HasValue && from.Value != DateTime.MinValue)
                sql += @" AND a.display_date >= @dateFrom ";
            else if (to.HasValue && to.Value != DateTime.MinValue)
                sql += @" AND a.display_date <= @dateTo ";

            sql += " AND cds.title LIKE @titleSearch ";

            if (regulars.Count > 0)
            {
                sql += " AND rha.regular_fk_id IN (";
                for (int i = 0; i < regulars.Count; i++)
                {
                    sql += regulars[i].ToString();
                    sql += i == (regulars.Count - 1) ? "" : ", ";

                }
                sql += " ) ";
            }
            sql += @") SELECT *, (SELECT max(RowNumber) FROM ArticleCTE) AllRecords 
FROM ArticleCTE 
WHERE RowNumber BETWEEN @fromRecordIndex AND @toRecordIndex ";

            list.AllRecords = 0;

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass.ToArray()))
            {
                
                while (reader.Read())
                {
                    BOArticle article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                    list.Add(article);

                    if (list.AllRecords < 1)
                        list.AllRecords = FormatTool.GetInteger(reader["AllRecords"]);
                }
            }

            return list;            
        }

        public PagedList<BOArticle> ListArticles(bool publishFlag, DateTime? from, DateTime? to, string regularIds, ListingState state, int languageId, bool showUntranslated)
        {
            return ListPagedArticles(publishFlag, from, to, regularIds, state, languageId, showUntranslated, null);
        }

        public void DeleteRegular(int id)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@id", id);

            string sql = @"DELETE FROM regular 
                           WHERE id=@id";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public void ChangeRegular(BORegular regular)
        {
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = SqlHelper.GetNullable("Id", regular.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", regular.ContentId.Value);

            string sql;

            if (regular.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[regular] SET content_fk_id=@ContentId WHERE id=@Id";
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"INSERT INTO [dbo].[regular]  (content_fk_id) VALUES (@ContentId); SET @id=SCOPE_IDENTITY();";
            }

            int affectedRows = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            // if regular didn't really exist, retry with insert
            if (regular.Id.HasValue && affectedRows != 1)
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"SET IDENTITY_INSERT [dbo].[regular] ON; INSERT INTO [dbo].[regular]  (content_fk_id, id) VALUES (@ContentId, @Id); SET IDENTITY_INSERT [dbo].[regular] OFF; SET @id=SCOPE_IDENTITY();";
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
                // since it did't exist, we erase it (and allow for the next statement to load it)
                regular.Id = null;
            }

            if (!regular.Id.HasValue)
            {
                regular.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public BORegular GetRegular(int id, bool showUntranslated )
        {
            BORegular regular = null;

            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@id", id);
            paramsToPass[1] = new SqlParameter("@languageId", Thread.CurrentThread.CurrentCulture.LCID);
            paramsToPass[2] = SqlHelper.GetNullable("@publishFlag", false);


            string sql = REGULAR_SELECT_PART;
            sql += showUntranslated ? "LEFT" : "INNER";
            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = r.content_fk_id AND cds.language_fk_id=@languageId 
                      WHERE r.id=@id";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    regular = new BORegular();
                    regular.ContentId = reader.GetInt32(10);
                    DbHelper.PopulateContent(reader, regular, Thread.CurrentThread.CurrentCulture.LCID);
                    regular.Id = reader.GetInt32(11);
                    regular.ArticleCount = reader.GetInt32(12);
                }
            }

            return regular;
        }

        public List<BORegular> ListRegulars(ListingState state, bool showUntranslated, int? articleId, bool? publish, int languageId)
        {
            List<BORegular> regulars = new List<BORegular>();

            if (string.IsNullOrEmpty(state.SortField))
            {
                state.SortField = "id";
            }

            SqlParameter[] paramsToPass = new SqlParameter[4];
            paramsToPass[0] = new SqlParameter("@sortBy", state.SortField);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            paramsToPass[2] = SqlHelper.GetNullable("@articleId", articleId);
            paramsToPass[3] = SqlHelper.GetNullable("@publishFlag", publish);
            
            string sql = REGULAR_SELECT_PART;

            if ( articleId.HasValue && publish.HasValue )
                sql += @" INNER JOIN [dbo].[regular_has_articles] rha ON rha.regular_fk_id=r.id ";

            sql += showUntranslated ? "LEFT" : "INNER";

            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = r.content_fk_id AND cds.language_fk_id=@languageId ";

            if (articleId.HasValue && publish.HasValue)
                sql += @" WHERE rha.article_fk_id=@articleId AND rha.article_fk_publish=@publishFlag ";

            sql += @"   ORDER BY 
                        CASE @sortBy WHEN 'title' THEN title ELSE NULL END,
                        CASE @sortBy WHEN 'subtitle' THEN subtitle ELSE NULL END,
                        CASE @sortBy WHEN 'principal_created_by' THEN principal_created_by ELSE NULL END,
                        CASE @sortBy WHEN 'principal_modified_by' THEN principal_modified_by ELSE NULL END,
                        CASE @sortBy WHEN 'content_fk_id' THEN r.content_fk_id ELSE NULL END,
                        CASE @sortBy WHEN 'id' THEN r.id ELSE NULL END ";

            sql += state.SortDirection == SortDir.Descending ? " DESC" : " ASC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, 
                CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BORegular regular = new BORegular();
                    regular.ContentId = reader.GetInt32(10);
                    DbHelper.PopulateContent(reader, regular, languageId);
                    regular.Id = reader.GetInt32(11);
                    regular.ArticleCount = reader.GetInt32(12);
                    regulars.Add(regular);
                }
            }

            return regulars;
        }
    }
}
