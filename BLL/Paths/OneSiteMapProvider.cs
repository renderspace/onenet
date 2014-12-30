using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Routing;
using NLog;
using One.Net.BLL.DAL;


// http://codeproject.com/aspnet/aspnet2security.asp?df=100&forumid=123760&exp=0&select=1198046
// http://msdn.microsoft.com/msdnmag/issues/06/02/WickedCode/
// http://weblogs.asp.net/scottgu/archive/2005/12/04/432318.aspx

namespace One.Net.BLL
{
    public class OneSiteMapProvider : StaticSiteMapProvider
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private static DbWebsite webSiteDb;
        private readonly object _lock = new object();
        private const string _cacheDependencyName = "__OneSiteMapCacheDependency";
        

        private SiteMapNode _root;
        private Dictionary<int, SiteMapNode> _nodes = new Dictionary<int, SiteMapNode>(64);

        private static int webSiteID;

        protected bool publishFlag = false;

        public OneSiteMapProvider() 
        {
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);        
        }


        public override void Initialize(string name, NameValueCollection config)
        {
            webSiteID = int.Parse(ConfigurationManager.AppSettings["WebSiteId"]);
            // Verify parameters
            if (config == null) throw new ArgumentNullException("config");
            if (String.IsNullOrEmpty(name)) name = "OneSiteMapProvider";

            base.Initialize(name, config);

            webSiteDb = new DbWebsite();
        }

        public override SiteMapNode BuildSiteMap()
        {
            
            lock (_lock)
            {
                
                if (_root != null)
                {
                    return _root;
                }
                log.Debug("-- cache miss");

                List<BOPage> pages = webSiteDb.GetSiteStructure(webSiteID, Thread.CurrentThread.CurrentCulture.LCID, publishFlag);

                foreach (BOPage page in pages)
                {
                    SiteMapNode addedNode = null;
                    if (_root == null)
                    {
                        _root = AddPage(page);
                        AddNode(_root, null);
                        addedNode = _root;
                    }
                    else
                    {
                        if (_nodes.ContainsKey(page.Id))
                        {
                            throw new ProviderException("Duplicate node ID");
                        }

                        SiteMapNode node = this.AddPage(page);
                        addedNode = node;
                        if (!_nodes.ContainsKey(page.ParentId.Value))
                        {
                            throw new ProviderException("Invalid parent ID");
                        }

                        try
                        {
                            base.AddNode(node, _nodes[page.ParentId.Value]);
                        }
                        catch (InvalidOperationException iex)
                        {
                            log.Fatal("", iex);
                            _root = null;
                            return null;
                        }
                    }
                }

                OCache.Add(_cacheDependencyName, new object(), null, Cache.NoAbsoluteExpiration, 
                    new TimeSpan(1,0,0), CacheItemPriority.AboveNormal, 
                    OnSiteMapChanged);
                return _root;
            }
        }

        public static void ReloadSiteMap()
        {
            OCache.Remove(_cacheDependencyName);
        }

        protected SiteMapNode AddPage(BOPage page)
        {
            string[] rolelist = null;
            string relLink;
            int depth;

            if (page.IsRoot)
            {
                relLink = "/";
                depth = 0;
            }
            else
            {
                //string parentRelLink = _nodes[page.ParentId.Value].Url;
                //relLink = parentRelLink + "/" + page.ParLink;
                
                string parentRelLink = _nodes[page.ParentId.Value].Url;
                if (parentRelLink.EndsWith("/"))
                {
                    relLink = parentRelLink + page.ParLink;
                }
                else
                {
                    relLink = parentRelLink + "/" + page.ParLink;
                }
                
                depth = int.Parse(_nodes[page.ParentId.Value]["_absDepth"]) + 1;
            }
            SiteMapNode node = new SiteMapNode(this, page.Id.ToString(), relLink, page.Title, page.Teaser, rolelist, null, null, null);
            node["_languageId"] = page.LanguageId.ToString();
            node["_template"] = page.Template.Name + ".aspx";
            node["_menuGroup"] = page.MenuGroup.ToString();
            node["_pageID"] = page.Id.ToString();
            node["_pageTitle"] = page.Title ?? "";
            node["_absDepth"] = depth.ToString();
            node["_IsRedirected"] = page.IsRedirected.ToString();
            node["_redirectToUrl"] = page.RedirectToUrl;
            node["_subRouteUrl"] = page.SubRouteUrl;

            _nodes.Add(page.Id, node);
            return node;
        }

        protected override SiteMapNode GetRootNodeCore()
        {
            return BuildSiteMap();
        }

        void OnSiteMapChanged(string key, object item, CacheItemRemovedReason reason)
        {
            lock (_lock)
            {
                log.Info("SiteMapChanged");
                if (key == _cacheDependencyName)
                {
                    base.Clear();
                    _nodes.Clear();
                    _root = null;
                }
            }
        }
    }
}
