using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for SiteMapHandler
    /// </summary>
    public class SiteMapHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            var websiteB = new BWebsite();
            var selectedWebSiteId = PresentBasePage.ReadWebSiteId();
            context.Response.Write(websiteB.GetGoogleSiteMap(selectedWebSiteId));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}