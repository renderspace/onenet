using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using MsSqlDBUtility;


namespace One.Net.BLL.DAL
{
    public class DbNewsLtr
    {
        public int Change(BONewsLtrSub subscription)
        {
            int rowsAffected = 0;

            if (subscription.SubscriptionId == -1)
            {
                SqlParameter[] paramsToPass = new SqlParameter[2];
                paramsToPass[0] = new SqlParameter("@email", subscription.Email);
                paramsToPass[1] = new SqlParameter("@emailID", -1);
                paramsToPass[1].Direction = ParameterDirection.Output;
                paramsToPass[1].DbType = DbType.Int32;
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, 
                    "[dbo].[email_modify]", paramsToPass);
                int emailID = (int)paramsToPass[1].Value;

                paramsToPass = new SqlParameter[4];
                paramsToPass[0] = new SqlParameter("@newsletterID", subscription.NewsLetterId);
                paramsToPass[1] = new SqlParameter("@emailID", emailID);
                paramsToPass[2] = new SqlParameter("@hash", subscription.Hash);
                paramsToPass[3] = new SqlParameter("@subscriptionID", -1);
                paramsToPass[3].Direction = ParameterDirection.Output;
                paramsToPass[3].DbType = DbType.Int32;

                rowsAffected = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                    @"INSERT INTO newsletter_subscription ( newsletter_fk_id, email_fk_id, date_subscribed, hash) 
	                    values ( @newsletterID, @emailID, GetDate(), @hash);
	                    SET @subscriptionID = @@identity", paramsToPass);
                subscription.SubscriptionId = (int)paramsToPass[3].Value;
            }
            else
            {
                SqlParameter[] paramsToPass = new SqlParameter[6];
                paramsToPass[0] = new SqlParameter("@subscriptionID", subscription.SubscriptionId);
                paramsToPass[1] = new SqlParameter("@hash", subscription.Hash);
                paramsToPass[2] = SqlHelper.GetNullable("dateConfirmed", subscription.DateConfirmed);
                paramsToPass[3] = (subscription.DateSubscribed == DateTime.MinValue ? new SqlParameter("@dateSubscribed", DBNull.Value) : new SqlParameter("@dateSubscribed", subscription.DateSubscribed));
                paramsToPass[4] = SqlHelper.GetNullable("dateUnsubscribed", subscription.DateUnsubscribed);
                paramsToPass[5] = SqlHelper.GetNullable("ipConfirmed", subscription.IpConfirmed); 
                rowsAffected = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, 
                    @"UPDATE newsletter_subscription 
                        SET date_unsubscribed=@dateUnsubscribed, date_confirmed=@dateConfirmed, 
                            date_subscribed=@dateSubscribed, ip_confirmed=@ipConfirmed 
                        WHERE  id = @subscriptionID and hash = @hash", paramsToPass);
            }

            return rowsAffected;
        }

        public int DeleteSubscription(int subscriptionID)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@subscriptionId", subscriptionID);

