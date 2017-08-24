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
            DbHelper.CONTENT_SELECT_PART + @" r.content_fk_id, r.human_readable_url, r.id, (SELECT Count(article_fk_id) FROM regular_has_articles WHERE regular_fk_id=r.id AND article_fk_publish=@publishFlag) ArticleCount
                        FROM [dbo].[regular] r 
                        INNER JOIN [dbo].[content] c ON c.id = r.content_fk_id ";

        public void UpgradeArticles()
        {
            var sql = @"IF NOT EXISTS(SELECT * FROM sys.columns 
                        WHERE Name = N'human_readable_url' AND Object_ID = Object_ID(N'article'))
                        BEGIN
                            ALTER TABLE [dbo].[article] ADD human_readable_url varchar(255) NULL
                        END;";
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql);
            sql = @"IF NOT EXISTS(SELECT * FROM sys.columns 
                        WHERE Name = N'human_readable_url' AND Object_ID = Object_ID(N'regular'))
                        BEGIN
                            ALTER TABLE [dbo].[regular] ADD human_readable_url varchar(255) NULL
                        END;";
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql);
        }

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
            if (article == null)
                return;

            if (!article.Id.HasValue || article.Id.Value < 1)
            { 
                var idParam = new SqlParameter();
                idParam.ParameterName = "@sequence_id";
                idParam.Value = 0;
                idParam.Direction = ParameterDirection.Output;
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[nextval]", new SqlParameter("@sequence", "articles"), idParam);
                article.Id = (int) idParam.Value;
                if (article.Id.Value < 1)
                    throw new Exception("Database returned negative next id for articles");
            }

            var exists = ((int) SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain,
                CommandType.Text, 
                "SELECT count(*) FROM [dbo].[article] WHERE id=@id AND publish=@publishFlag", 
                new SqlParameter("@id", article.Id.Value), 
                new SqlParameter("@publishFlag", article.PublishFlag) )
                ) > 0;

            SqlParameter[] paramsToPass = new SqlParameter[8];
            paramsToPass[0] = SqlHelper.GetNullable("@id", article.Id);
            paramsToPass[1] = new SqlParameter("@publishFlag", article.PublishFlag);
            paramsToPass[2] = new SqlParameter("@changed", article.IsChanged);
            paramsToPass[3] = new SqlParameter("@contentId", article.ContentId.Value);
            paramsToPass[4] = new SqlParameter("@markedForDeletion", article.MarkedForDeletion);
            paramsToPass[5] = new SqlParameter("@displayDate", article.DisplayDate);
            paramsToPass[6] = new SqlParameter("@hru", article.HumanReadableUrl);
            paramsToPass[7] = new SqlParameter("@noSingleView", article.NoSingleView);

            var sql = "";
            if (exists)
            {
                sql = @"UPDATE [dbo].[article]
			                    SET changed=@changed, content_fk_id=@contentID, marked_for_deletion=@markedForDeletion, display_date=@displayDate, human_readable_url=@hru, no_single_view=@noSingleView
			                    WHERE id=@id AND publish=@publishFlag";
            }
            else
            {
                sql = @"INSERT INTO [dbo].[article] ( id, publish, changed, content_fk_id, marked_for_deletion, display_date, human_readable_url, no_single_view)
			                VALUES (@id, @publishFlag, @changed, @contentId, @markedForDeletion, @displayDate, @hru, @noSingleView)";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

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

        public BOArticle GetArticle(string humanReadableUrl, bool publishFlag, int languageId) 
        {
            BOArticle article = null;

            var paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@humanReadableUrl", humanReadableUrl);
            paramsToPass[1] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[2] = new SqlParameter("@languageId", languageId);

            string sql = DbHelper.CONTENT_SELECT_PART + @" a.id, a.publish, a.content_fk_id, a.display_date, a.marked_for_deletion, a.changed, a.human_readable_url, a.no_single_view, ";
            if (publishFlag)
                sql += " 1 ";
            else
                sql += @" ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished ";
            sql += @"   FROM [dbo].[article] a
                        INNER JOIN [dbo].[content] c ON c.id = a.content_fk_id ";
            sql += "INNER";
            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = c.id AND cds.language_fk_id=@languageId 
                      WHERE a.human_readable_url=@humanReadableUrl AND a.publish=@publishFlag";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                }
            }

            return article;
        }

        public BOArticle GetArticle(int id, bool publishFlag, int languageId)
        {
            BOArticle article = null;

            var paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@id", id);
            paramsToPass[1] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[2] = new SqlParameter("@languageId", languageId);

            string sql = DbHelper.CONTENT_SELECT_PART + @" a.id, a.publish, a.content_fk_id, a.display_date, a.marked_for_deletion, a.changed, a.human_readable_url, a.no_single_view, ";
            if (publishFlag)
                sql += " 1 ";
            else
                sql += @" ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished ";
            sql += @"   FROM [dbo].[article] a
                        INNER JOIN [dbo].[content] c ON c.id = a.content_fk_id ";
            sql += "INNER";
            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = c.id AND cds.language_fk_id=@languageId 
                      WHERE a.id=@id AND a.publish=@publishFlag";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                }
            }

            return article;
        }

        public DateTime? GetFirstDateWithArticles(bool publishFlag, List<int> regularIDs, int fromYear, int fromMonth, int languageId, bool? excludePast)
        {
            DateTime? result = null;
            string regularIdString = string.Join(",",  regularIDs);
            string sql = @"SELECT MIN(a.display_date) date_day ";

            sql += @"   FROM [dbo].[article] a
		                INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		                INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish
		                INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId 
		                WHERE a.publish = @publishFlag ";

            if (!string.IsNullOrEmpty(regularIdString))
                sql += " AND ra.regular_fk_id IN (" + regularIdString + ") ";

            sql += " AND ((YEAR(a.display_date) = @year AND MONTH(a.display_date) >= @month) OR (YEAR(a.display_date) > @year)) ";

            if (excludePast.HasValue && excludePast.Value)
            {
                sql += " AND a.display_date > getdate() ";
            }

            var paramsToPass = new SqlParameter[4];

            paramsToPass[0] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            paramsToPass[2] = new SqlParameter("@year", fromYear);
            paramsToPass[3] = new SqlParameter("@month", fromMonth);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    result = reader["date_day"] != DBNull.Value ? (DateTime)reader["date_day"] : (DateTime?)null;
                }
            }

            return result;
        }

        public List<BOArticleMonthDay> ListArticleMonthDays(bool publishFlag, List<int> regularIDs, bool showArticleCount, int year, int month, int languageId)
        {
            var results = new List<BOArticleMonthDay>();
            string regularIdString = string.Join(",", regularIDs);
            string sql = @"SELECT DISTINCT CAST(a.display_date AS DATE) date_day";

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

                sql += @" AND YEAR(a2.display_date)=YEAR(a.display_date) AND MONTH(a2.display_date)=MONTH(a.display_date) AND DAY(a2.display_date)=DAY(a.display_date)) cnt ";
            }

            sql += @"      FROM [dbo].[article] a
		                   INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		                   INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish
		                   INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId 
		                   WHERE a.publish = @publishFlag ";

            if (!string.IsNullOrEmpty(regularIdString))
                sql += " AND ra.regular_fk_id IN (" + regularIdString + ")";

            sql += " AND YEAR(a.display_date) = @year AND MONTH(a.display_date) = @month ";

            var paramsToPass = new SqlParameter[4];

            paramsToPass[0] = new SqlParameter("@publishFlag", publishFlag);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            paramsToPass[2] = new SqlParameter("@year", year);
            paramsToPass[3] = new SqlParameter("@month", month);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    var day = (DateTime)reader["date_day"];

                    var d = new BOArticleMonthDay();

                    d.Date = day;

                    if (showArticleCount)
                        d.ArticleCount = (int)reader["cnt"];

                    results.Add(d);
                }
            }

            return results;
        }

        public List<BOArticleMonth> ListArticleMonths(bool publishFlag, List<int> regularIDs, bool showArticleCount, int languageId)
        {
            List<BOArticleMonth> results = new List<BOArticleMonth>();
            string regularIdString = string.Join(",", regularIDs);
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

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
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

        private static PagedList<BOArticle> ListPagedArticles(bool publishFlag, DateTime? from, DateTime? to, List<int> regularIds, ListingState state, int languageId, bool? changed, List<int> excludeRegularIds)
        {
            if (regularIds == null)
                regularIds = new List<int>();
            if (excludeRegularIds == null)
                excludeRegularIds = new List<int>();

            var sortField = state.SortField.Length > 1 ? state.SortField : "id";

            var list = new PagedList<BOArticle>();

            SqlParameter[] paramsToPass = new SqlParameter[5];
            paramsToPass[0] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            paramsToPass[1] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);
            paramsToPass[2] = new SqlParameter("@languageId", languageId);
            paramsToPass[3] = from.HasValue && from.Value != DateTime.MinValue ? new SqlParameter("@dateFrom", from.Value) : new SqlParameter("@dateFrom", DBNull.Value);
            paramsToPass[4] = to.HasValue && to.Value != DateTime.MinValue ? new SqlParameter("@dateTo", to.Value) : new SqlParameter("@dateTo", DBNull.Value);

            string sql = 
            
            @"  SELECT DISTINCT articles.*, ROW_NUMBER() OVER (ORDER BY " + sortField + " " + (state.SortDirection == SortDir.Ascending ? "ASC" : "DESC") +
                @") AS rownum
                INTO #pagedlist 
                FROM (
                    SELECT DISTINCT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
			        c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				    a.display_date, a.marked_for_deletion, a.changed, a.human_readable_url, a.no_single_view, ";

            if (publishFlag)
                sql += " 1 countPublished, ";
            else
                sql += " (select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished, ";

            if (sortField == "random")
                sql += @" NEWID() as random ";
            else
                sql += @" 0 AS random ";

            sql += 
                @"  FROM [dbo].[article] a
		            INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id ";

            if (regularIds.Count > 0)
            {
                sql += @" INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish";
            }
            
            sql += " INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId ";
            sql += " WHERE a.publish = " + (publishFlag ? "1" : "0");

            if (regularIds.Count > 0)
            {
                sql += " AND ra.regular_fk_id IN (" + regularIds.ToCommaSeparatedValues<int>() + ")";
            }

            if (excludeRegularIds.Count > 0)
            {
                sql += @" AND ra.article_fk_id NOT IN (
                SELECT ra2.article_fk_id FROM [dbo].[regular_has_articles] ra2 WHERE ra2.regular_fk_id IN (" + excludeRegularIds.ToCommaSeparatedValues<int>() + ")" +
                ")";
            }

            if (from.HasValue && from.Value != DateTime.MinValue)
                sql += " AND a.display_date >= @dateFrom ";
            if (to.HasValue && to.Value != DateTime.MinValue)
                sql += " AND a.display_date <= @dateTo ";
            if (changed.HasValue)
                sql += " AND a.changed = " + (changed.Value ? "1" : "0");

            sql += @") [articles] ";

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                sql += " ORDER BY " + sortField + " " + (state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            }
            
            sql += @";  SELECT *
						FROM #pagedlist
						WHERE rownum BETWEEN @fromRecordIndex AND @toRecordIndex
						ORDER BY rownum;
						SELECT COUNT(id) FROM #pagedlist";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    var article = new BOArticle();
                    PopulateArticle(reader, article, languageId);
                    list.Add(article);
                }
                if (reader.NextResult() && reader.Read())
                    list.AllRecords = reader.GetInt32(0);
            }

            return list;
        }

        private static void PopulateArticle(IDataReader reader, BOArticle article, int languageId, bool allowNullInHumanReadableUrl = false)
        {
            DbHelper.PopulateContent(reader, article, languageId);

            article.Id = reader.GetInt32(10);
            article.PublishFlag = reader.GetBoolean(11);
            article.ContentId = reader.GetInt32(12);
            article.DisplayDate = reader.GetDateTime(13);
            article.MarkedForDeletion = reader.GetBoolean(14);
            article.IsChanged = reader.GetBoolean(15);
            article.IsNew = reader.GetInt32(17) == 0;

            article.HumanReadableUrl = reader["human_readable_url"] == DBNull.Value ? "" : reader["human_readable_url"].ToString();
            article.NoSingleView = reader["no_single_view"] == DBNull.Value ? false : bool.Parse(reader["no_single_view"].ToString());
        }

        public PagedList<BOArticle> ListFilteredArticles(bool publishFlag, DateTime? from, DateTime? to, ListingState state, int languageId, string titleSearch, List<int> regulars, List<int> excludeRegulars)
        {
            if (regulars == null)
                regulars = new List<int>();
            if (excludeRegulars == null)
                excludeRegulars = new List<int>();

            // TODO: regularIds
            PagedList<BOArticle> list = new PagedList<BOArticle>();

            List<SqlParameter> paramsToPass = new List<SqlParameter>();
            paramsToPass.Add(new SqlParameter("@publishFlag", publishFlag));
            paramsToPass.Add(new SqlParameter("@languageId", languageId));
            paramsToPass.Add(new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex));
            paramsToPass.Add(new SqlParameter("@toRecordIndex", state.DbToRecordIndex));

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
                    date_modified, votes, score, article_id, publish, content_id, display_date, marked_for_deletion, changed, 
                    human_readable_url, no_single_view, countPublished, RowNumber, random)
                    AS
                    (
		                    SELECT	DISTINCT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				                    c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				                    a.display_date, a.marked_for_deletion, a.changed, a.human_readable_url, a.no_single_view,
                                    ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished,
                                    ROW_NUMBER() OVER (";
            if (!string.IsNullOrWhiteSpace(state.SortField))
                sql += "  ORDER BY " + state.SortField + " " + (state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            else
                sql += " ORDER BY a.id";

            sql += @") AS RowNumber, ";

            if (state.SortField == "random")
                sql += " NEWID() as random ";
            else
                sql += " 0 AS random ";

            sql += @"       FROM [dbo].[article] a
		                    INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
                ";
            sql += @" INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId ";

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
                sql += " AND rha.regular_fk_id IN (" + regulars.ToCommaSeparatedValues<int>() + ")";
            }

            if (excludeRegulars.Count > 0)
            {
                sql += @" AND rha.article_fk_id NOT IN (
                SELECT ra2.article_fk_id FROM [dbo].[regular_has_articles] ra2 WHERE ra2.regular_fk_id IN (" + excludeRegulars.ToCommaSeparatedValues<int>() + ")" +
                ")";
            }

            sql += @") SELECT *, (SELECT max(RowNumber) FROM ArticleCTE) AllRecords 
FROM ArticleCTE 
WHERE RowNumber BETWEEN @fromRecordIndex AND @toRecordIndex ";

            list.AllRecords = 0;

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass.ToArray()))
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

        public PagedList<BOArticle> ListArticles(bool publishFlag, DateTime? from, DateTime? to, List<int> regularIds, ListingState state, int languageId, List<int> excludeRegularIds)
        {
            return ListPagedArticles(publishFlag, from, to, regularIds, state, languageId, null, excludeRegularIds);
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
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = SqlHelper.GetNullable("Id", regular.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", regular.ContentId.Value);
            paramsToPass[2] = new SqlParameter("@HumanReadableUrl", regular.HumanReadableUrl);

            string sql;

            if (regular.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[regular] SET content_fk_id=@ContentId, human_readable_url=@HumanReadableUrl WHERE id=@Id";
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"INSERT INTO [dbo].[regular]  (content_fk_id, human_readable_url) VALUES (@ContentId, @HumanReadableUrl); SET @id=SCOPE_IDENTITY();";
            }

            int affectedRows = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            // if regular didn't really exist, retry with insert
            if (regular.Id.HasValue && affectedRows != 1)
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"SET IDENTITY_INSERT [dbo].[regular] ON; INSERT INTO [dbo].[regular]  (content_fk_id, id, human_readable_url) VALUES (@ContentId, @Id, @HumanReadableUrl); SET IDENTITY_INSERT [dbo].[regular] OFF; SET @id=SCOPE_IDENTITY();";
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
            return GetRegular(id, "", showUntranslated);
        }
        
        public BORegular GetRegular(string humanReadableUrl, bool showUntranslated)
        {
            return GetRegular(0, humanReadableUrl, showUntranslated);
        }

        internal BORegular GetRegular(int id, string humanReadableUrl, bool showUntranslated)
        {
            BORegular regular = null;
            var paramsToPass = new SqlParameter[3];
            paramsToPass[0] = id > 0 ? new SqlParameter("@id", id) : new SqlParameter("@humanReadableUrl", humanReadableUrl);
            paramsToPass[1] = new SqlParameter("@languageId", Thread.CurrentThread.CurrentCulture.LCID);
            paramsToPass[2] = SqlHelper.GetNullable("@publishFlag", false);

            string sql = REGULAR_SELECT_PART;
            sql += showUntranslated ? "LEFT" : "INNER";
            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = r.content_fk_id AND cds.language_fk_id=@languageId ";
            sql += id > 0 ? "WHERE r.id=@id" : "WHERE r.human_readable_url=@humanReadableUrl";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    regular = PopulateRegular(reader, Thread.CurrentThread.CurrentCulture.LCID);
                    DbHelper.PopulateContent(reader, regular, Thread.CurrentThread.CurrentCulture.LCID);
                }
            }
            return regular;        
        }

        public List<BORegular> ListArticleRegulars(int articleId, bool publish, int languageId)
        { 
            var paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@languageId", languageId);
            paramsToPass[1] = SqlHelper.GetNullable("@articleId", articleId);
            paramsToPass[2] = SqlHelper.GetNullable("@publishFlag", publish);

            string sql = REGULAR_SELECT_PART;
            sql += @" INNER JOIN [dbo].[regular_has_articles] rha ON rha.regular_fk_id=r.id ";
            sql += @" INNER JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = r.content_fk_id AND cds.language_fk_id=@languageId ";
            sql += @" WHERE rha.article_fk_id=@articleId AND rha.article_fk_publish=@publishFlag ";
            sql += @"   ORDER BY title DESC";

            List<BORegular> regulars = new List<BORegular>();
            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    var regular = PopulateRegular(reader, languageId);
                    regulars.Add(regular);
                }
            }
            return regulars;
        }

        private static BORegular PopulateRegular(IDataReader reader, int languageId)
        {
            var regular = new BORegular();
            regular.ContentId = reader.GetInt32(10);
            DbHelper.PopulateContent(reader, regular, languageId);
            regular.HumanReadableUrl = reader["human_readable_url"] == DBNull.Value ? "" : reader["human_readable_url"].ToString();
            regular.Id = reader.GetInt32(12);
            regular.ArticleCount = reader.GetInt32(13);
            return regular;
        }

        public List<BORegular> ListRegulars(ListingState state, bool publish, bool showUntranslated, int languageId)
        {
            //, bool includeArticleCount
            List<BORegular> regulars = new List<BORegular>();

            if (string.IsNullOrEmpty(state.SortField))
            {
                state.SortField = "id";
            }

            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@sortBy", state.SortField);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            paramsToPass[2] = new SqlParameter("@publishFlag", publish); 
            
            string sql = REGULAR_SELECT_PART;
            sql += showUntranslated ? "LEFT" : "INNER";

            sql += @" JOIN [dbo].[content_data_store] cds 
                      ON cds.content_fk_id = r.content_fk_id AND cds.language_fk_id=@languageId ";

            sql += @"   ORDER BY 
                        CASE @sortBy WHEN 'title' THEN title ELSE NULL END,
                        CASE @sortBy WHEN 'subtitle' THEN subtitle ELSE NULL END,
                        CASE @sortBy WHEN 'principal_created_by' THEN principal_created_by ELSE NULL END,
                        CASE @sortBy WHEN 'principal_modified_by' THEN principal_modified_by ELSE NULL END,
                        CASE @sortBy WHEN 'content_fk_id' THEN r.content_fk_id ELSE NULL END,
                        CASE @sortBy WHEN 'human_readable_url' THEN r.human_readable_url ELSE NULL END,
                        CASE @sortBy WHEN 'id' THEN r.id ELSE NULL END ";

            sql += state.SortDirection == SortDir.Descending ? " DESC" : " ASC";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, 
                CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    var regular = PopulateRegular(reader, languageId);
                    regulars.Add(regular);
                }
            }
            return regulars;
        }
    }
}
