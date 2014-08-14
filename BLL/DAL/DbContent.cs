using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using MsSqlDBUtility;

namespace One.Net.BLL.DAL
{
    public class DbContent
    {
        public void Delete(int contentId)
        {
            // Content datastore should have ON DELETE CASCADE
            SqlParameter paramsToPass = new SqlParameter("@contentId", contentId);
            SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text,
                @"DELETE FROM [dbo].[content] WHERE id = @contentId", paramsToPass);
        }

        public BOInternalContent GetContent(int contentID, int languageID)
        {
            BOInternalContent content = null;

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@contentID", contentID);
            paramsToPass[1] = new SqlParameter("@languageID", languageID);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, c.principal_modified_by, c.date_modified, c.votes, c.score
                    FROM [dbo].[content] c
                    INNER JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageID
                    WHERE c.id = @contentID", paramsToPass))
            {
                if (reader.Read())
                {
                    content = new BOInternalContent();
                    content.ContentId = contentID;
                    DbHelper.PopulateContent(reader, content, languageID);
                }
            }

            return content;
        }

        public string GetContentTitleInAnyLanguage(int contentId)
        {
            object result = SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT TOP(1) title FROM [dbo].[content_data_store] WHERE content_fk_id = @contentId ", 
                new SqlParameter("@contentID", contentId));

