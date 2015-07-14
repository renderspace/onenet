using System;
using System.Configuration;

namespace One.Net.BLL.WebConfig
{
    public class RssConfiguration : ConfigurationSection
    {
        private static readonly RssConfiguration configuration
            = ConfigurationManager.GetSection("rssConfiguration") as RssConfiguration;

        public static RssConfiguration Configuration { get { return configuration; } }

        [ConfigurationProperty("providers")]
        public RssConfigProviderCollection RssProviders { get { return this["providers"] as RssConfigProviderCollection; } } 
    }
}
