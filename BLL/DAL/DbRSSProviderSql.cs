using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using MsSqlDBUtility;
using One.Net.BLL.Model;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;

namespace One.Net.BLL.DAL
{
    public class DbRSSProviderSql
    {
        public List<BORssItem> ListItems(List<int> categories, int languageId, string connectionString, string listItemsSP)
        {
            List<BORssItem> items = new List<BORssItem>();

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@categoryIds", string.Join(",", categories));
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            connectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;

            using (var reader = SqlHelper.ExecuteReader(connectionString,
                CommandType.StoredProcedure, listItemsSP, paramsToPass))
            {
                while (reader.Read())
                {
                    BORssItem item = new BORssItem();
                    item.Id = reader.GetInt32(0);
                    item.Title = reader.GetString(1);
                    item.SubTitle = reader.GetString(2);
                    item.Teaser = reader.GetString(3);
                    item.Html = reader.GetString(4);
                    item.PubDate = reader.GetDateTime(5);

                    if (ColumnExists(reader, "image_url"))
                        item.ImageUrl = reader["image_url"] != DBNull.Value ? (string)reader["image_url"] : "";
                    else
                        item.ImageUrl = "";

                    items.Add(item);
                }
            }

            return items;
        }

        private bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        public List<BORssCategory> ListCategories(int languageId, string connectionString, string listItemsSP)
        {
            List<BORssCategory> items = new List<BORssCategory>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@languageId", languageId);

            connectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;

            if (!string.IsNullOrEmpty(listItemsSP))
            {
                using (var reader = SqlHelper.ExecuteReader(connectionString,
                    CommandType.StoredProcedure, listItemsSP, paramsToPass))
                {
                    while (reader.Read())
                    {
                        BORssCategory cat = new BORssCategory();
                        cat.Id = reader.GetInt32(0);
                        cat.Title = reader.GetString(1);
                        items.Add(cat);
                    }
                }
            }

            return items;
        }
    }
}
