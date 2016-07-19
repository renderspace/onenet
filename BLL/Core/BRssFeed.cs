using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Web.UI;

using One.Net.BLL.Model;
using One.Net.BLL.DAL;
using One.Net.BLL.WebConfig;

namespace One.Net.BLL
{
    public class BRssFeed : BusinessBaseClass
    {
        private static readonly DbRssFeed rssFeedDb = new DbRssFeed();

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingRssChannel = new Object();

        public void Change(BORssFeed rssFeed)
        {
            rssFeedDb.Change(rssFeed);
        }

        public void Delete(int id)
        {
            rssFeedDb.Delete(id);
        }

        public PagedList<BORssFeed> List(ListingState state)
        {
            return rssFeedDb.List(state, LanguageId);
        }

        public BORssFeed Get(int id)
        {
            string cacheKey = "RssFeed-" + id;
            BORssFeed cRss = cache.Get<BORssFeed>(cacheKey);
            if (cRss == null)
            {
                cRss = GetUnCached(id);
                if (cRss != null)
                {
                    lock (rssFeedDb)
                    {
                        BORssFeed tempRss = cache.Get<BORssFeed>(cacheKey);
                        if (null == tempRss)
                            cache.Put(cacheKey, cRss);
                    }
                }
            }
            return cRss;
        }

        public BORssFeed GetUnCached(int id)
        {
            return rssFeedDb.Get(id);
        }

        public XmlDocument GetRssChannel(int channelId)
        {
            string cacheKey = "RssChannel-" + channelId + "_" + LanguageId;
            XmlDocument cDoc = cache.Get<XmlDocument>(cacheKey);

            if (cDoc == null)
            {
                cDoc = GetUnCachedRssChannel(channelId);
                if (cDoc != null)
                {
                    lock (cacheLockingRssChannel)
                    {
                        XmlDocument tempRssChannel = cache.Get<XmlDocument>(cacheKey);
                        if (null == tempRssChannel)
                            cache.Put(cacheKey, cDoc);
                    }
                }
            }
            return cDoc;
        }

        /// <summary>
        /// Method takes id of an RSS feed, looks up the details in the DB, including information 
        /// about which provider to use. Then it uses reflection to load provider, passing 
        /// along required categories. It then combines both responses into RSS 2.0 compliant 
        /// XML. Method returns null if there is no feed by that ID; if provider doesn't exist, 
        /// it throws exception.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        protected XmlDocument GetUnCachedRssChannel(int channelId)
        {
            BORssFeed feed = Get(channelId);
            if (feed == null)
                return CreateErrorDocument("Rss feed with id=" + channelId + " does not exist.");
            else if (RssConfiguration.Configuration == null)
                return CreateErrorDocument("Rss Configuration element missing from configuration");
            else if (RssConfiguration.Configuration.RssProviders == null)
                return CreateErrorDocument("Could not load Rss configuration providers.");
            
            RssConfigProviderCollection configProviders = RssConfiguration.Configuration.RssProviders;

            if (configProviders.Count == 0)
                return CreateErrorDocument("No Rss configuration providers found.");

            try
            {
                foreach (RssConfigProvider configProvider in configProviders)
                {
                    if (configProvider.Name == feed.Type)
                    {
                        XmlDocument doc = new XmlDocument();

                        XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
                        doc.AppendChild(dec);

                        XmlElement rssRoot = doc.CreateElement("rss");
                        rssRoot.SetAttribute("version", "2.0");
                        doc.AppendChild(rssRoot);

                        XmlElement channel = doc.CreateElement("channel");
                        rssRoot.AppendChild(channel);

                        XmlElement title = doc.CreateElement("title");
                        title.InnerText = feed.Title;
                        channel.AppendChild(title);

                        // atom:link is required to make feed valid
                        if (HttpContext.Current.Request != null)
                        {
                            // Use page instance.
                            XmlElement atomLink = doc.CreateElement("link", "http://www.w3.org/2005/Atom");
                            atomLink.SetAttribute("href", HttpContext.Current.Request.Url.AbsoluteUri);
                            atomLink.SetAttribute("rel", "self");
                            atomLink.SetAttribute("type", "application/rss+xml");
                            channel.AppendChild(atomLink);

                            // Use page instance.
                            XmlElement link = doc.CreateElement("link");
                            link.InnerText = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host;

                            channel.AppendChild(link);
                        }

                        XmlElement language = doc.CreateElement("language");
                        language.InnerText = Thread.CurrentThread.CurrentCulture.Name;
                        channel.AppendChild(language);

                        XmlElement description = doc.CreateElement("description");
                        description.InnerText = feed.Description;
                        channel.AppendChild(description);

                        // found provider, proceed loading provider dynamically
                        Type feedType = Type.GetType(configProvider.Type);
                        if (feedType != null)
                        {
                            ConstructorInfo constructor = feedType.GetConstructor(new Type[0]);

                            if (constructor != null)
                            {
                                IRssProvider rssProvider = constructor.Invoke(null) as IRssProvider;

                                if (rssProvider != null)
                                {
                                    rssProvider.ConfigProvider = configProvider;
                                    List<BORssItem> items = rssProvider.ListItems(feed.Categories, LanguageId);

                                    foreach (BORssItem item in items)
                                    {
                                        XmlElement xmlItem = doc.CreateElement("item");
                                        channel.AppendChild(xmlItem);

                                        XmlElement itemTitle = doc.CreateElement("title");
                                        itemTitle.InnerText = item.Title;
                                        xmlItem.AppendChild(itemTitle);

                                        XmlElement itemGuid = doc.CreateElement("guid");
                                        itemGuid.InnerText = feed.LinkToSingle + item.Id;
                                        xmlItem.AppendChild(itemGuid);

                                        XmlElement itemLink = doc.CreateElement("link");
                                        itemLink.InnerText = feed.LinkToSingle + item.Id;
                                        xmlItem.AppendChild(itemLink);

                                        XmlElement itemPubDate = doc.CreateElement("pubDate");
                                        // date formatting (RFC 822)
                                        itemPubDate.InnerText = item.PubDate.ToUniversalTime().ToString("R");
                                        xmlItem.AppendChild(itemPubDate);

                                        XmlElement itemDescription = doc.CreateElement("description");
                                        itemDescription.InnerText = item.Teaser;
                                        xmlItem.AppendChild(itemDescription);

                                        if (!string.IsNullOrEmpty(item.ImageUrl))
                                        {
                                            XmlElement itemEnclosure = doc.CreateElement("enclosure");

                                            itemEnclosure.SetAttribute("url", item.ImageUrl);
                                            itemEnclosure.SetAttribute("type", GetMimeType(item.ImageUrl));
                                            // When an enclosure's size cannot be determined, a publisher SHOULD use a length of 0.
                                            itemEnclosure.SetAttribute("length", "0");

                                            xmlItem.AppendChild(itemEnclosure);
                                        }
                                    }
                                }
                                else
                                {
                                    return CreateErrorDocument("Could not invoke RssProvider " + configProvider.Type);
                                }
                            }
                            else
                            {
                                return CreateErrorDocument("Could not create RssProvider constructor " + configProvider.Type);
                            }
                        }
                        else
                        {
                            return CreateErrorDocument("Could not create RssProvider type " + configProvider.Type);
                        }

                        return doc;
                    }
                }
                return CreateErrorDocument("Could not find Rss configuration provider '" + feed.Type + "'");
            }
            catch (Exception ex)
            {
                log.Error(ex, "loading RSS channel");
                return CreateErrorDocument("Exception loading RSS channel:'" + ex.Message + "'");
            }
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string extension = System.IO.Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".gif": mimeType = "image/gif"; break;
                case ".jpeg": mimeType = "image/jpeg"; break;
                case ".jpg": mimeType = "image/jpeg"; break;
                case ".png": mimeType = "image/png"; break;
                default: break;
            }

