using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for Robots
    /// </summary>
    public class Robots : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var publishFlag = PresentBasePage.ReadPublishFlag();

            context.Response.ContentType = "text/plain";

            if (publishFlag)
            {
                context.Response.Write(@"User-agent: *
Sitemap: /sitemap.xml
Disallow: /adm/
Disallow: /Account/
Allow: /");
            }
            else
            {
                context.Response.Write(@"User-agent: *
Disallow: /");
            }
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