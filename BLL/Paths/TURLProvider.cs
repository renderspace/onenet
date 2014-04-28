using System;
using System.Configuration;
using System.Web;
using log4net;
using UrlRewritingNet.Configuration.Provider;
using UrlRewritingNet.Web;

namespace One.Net.BLL
{
    public class TURLProvider : UrlRewritingProvider
    {
        public override RewriteRule CreateRewriteRule()
        {
            return new TRewriteRule();
        }
    }

    public class TRewriteRule : RewriteRule
    {
        private static string templatesPath;
        private readonly ILog log = LogManager.GetLogger("TRewriteRule");

        public TRewriteRule()
        {
            log.Debug("~TRewriteRule");
            templatesPath = ConfigurationManager.AppSettings["aspxTemplatesFolder"];
            if (String.IsNullOrEmpty(templatesPath))
            {
                throw new ConfigurationErrorsException("Missing aspxTemplatesFolder web.config setting.");
            }
        }

        private SiteMapNode AnalyseUrl(string reqUrl)
        {
            log.Debug("VVVV AnalyseUrl");
            string[] split = reqUrl.Split(new[] { '?' }, 2);
            string requestUrl = split[0];
            /*
             string fileName = FormatTool.GetFileName(requestUrl);
            string extension = FormatTool.GetFileExtension(requestUrl);
             * 
            if (fileName == "index.aspx" || fileName == "default.aspx" ) //extension.Length > 0
            {
                actualVirtualPath = requestUrl.Replace(fileName, "").TrimEnd(new char[] {'/'});
            }
            else
            {*/
            string actualVirtualPath = requestUrl;
            //}
            SiteMapNode smn = SiteMap.Provider.FindSiteMapNode(actualVirtualPath);
            log.Debug("^^^^ AnalyseUrl");
            return smn;
        }

        public override bool IsRewrite(string requestUrl)
        {
            
            if (requestUrl.Contains("WebResource.axd"))
            {
                log.Debug("-IsRewrite: No (WebResource.axd)");
                return false;
            }
            SiteMapNode node = AnalyseUrl(requestUrl);
            if (node != null)
            {
                log.Debug("-IsRewrite: Yes (" + requestUrl + ")");
                return true;
            }
            else
            {
                log.Debug("-IsRewrite: No (" + requestUrl + ")");
                return false;
            }
        }

        public override string RewriteUrl(string url)
        {
            SiteMapNode smn = this.AnalyseUrl(url);
            if (smn["_template"] != null)
            {
                //This runs in separate thread so setting culture here is ineffective.
                //Thread.CurrentThread.CurrentCulture = new CultureInfo(int.Parse(smn["_languageId"]));
                return "~/" + templatesPath + "/" + smn["_template"];
            }
            return url;
        }
    }
}