            return SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[DeleteNewsletterSubscription]", paramsToPass);
        }

        public PagedList<BONewsLtrSub> ListSubscriptions(int newsletterID, ListingState state, int subscriptionType)
        {
            PagedList<BONewsLtrSub> list = new PagedList<BONewsLtrSub>();

            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = new SqlParameter("@newsletterID", newsletterID);
            paramsToPass[1] = new SqlParameter("@fromRecordIndex", state.DbFromRecordIndex);
            paramsToPass[2] = new SqlParameter("@toRecordIndex", state.DbToRecordIndex);
            paramsToPass[3] = state.SortField == null || state.SortField.Length < 2 ? new SqlParameter("@sortBy", DBNull.Value) : new SqlParameter("@sortBy", state.SortField);
            paramsToPass[4] = new SqlParameter("@sortOrder", state.DbSortDirection);
            paramsToPass[5] = new SqlParameter("@subscriptionType", subscriptionType);
            
            CommandType commandType = CommandType.StoredProcedure;
            string sql = "";

            commandType = CommandType.Text;
            sql = @"SELECT ns.id, newsletter_fk_id, email_fk_id, date_subscribed, date_unsubscribed, hash, date_confirmed, ip_confirmed, e.email 
                    FROM newsletter_subscription ns 
                    INNER JOIN email e on e.id=ns.email_fk_id 
                    WHERE ns.newsletter_fk_id=@newsletterID";

            if (subscriptionType == BONewsLtrSub.STARTED_SUBSCRIPTION)
                sql += " AND date_subscribed IS NOT null AND date_confirmed IS null AND date_unsubscribed IS null";
            else if (subscriptionType == BONewsLtrSub.CONFIRMED)
                sql += " AND date_subscribed IS NOT null AND date_confirmed IS NOT null AND date_unsubscribed IS null";
            else if (subscriptionType == BONewsLtrSub.UNSUBSCRIBED)
                sql += " AND date_subscribed IS NOT null AND date_confirmed IS NOT null AND date_unsubscribed IS NOT null";

            sql += " ORDER BY email ASC";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, commandType, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BONewsLtrSub sub = new BONewsLtrSub();
                    sub.SubscriptionId = (reader.GetValue(0) == DBNull.Value ? -1 : (int)reader.GetValue(0));
                    sub.NewsLetterId = newsletterID;
                    sub.EmailId = (int)reader.GetValue(2);
                    sub.DateSubscribed = (DateTime)reader.GetValue(3);
                    sub.DateUnsubscribed = reader.GetValue(4) as DateTime?;
                    sub.Hash = (reader.GetValue(5) == DBNull.Value ? "" : (string)reader.GetValue(5));
                    sub.DateConfirmed = reader.GetValue(6) as DateTime?;
                    sub.IpConfirmed = (reader.GetValue(7) == DBNull.Value ? new IPAddress(0) : IPAddress.Parse((string)reader.GetValue(7)));
                    sub.Email = (reader.GetValue(8) == DBNull.Value ? "" : (string)reader.GetValue(8));

                    list.Add(sub);
                }

                if (reader.NextResult() && reader.Read())
                    list.AllRecords = (int)reader.GetValue(0);
                else
                    list.AllRecords = 0;
            }

            return list;
        }

        public List<BONewsLtrSub> ListSubscriptions(string email)
        {
            List<BONewsLtrSub> list = new List<BONewsLtrSub>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@email", email);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, "select ns.id, newsletter_fk_id, email_fk_id, date_subscribed, date_unsubscribed, hash, date_confirmed, ip_confirmed, e.email from newsletter_subscription ns inner join email e on e.id=ns.email_fk_id where e.email=@email and date_confirmed is not null and date_unsubscribed is null", paramsToPass))
            {
                while (reader.Read())
                {
                    BONewsLtrSub sub = new BONewsLtrSub();
                    sub.SubscriptionId = (reader.GetValue(0) == DBNull.Value ? -1 : (int)reader.GetValue(0));
                    sub.NewsLetterId = (int)reader.GetValue(1);
                    sub.EmailId = (int)reader.GetValue(2);
                    sub.DateSubscribed = (DateTime)reader.GetValue(3);
                    sub.DateUnsubscribed = reader.GetValue(4) as DateTime?;
                    sub.Hash = (reader.GetValue(5) == DBNull.Value ? "" : (string)reader.GetValue(5));
                    sub.DateConfirmed = reader.GetValue(6) as DateTime?;
                    sub.IpConfirmed = (reader.GetValue(7) == DBNull.Value ? new IPAddress(0) : IPAddress.Parse((string)reader.GetValue(7)));
                    sub.Email = (reader.GetValue(8) == DBNull.Value ? "" : (string)reader.GetValue(8));

                    list.Add(sub);
                }
            }

            return list;
        }

        public BONewsLtrSub GetSubscription(int subscriptionID, int newsletterID, string email)
        {
            BONewsLtrSub sub = new BONewsLtrSub();

            sub.NewsLetterId = newsletterID;
            sub.Email = email;

            SqlParameter[] paramsToPass = new SqlParameter[3];

            paramsToPass[0] = (subscriptionID <= 0 ? new SqlParameter("@subscriptionID", DBNull.Value) : new SqlParameter("@subscriptionID", subscriptionID));
            paramsToPass[1] = SqlHelper.GetNullable("@email", email);
            paramsToPass[2] = (newsletterID <= 0 ? new SqlParameter("@newsletterID", DBNull.Value) : new SqlParameter("@newsletterID", newsletterID));

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "newsletter_subscription_get", paramsToPass))
            {
                while (reader.Read())
                {
                    sub.SubscriptionId = (reader.GetValue(0) == DBNull.Value ? -1 : (int)reader.GetValue(0));
                    sub.EmailId = (int)reader.GetValue(2);
                    sub.DateSubscribed = (DateTime)reader.GetValue(3);
                    sub.DateUnsubscribed = reader.GetValue(4) as DateTime?;
                    sub.Hash = (reader.GetValue(5) == DBNull.Value ? "" : (string)reader.GetValue(5));
                    sub.DateConfirmed = reader.GetValue(6) as DateTime?;
                    sub.IpConfirmed = (reader.GetValue(7) == DBNull.Value ? new IPAddress(0) : IPAddress.Parse((string)reader.GetValue(7)));
                    sub.Email = (reader.GetValue(8) == DBNull.Value ? "" : (string)reader.GetValue(8));
                    sub.NewsLetterId = (reader.GetValue(1) == DBNull.Value ? -1 : (int)reader.GetValue(1));
                }
            }

            return sub;
        }

        public BONewsLtr GetNewsletter(int newsletterID)
        {
            BONewsLtr newsletter = null;
            SqlParameter paramsToPass = new SqlParameter("@newsletterID", newsletterID);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, 
                @"SELECT n.id, n.name newsletter_name
                    FROM [dbo].[newsletter] n 
                        WHERE n.id = @newsletterID", paramsToPass))
            {
                if (reader.Read())
                {
                    newsletter = new BONewsLtr();
                    newsletter.Id = (reader.GetValue(0) == DBNull.Value ? -1 : (int)reader.GetValue(0));
                    newsletter.Name = (reader.GetValue(1) == DBNull.Value ? "" : (string)reader.GetValue(1));
                }
            }

            return newsletter;
        }

        public List<BONewsLtr> ListNewsletters(List<int> newsletterIds)
        {
            List<BONewsLtr> list = new List<BONewsLtr>();

            List<SqlParameter> paramsToPass = new List<SqlParameter>();
            
            string sql =
                @"SELECT n.id, n.template_fk_id, n.name newsletter_name
                    FROM newsletter n";

            if (newsletterIds != null && newsletterIds.Count > 0)
                sql += " WHERE n.id IN (" + string.Join(",", newsletterIds) + ")";

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, 
                sql, paramsToPass.ToArray()))
            {
                while (reader.Read())
                {
                    BONewsLtr newsletter = new BONewsLtr();
                    newsletter.Id = (reader.GetValue(0) == DBNull.Value ? -1 : (int)reader.GetValue(0));
                    newsletter.Name = (reader.GetValue(2) == DBNull.Value ? "" : (string)reader.GetValue(2));
                    list.Add(newsletter);
                }
            }

            return list;
        }
    }
}
