using System;
using System.Data;
using System.Configuration;

namespace One.Net.BLL.WebConfig
{
    public class RssConfigProvider : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name { get { return this["name"] as string; } }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type { get { return this["type"] as string; } }

        [ConfigurationProperty("listItemsMethod", IsRequired = true)]
        public string ListItemsMethod { get { return this["listItemsMethod"] as string; } }

        [ConfigurationProperty("listCategoriesMethod", IsRequired = true)]
        public string ListCategoriesMethod { get { return this["listCategoriesMethod"] as string; } }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString { get { return this["connectionString"] as string; } }
    }
}
