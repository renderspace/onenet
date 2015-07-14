using System;
using System.Collections.Generic;
using System.Text;
using One.Net.BLL.Model;
using System.Xml;
using System.IO;
using One.Net.BLL.WebConfig;
using One.Net.BLL.DAL;

namespace One.Net.BLL
{
    public class RSSProviderSQL : IRssProvider
    {
        private static readonly DbRSSProviderSql providerSql = new DbRSSProviderSql();

        private RssConfigProvider configProvider;
        public RssConfigProvider ConfigProvider
        {
            get { return configProvider; }
            set { configProvider = value; }
        }

        public List<BORssItem> ListItems(List<int> categories, int languageId)
        {
            string cats = "";
            foreach (int category in categories)
                cats += category + ",";
            cats.TrimEnd(',');

            return providerSql.ListItems(categories, languageId, configProvider.ConnectionString,
                                          configProvider.ListItemsMethod);
        }

        public List<BORssCategory> ListCategories(int languageId)
        {
            return providerSql.ListCategories(languageId, configProvider.ConnectionString,
                                              configProvider.ListCategoriesMethod);
        }
    }
}
