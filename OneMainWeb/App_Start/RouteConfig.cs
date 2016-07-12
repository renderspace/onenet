using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using System.ServiceModel.Activation;
using OneMainWeb.Base;
using One.Net.BLL.Service;
using System.Web.Caching;
using Forms.BLL;

namespace OneMainWeb
{
    public static class RouteConfig
    {
        private static readonly ServiceRoute FormService = new ServiceRoute("FormService", new WebServiceHostFactory(), typeof(FormService));
        private static readonly ServiceRoute AdminService = new ServiceRoute("AdminService", new WebServiceHostFactory(), typeof(AdminService));
        private static readonly ServiceRoute ScaffoldService = new ServiceRoute("ScaffoldService", new WebServiceHostFactory(), typeof(ScaffoldService));

        public static void ReloadRoutes(RouteCollection routes)
        {
            var node = SiteMap.Provider.RootNode;
            
            using (routes.GetWriteLock())
            {
                routes.Clear();
                if (!PresentBasePage.ReadPublishFlag())
                {
                    routes.Add(FormService);
                    routes.Add(AdminService);
                    routes.Add(ScaffoldService);
                }
                routes.Add(new Route("sitemap.xml", new HttpHandlerRoute("~/Utils/SiteMapHandler.ashx")));
                routes.Add(new Route("robots.txt", new HttpHandlerRoute("~/Utils/Robots.ashx")));
                routes.Add(new Route("favicon.ico", new HttpHandlerRoute("~/Utils/Favicon.ashx")));

                if (node != null)
                {
                    routes.MapPageRoute("Root", "", "~/site_specific/aspx_templates/" + node["_template"]);
                    var i = 0;

                    if (!string.IsNullOrWhiteSpace(node["_subRouteUrl"]) && node["_subRouteUrl"].StartsWith("{") && node.HasChildNodes && node.ChildNodes.Count == 1)
                    {
                        // route subroute parameter is taken from parent, the rest is taken from subpage where single module should reside.
                        // in this case, subroute rewrite is set on ROOT!
                        var s = node.ChildNodes[0];
                        Route additionalRoute = new Route(node["_subRouteUrl"], new PageRouteHandler("~/site_specific/aspx_templates/" + s["_template"], false));
                        additionalRoute.DataTokens = new RouteValueDictionary { { "_pageID", s["_pageID"] } };
                        routes.Add(additionalRoute);
                    }
                    else 
                    {
                        PopulateRoutes(routes, node, ref i);
                    }                    
                }
            }
        }

        private static void PopulateRoutes(RouteCollection routes, SiteMapNode node, ref int i)
        {
            foreach (SiteMapNode n in node.ChildNodes)
            {
                routes.MapPageRoute("Page" + i++.ToString(), n.Url.TrimStart('/'), "~/site_specific/aspx_templates/" + n["_template"]);

                if (!string.IsNullOrWhiteSpace(n["_subRouteUrl"]) && n["_subRouteUrl"].StartsWith("{") && n.HasChildNodes && n.ChildNodes.Count == 1)
                {
                    // route subroute parameter is taken from parent, the rest is taken from subpage where single module should reside.
                    var s = n.ChildNodes[0];
                    Route additionalRoute = new Route(n.Url.TrimStart('/') + "/" + n["_subRouteUrl"], new PageRouteHandler("~/site_specific/aspx_templates/" + s["_template"], false));
                    additionalRoute.DataTokens = new RouteValueDictionary { { "_pageID", s["_pageID"] } };
                    routes.Add(additionalRoute);
                }
                else
                {
                    PopulateRoutes(routes, n, ref i);
                }
            }
        }
    }
    /*
    public class RouteConfigChangeNotifier
    {
        Action<string> _changeCallback;

        private RouteConfigChangeNotifier(Action<string> changeCallback)
        {
            _changeCallback = changeCallback;
        }

        public static void Listen(Action<string> action)
        {
            var notifier = new RouteConfigChangeNotifier(action);
            notifier.ListenForChanges();
        }

        private void ListenForChanges()
        {
            // Get a CacheDependency from the BuildProvider, 
            // so that we know anytime something changes
            var virtualPathDependencies = new List<string>();
            virtualPathDependencies.Add(virtualPath);
            CacheDependency cacheDependency = _vpp.GetCacheDependency(
              virtualPath, virtualPathDependencies, DateTime.UtcNow);
            HttpRuntime.Cache.Insert(virtualPath ,
              virtualPath,
              cacheDependency,
              Cache.NoAbsoluteExpiration,
              Cache.NoSlidingExpiration,
              CacheItemPriority.NotRemovable,
              new CacheItemRemovedCallback(OnConfigFileChanged));
        }

        private void OnConfigFileChanged(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason != CacheItemRemovedReason.DependencyChanged)
                return;
            _changeCallback(key);
            ListenForChanges();
        }
    }*/
}