            if (result != null)
                return (string) result;
            else
                return null;
        }

        public void SaveVote(int votedScore, int contentId)
        {
            if (!(contentId > 0) || votedScore > 5 || votedScore < 1)
                throw new ApplicationException("Trying to VOTE on uncomplete BOInternalContent OR score out of range");
            else
            {
                SqlParameter[] paramsToPass = new SqlParameter[2];
                paramsToPass[0] = new SqlParameter("@votedScore", votedScore);
                paramsToPass[1] = new SqlParameter("@contentId", contentId);
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure,
                                          @"[dbo].[Vote]", paramsToPass);
            }
        }

        public void ChangeContent(BOInternalContent content, string connString = "")
        {
            if (!content.IsComplete)
                throw new ApplicationException("Trying to save uncomplete BOInternalContent.");
            else
            {
                SqlParameter[] paramsToPass = new SqlParameter[9];
                paramsToPass[0] = SqlHelper.GetNullable("contentID", content.ContentId);
                paramsToPass[1] = new SqlParameter("@languageID", content.LanguageId);
                paramsToPass[2] = new SqlParameter("@title", content.Title);
                paramsToPass[3] = new SqlParameter("@subtitle", content.SubTitle);
                paramsToPass[4] = new SqlParameter("@teaser", content.Teaser);
                paramsToPass[5] = new SqlParameter("@html", content.Html);
                paramsToPass[6] = new SqlParameter("@principal", Thread.CurrentPrincipal.Identity.Name);
                paramsToPass[7] = SqlHelper.GetNullable("score", 0);
                paramsToPass[8] = new SqlParameter("@votes", 0);

                object result = SqlHelper.ExecuteScalar(string.IsNullOrWhiteSpace(connString) ? SqlHelper.ConnStringMain : connString, CommandType.StoredProcedure, "[ChangeContent]", paramsToPass);

                if (content.ContentId == null) // Inserted
                {
                    content.ContentId = (int)result;
                    content.PrincipalCreated = Thread.CurrentPrincipal.Identity.Name;
                    content.DateCreated = DateTime.Now;
                }
                else // Updated
                {
                    content.PrincipalModified = Thread.CurrentPrincipal.Identity.Name;
                    content.DateModified = DateTime.Now;
                }
            }
        }

        public void AuditContent(BOInternalContent content)
        {
            SqlParameter[] paramsToPass = new SqlParameter[8];
            paramsToPass[0] = new SqlParameter("@contentId", content.ContentId);
            paramsToPass[1] = new SqlParameter("@languageId", content.LanguageId);
            paramsToPass[2] = new SqlParameter("@title", content.Title);
            paramsToPass[3] = new SqlParameter("@subtitle", content.SubTitle);
            paramsToPass[4] = new SqlParameter("@teaser", content.Teaser);
            paramsToPass[5] = new SqlParameter("@html", content.Html);
            paramsToPass[6] = new SqlParameter("@principal", Thread.CurrentPrincipal.Identity.Name);
            paramsToPass[7] = new SqlParameter("@guid", System.Guid.NewGuid());

            string sql = @"INSERT INTO [dbo].[content_data_store_audit]
                           (guid, content_fk_id, language_fk_id, title, subtitle, teaser, html, principal_saved_by)
                           VALUES
                           (@guid, @contentId, @languageId, @title, @subtitle, @teaser, @html, @principal)";

            SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public BOInternalContentAudit GetAudit(string guid)
        {
            BOInternalContentAudit audit = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@guid", guid);

            string sql = @"SELECT guid, content_fk_id, language_fk_id, title, subtitle, teaser, html, principal_saved_by, date_saved
                           FROM [dbo].[content_data_store_audit]
                           WHERE guid=@guid";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    audit = new BOInternalContentAudit();
                    PopulateContentAudit(reader, audit);
                }
            }

            return audit;
        }

        public List<BOInternalContentAudit> ListAudits(int contentId, int languageId)
        {
            List<BOInternalContentAudit> list = new List<BOInternalContentAudit>();

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@contentId", contentId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = @"SELECT guid, content_fk_id, language_fk_id, title, subtitle, teaser, html, principal_saved_by, date_saved
                           FROM [dbo].[content_data_store_audit]
                           WHERE content_fk_id=@contentId AND language_fk_id=@languageId
                           ORDER BY date_saved DESC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BOInternalContentAudit audit = new BOInternalContentAudit();
                    PopulateContentAudit(reader, audit);
                    list.Add(audit);
                }
            }

            return list;
        }

        private static void PopulateContentAudit(IDataRecord reader, BOInternalContentAudit audit)
        {
            audit.AuditGuid = reader.GetString(0);
            audit.ContentId = reader.GetInt32(1);
            audit.LanguageId = reader.GetInt32(2);
            audit.Title = reader.GetString(3);
            audit.SubTitle = reader.GetString(4);
            audit.Teaser = reader.GetString(5);
            audit.Html = reader.GetString(6);
            audit.PrincipalModified = reader.GetString(7);
            audit.DateModified = reader.GetDateTime(8);
        }

        public List<int> ListLanguages()
        {
            List<int> langList = new List<int>();

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
            "SELECT DISTINCT id FROM [dbo].[language] WHERE id != 1279"))
            {
                while (rdr.Read())
                    langList.Add((int)rdr["id"]);
            }
            return langList;
        }

        #region Dictionary

        public List<BODictionaryEntry> ListDictionaryEntries(int languageId, string searchKeyword)
        {
            var entries = new List<BODictionaryEntry>();

            if (string.IsNullOrEmpty(searchKeyword))
                return entries;

            var paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@languageId", languageId);
            paramsToPass[1] = new SqlParameter("@searchKeyword", "%" + searchKeyword.Trim() + "%");

            var sql = DbHelper.CONTENT_SELECT_PART + "d.content_fk_id, d.keyword ";
            sql += @" FROM [dbo].[dictionary] d 
                     INNER JOIN [dbo].[content] c ON c.id = d.content_fk_id ";
            sql += " INNER JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageId";
            sql += " WHERE d.keyword LIKE @searchKeyword ";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BODictionaryEntry entry = new BODictionaryEntry();
                    entry.ContentId = (int)reader["content_fk_id"];
                    DbHelper.PopulateContent(reader, entry, languageId);
                    entry.KeyWord = reader["keyword"].ToString();
                    entries.Add(entry);
                }
            }

            return entries;
        }

        public PagedList<BODictionaryEntry> ListDictionaryEntries(ListingState state, bool showUntranslated, int languageId, string searchKeywordOrMeaning)
        {
            PagedList<BODictionaryEntry> entries = new PagedList<BODictionaryEntry>();
            if (string.IsNullOrEmpty(state.SortField))
            {
                state.SortField = "keyword";
            }
            SqlParameter[] paramsToPass = new SqlParameter[4];
            paramsToPass[0] = new SqlParameter("@languageId", languageId);
            paramsToPass[1] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            paramsToPass[2] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);
            paramsToPass[3] = new SqlParameter("@searchKeywordOrMeaning", "%" + searchKeywordOrMeaning.Trim() + "%");
            
            string sql =
                @"WITH DictionaryCTE (title, subtitle, teaser, html, principal_created_by, 
                    date_created, principal_modified_by, date_modified, votes, score, 
                    content_fk_id, keyword, RowNumber)
                    AS
                    ( ";
            sql += DbHelper.CONTENT_SELECT_PART +
                   "d.content_fk_id, d.keyword, ";
            sql += "ROW_NUMBER() OVER (  ORDER BY " + 
                state.SortField + " " + (state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            sql += ") AS RowNumber ";

            sql += @"FROM [dbo].[dictionary] d 
                     INNER JOIN [dbo].[content] c ON c.id = d.content_fk_id ";

            sql += showUntranslated ? "LEFT" : "INNER";
            sql += " JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageId";

            if (!string.IsNullOrEmpty(searchKeywordOrMeaning))
            {
                sql += " WHERE cds.title LIKE @searchKeywordOrMeaning ";
                sql += "OR cds.subtitle LIKE @searchKeywordOrMeaning ";
                sql += "OR cds.teaser LIKE @searchKeywordOrMeaning ";
                sql += "OR cds.html LIKE @searchKeywordOrMeaning ";
                sql += "OR d.keyword LIKE @searchKeywordOrMeaning";
            }

            sql += @") SELECT *, (SELECT max(RowNumber) FROM DictionaryCTE) AllRecords 
FROM DictionaryCTE 
WHERE RowNumber BETWEEN @fromRecordIndex AND @toRecordIndex ";

            entries.AllRecords = 0;

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BODictionaryEntry entry = new BODictionaryEntry();
                    entry.ContentId = (int)reader["content_fk_id"];
                    DbHelper.PopulateContent(reader, entry, languageId);
                    entry.KeyWord = reader["keyword"].ToString();
                    entries.Add(entry);

                    if (entries.AllRecords < 1)
                        entries.AllRecords = FormatTool.GetInteger(reader["AllRecords"]);
                }
            }
            return entries;
        }

        public void InsertDictionaryEntry(BODictionaryEntry entry)
        {
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[1] = new SqlParameter("@ContentId", entry.ContentId.Value);
            paramsToPass[2] = new SqlParameter("@KeyWord", entry.KeyWord);
            // we don't really need Id
            paramsToPass[0] = new SqlParameter("@Id", DBNull.Value);
            paramsToPass[0].Direction = ParameterDirection.Output;
            paramsToPass[0].DbType = DbType.Int32;

            string sql = @"INSERT INTO [dbo].[dictionary] 
                        (content_fk_id, keyword)
                        VALUES (@ContentId, @KeyWord); 
                        SET @Id=SCOPE_IDENTITY();";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            // entry.Id = Int32.Parse(paramsToPass[0].Value.ToString());
        }

        public void DeleteDictionaryEntry(string keyWord)
        {
            SqlParameter paramsToPass = new SqlParameter("@keyWord", keyWord);
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                    "DELETE FROM [dbo].[dictionary] WHERE keyword=@keyWord COLLATE SQL_Latin1_General_CP1_CS_AS", 
                    paramsToPass);
        }

        public BODictionaryEntry GetDictionaryEntry(string keyWord)
        {
            BODictionaryEntry entry = null;

            SqlParameter paramsToPass = new SqlParameter("@keyWord", keyWord);

            string sql = @"SELECT id, content_fk_id, keyword 
                           FROM [dbo].[dictionary] 
                           WHERE keyword=@keyWord COLLATE SQL_Latin1_General_CP1_CS_AS";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    entry = new BODictionaryEntry();
                    entry.ContentId = reader.GetInt32(1);
                    entry.KeyWord = reader.GetString(2);
                }
            }

            return entry;
        }

        public BODictionaryEntry GetDictionaryEntry(string keyWord, int languageId, bool showUntranslated = false)
        {
            BODictionaryEntry entry = null;

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@keyWord", keyWord);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = DbHelper.CONTENT_SELECT_PART +
                         @"d.content_fk_id, d.keyword
                            FROM [dbo].[dictionary] d
                            INNER JOIN [dbo].[content] c ON c.id=d.content_fk_id";
            sql += showUntranslated ? " LEFT " : " INNER ";
            sql += @"JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageId
                          WHERE keyword=@keyWord COLLATE SQL_Latin1_General_CP1_CS_AS";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    entry = new BODictionaryEntry();
                    entry.ContentId = (int)reader["content_fk_id"];
                    entry.KeyWord = reader["keyword"].ToString();
                    DbHelper.PopulateContent(reader, entry, languageId);
                }
            }

            return entry;
        }

        public List<List<BODictionaryEntry>> ListAllDictionaryEntries()
        {
            List<List<BODictionaryEntry>> entries = new List<List<BODictionaryEntry>>();

            SqlParameter[] paramsToPass = new SqlParameter[0];

            var sql = DbHelper.CONTENT_SELECT_PART +
                   @"d.content_fk_id, d.keyword, cds.language_fk_id
                     FROM [dbo].[dictionary] d
                     INNER JOIN [dbo].[content] c ON c.id=d.content_fk_id
                     LEFT JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=d.content_fk_id
                     ORDER by keyword, language_fk_id";

            var prevKeyword = "";
            List<BODictionaryEntry> innerEntries = new List<BODictionaryEntry>();

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BODictionaryEntry entry = new BODictionaryEntry();
                    entry.ContentId = (int)reader["content_fk_id"];
                    int languageId = (int)reader["language_fk_id"];
                    DbHelper.PopulateContent(reader, entry, languageId);
                    entry.KeyWord = reader["keyword"].ToString();

                    if (entry.KeyWord != prevKeyword && !string.IsNullOrEmpty(prevKeyword))
                    {
                        entries.Add(innerEntries);
                        innerEntries = new List<BODictionaryEntry>();
                    }

                    prevKeyword = entry.KeyWord;
                    innerEntries.Add(entry);
                }

                entries.Add(innerEntries);
            }

            return entries;
        }

        #endregion Dictionary
    }
}
