using System;
using System.Collections.Generic;
using System.Text;
using MsSqlDBUtility;
using System.Data.SqlClient;
using System.Data;

namespace One.Net.BLL.DAL
{
    public class DbPublisher
    {
        public PagedList<BOPublisherData> ListPublisherData(ListingState listingState, bool published, bool showPending)
        {
            PagedList<BOPublisherData> list = new PagedList<BOPublisherData>();

            SqlParameter[] paramsToPass = new SqlParameter[6];

            paramsToPass[0] = new SqlParameter("@fromRecordIndex", listingState.DbFromRecordIndex);
            paramsToPass[1] = new SqlParameter("@toRecordIndex", listingState.DbToRecordIndex);
            paramsToPass[2] = SqlHelper.GetNullableSortFieldParameter(listingState.SortField);
            paramsToPass[3] = new SqlParameter("@sortOrder", listingState.DbSortDirection);
            paramsToPass[4] = new SqlParameter("@published", published);
            paramsToPass[5] = new SqlParameter("@show_pending", showPending);

            using ( SqlDataReader reader = SqlHelper.ExecuteReader(
                SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[ListPagedPublisherData]", paramsToPass ))
            {
                while ( reader.Read())
                {
                    if (list.AllRecords < 1)
                        list.AllRecords = reader.GetInt32(7);

                    BOPublisherData data = new BOPublisherData();
                    data.Id = reader.GetInt32(0);
                    data.SubSystem = reader.GetString(1);
                    data.FkId = reader.GetInt32(2);
                    data.ScheduledAt = reader.GetDateTime(3);
                    data.PublishedAt = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                    data.Table = reader.GetString(5);
                    list.Add(data);
                }
            }

            return list;
        }

        public void Delete(int itemId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("id", itemId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                                      "DELETE FROM [dbo].[publisher] WHERE id=@id", paramsToPass);
        }

        public void Change(BOPublisherData data)
        {
            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = SqlHelper.GetNullable("id", data.Id); 
            paramsToPass[1] = new SqlParameter("subSystem", data.SubSystem);
            paramsToPass[2] = new SqlParameter("fkId", data.FkId);
            paramsToPass[3] = new SqlParameter("scheduledAt", data.ScheduledAt);
            paramsToPass[4] = SqlHelper.GetNullable("publishedAt", data.PublishedAt); 
            
            switch (data.SubSystem)
            {
                case BOPublisherData.ARTICLE_SUBSYSTEM :
                    paramsToPass[5] = new SqlParameter("table", "[dbo].[article]");
                    break;
                case BOPublisherData.EVENT_SUBSYSTEM:
                    paramsToPass[5] = new SqlParameter("table", "[dbo].[event]");
                    break;
                case BOPublisherData.FAQ_SUBSYSTEM:
                    paramsToPass[5] = new SqlParameter("table", "[dbo].[faq_question]");
                    break;
                case BOPublisherData.COMMENT_SUBSYSTEM:
                    paramsToPass[5] = new SqlParameter("table", "[dbo].[comments]");
                    break;
                case BOPublisherData.PAGE_SUBSYSTEM:
                    paramsToPass[5] = new SqlParameter("table", "[dbo].[pages]");
                    break;
            }


            string sql;

            if (data.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[publisher] 
                        SET subsystem=@subSystem, 
                            fkid=@fkId, 
                            scheduled_at=@scheduledAt, 
                            published_at=@publishedAt,
                            [table]=@table
                        WHERE id=@id";
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].DbType = DbType.Int32;
                sql = @"INSERT INTO [dbo].[publisher]
                        (subsystem, fkid, scheduled_at, [table])
                        VALUES 
                        (@subSystem, @fkId, @scheduledAt, @table);
                        SET @id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                                      sql, paramsToPass);

            if (!data.Id.HasValue)
                data.Id = Int32.Parse(paramsToPass[0].Value.ToString());
        }
    }
}
