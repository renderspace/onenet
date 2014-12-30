using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;
using System.ServiceModel.Activation;
using OneMainWeb.Base;
using One.Net.BLL.Service;
using System.Web.Caching;

namespace OneMainWeb
{
    public static class RouteConfig
    {
        /*
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        } */

//        private static readonly ServiceRoute FormService = new ServiceRoute("FormService", new WebServiceHostFactory(), typeof(FormService));
        private static readonly ServiceRoute AdminService = new ServiceRoute("AdminService", new WebServiceHostFactory(), typeof(AdminService));

        public static void ReloadRoutes(RouteCollection routes)
        {
            var node = SiteMap.Provider.RootNode;
            
            using (routes.GetWriteLock())
            {
                routes.Clear();
                if (!PresentBasePage.ReadPublishFlag())
                {
                    // routes.Add(FormService);
                    routes.Add(AdminService);
                }
                routes.Add(new Route("sitemap.xml", new HttpHandlerRoute("~/Utils/SiteMapHandler.ashx")));
                routes.Add(new Route("robots.txt", new HttpHandlerRoute("~/Utils/Robots.ashx")));
                routes.Add(new Route("favicon.ico", new HttpHandlerRoute("~/Utils/Favicon.ashx")));

                if (node != null)
                {
                    routes.MapPageRoute("Root", "", "~/site_specific/aspx_templates/" + node["_template"]);
                    var i = 0;
                    foreach (SiteMapNode n in node.GetAllNodes())
                    {
                        routes.MapPageRoute("Page" + i++.ToString(), n.Url.TrimStart('/'), "~/site_specific/aspx_templates/" + n["_template"]);
                        /*if (n.Url.Contains("test"))
                        {
                            routes.MapPageRoute("Page" + i++.ToString(), n.Url.TrimStart('/') + "/{year}", "~/site_specific/aspx_templates/" + n["_template"]);
                        }*/
                    }
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
