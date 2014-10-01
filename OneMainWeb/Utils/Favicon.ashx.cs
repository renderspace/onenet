using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for Favicon
    /// </summary>
    public class Favicon : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var publishFlag = PresentBasePage.ReadPublishFlag();
            var selectedWebSiteId = PresentBasePage.ReadWebSiteId();
            var id = 0;
            if (!publishFlag && context.Request["id"] != null && int.TryParse(context.Request["id"].ToString(), out id))
            {
                selectedWebSiteId = id;
            }
            context.Response.ContentType = "image/x-icon";
            var websiteB = new BWebsite();
            var website = websiteB.Get(selectedWebSiteId);
            if (website != null)
            {
                var content = website.GetSettingValue("Favicon");
                if (string.IsNullOrWhiteSpace(content))
                {
                    content = @"AAABAAEAEBACAAAAAACwAAAAFgAAACgAAAAQAAAAIAAAAAEAAQAAAAAAQAAAAAAAAAAAAAAAAgAA
AAAAAAD///8AAAD/AP//AACzawAArqsAAK6rAACuqwAAs2cAAP//AAD//wAA//8AAP//AACZAwAA
kTMAAIEzAACJMwAAmQMAAP//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
                }
                var result = Convert.FromBase64String(content);
                context.Response.BinaryWrite(result);
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