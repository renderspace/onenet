using System;
using System.Data;
using System.Data.SqlClient;
using MsSqlDBUtility;
using One.Net.BLL.Model;

namespace One.Net.BLL.DAL
{
    public class DbRssFeed
    {
        public void Change(BORssFeed rssFeed )
        {
            string categories = StringTool.RenderAsString(rssFeed.Categories);

            SqlParameter[] paramsToPass = new SqlParameter[8];
            paramsToPass[0] = new SqlParameter("@title", rssFeed.Title);
            paramsToPass[1] = new SqlParameter("@description", rssFeed.Description);
            paramsToPass[2] = SqlHelper.GetNullable("@categories", categories);
            paramsToPass[3] = new SqlParameter("@languageId", rssFeed.LanguageId);
            paramsToPass[4] = new SqlParameter("@linkToList", rssFeed.LinkToList);
            paramsToPass[5] = new SqlParameter("@linkToSingle", rssFeed.LinkToSingle);
            paramsToPass[6] = new SqlParameter("@type", rssFeed.Type);
            paramsToPass[7] = SqlHelper.GetNullable("@id", rssFeed.Id);
            paramsToPass[7].DbType = DbType.Int32;
            paramsToPass[7].Direction = ParameterDirection.InputOutput;

            string sql;

            if ( rssFeed.Id.HasValue )
            {
                sql =
                    @"UPDATE [dbo].[rss_feeds]
                        SET title=@title,
                            description=@description,
                            categories=@categories,
                            language_fk_id=@languageId,
                            link_to_list=@linkToList,
                            link_to_single=@linkToSingle,
                            type=@type
                        WHERE id=@id";
            }
            else
            {
                sql =
                    @"INSERT INTO [dbo].[rss_feeds] 
                        (title, description, categories, language_fk_id, link_to_list, link_to_single, type)
                      VALUES
                        (@title, @description, @categories, @languageId, @linkToList, @linkToSingle, @type);
                      SET @id=SCOPE_IDENTITY();";                
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            rssFeed.Id = Int32.Parse(paramsToPass[7].Value.ToString());
        }

        public void Delete(int id)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@id", id);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, "DELETE FROM [dbo].[rss_feeds] WHERE id=@id", paramsToPass);
        }

        public PagedList<BORssFeed> List(ListingState state, int languageId)
        {
            PagedList<BORssFeed> rssFeeds = new PagedList<BORssFeed>();

            SqlParameter[] paramsToPass = new SqlParameter[5];
            paramsToPass[0] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            paramsToPass[1] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);
            paramsToPass[2] = SqlHelper.GetNullableSortFieldParameter(state.SortField);
            paramsToPass[3] = new SqlParameter("@sortOrder", state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            paramsToPass[4] = new SqlParameter("@languageId", languageId);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.StoredProcedure, "[dbo].[ListPagedRssFeeds]", paramsToPass))
            {
                while (reader.Read())
                {
                    BORssFeed rssFeed = new BORssFeed();
                    PopulateRssFeed(reader, rssFeed);
                    rssFeeds.Add(rssFeed);
                }
                if (reader.NextResult())
                    if (reader.Read())
                        rssFeeds.AllRecords = reader.GetInt32(0);
            }

            return rssFeeds;
        }

        public BORssFeed Get(int id)
        {
            BORssFeed rssFeed = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@id", id);

            string sql = @" SELECT rf.id, rf.title, rf.description, rf.type, rf.link_to_list, rf.link_to_single, rf.categories, rf.language_fk_id
		                    FROM [dbo].[rss_feeds] rf WHERE rf.id=@id";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                sql, paramsToPass))
            {
                if (reader.Read())
                {
                    rssFeed = new BORssFeed();
                    PopulateRssFeed(reader, rssFeed);
                }
            }

            return rssFeed;
        }

        private static void PopulateRssFeed(IDataRecord reader, BORssFeed rssFeed)
        {
            if (rssFeed != null && reader[0] != DBNull.Value)
            {
                rssFeed.Id = reader.GetInt32(0);
                rssFeed.Title = reader.GetString(1);
                rssFeed.Description = reader.GetString(2);
                rssFeed.Type = reader.GetString(3);
                rssFeed.LinkToList = reader.GetString(4);
                rssFeed.LinkToSingle = reader.GetString(5);
                rssFeed.Categories =
                    StringTool.SplitStringToIntegers(reader.IsDBNull(6) ? "" : reader.GetString(6));
                rssFeed.LanguageId = reader.GetInt32(7);
            }
        }

    }
}