            return mimeType;
        }

        public XmlDocument CreateErrorDocument(string error)
        {
            log.Error(error);

            XmlDocument doc = new XmlDocument();

            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);

            XmlElement rssRoot = doc.CreateElement("rss");
            rssRoot.SetAttribute("version", "2.0");
            doc.AppendChild(rssRoot);

            XmlElement channel = doc.CreateElement("channel");
            rssRoot.AppendChild(channel);

            XmlElement title = doc.CreateElement("title");
            title.InnerText = "Error";
            channel.AppendChild(title);

            XmlElement description = doc.CreateElement("description");
            description.InnerText = error;
            channel.AppendChild(description);

            return doc;
        }

        public List<BORssCategory> ListCategories(string providerName)
        {
            List<BORssCategory> cCategories = cache.Get<List<BORssCategory>>("RssLC_" + LanguageId);

            if (cCategories == null)
            {
                cCategories = ListUnCachedCategories(providerName);
                if (cCategories != null)
                    cache.Put("RssLC_" + LanguageId, cCategories);
            }

            return cCategories;
        }

        public RssConfigProviderCollection RetreiveProviderCollection()
        {
            RssConfigProviderCollection configProviders = RssConfiguration.Configuration.RssProviders;
            if (configProviders == null)
                throw new Exception("Missing rssConfiguration from web.config");
            else if (configProviders.Count == 0)
                throw new Exception("Missing rssConfiguration providers from web.config");
            return configProviders;
        }

        public List<BORssCategory> ListUnCachedCategories(string providerName)
        {
            RssConfigProviderCollection configProviders = RssConfiguration.Configuration.RssProviders;

            foreach (RssConfigProvider configProvider in configProviders)
            {
                if (configProvider.Name == providerName)
                {
                    Type feedType = Type.GetType(configProvider.Type);
                    if (feedType != null)
                    {
                        ConstructorInfo constructor = feedType.GetConstructor(new Type[0]);
                        if (constructor != null)
                        {
                            IRssProvider rssProvider = constructor.Invoke(null) as IRssProvider;
                            if (rssProvider != null)
                            {
                                rssProvider.ConfigProvider = configProvider;
                                return rssProvider.ListCategories(LanguageId);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
