using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;

namespace One.Net.BLL.Paths
{
    public class CustomRouteHandler : IRouteHandler
    {
        private static string templatesPath;

        public CustomRouteHandler(string virtualPath)
        {
            CustomRouteHandler.templatesPath = ConfigurationManager.AppSettings["aspxTemplatesFolder"];
            this.VirtualPath = virtualPath;
        }

        public string VirtualPath { get; private set; }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var vp = "~/" + templatesPath + "/" + VirtualPath;

            var page = BuildManager.CreateInstanceFromVirtualPath (vp, typeof(Page)) as IHttpHandler;
            return page;
        }
    }
}
