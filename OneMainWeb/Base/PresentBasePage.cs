using System;
using System.Configuration;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using One.Net.BLL;
using One.Net.BLL.Model.Web;
using One.Net.BLL.Web;
using One.Net.BLL.Model;
using One.Net.BLL.WebConfig;
using OneMainWeb.Base;


namespace OneMainWeb
{
    public class PresentBasePage : Page
    {
        protected BWebsite websiteB;
        private readonly string customModulesFolder;
        

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("PresentBasePage");

        private readonly List<MModule> activeModules = new List<MModule>();
        private readonly List<BOIntContImage> imagesOnThisPage = new List<BOIntContImage>();

        public int PageId
        {
            get
            {
                return (SiteMap.CurrentNode != null) ? FormatTool.GetInteger(SiteMap.CurrentNode["_pageID"]) : -1;
            }
        }

        public bool PublishFlag { get; set; }
        public BOPage CurrentPage { get; set; }
        public BOWebSite CurrentWebsite { get; set; }

        protected internal string CustomModulesFolder
        {
            get { return customModulesFolder; }
        }

        public static bool ReadPublishFlag()
        { 
            var publishFlag = false;
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);
            return publishFlag;
        }

        public static int ReadWebSiteId()
        {
            var websiteId = 0;
            int.TryParse(ConfigurationManager.AppSettings["WebSiteId"], out websiteId);
            return websiteId;
        }

        public PresentBasePage()
        {
            var websiteB = new BWebsite();
            CurrentPage = websiteB.GetPage(PageId);
            CurrentWebsite = websiteB.Get(CurrentPage.WebSiteId);
            customModulesFolder = "site_specific/custom_modules";
            // main language setup.
            if (SiteMap.CurrentNode != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(int.Parse(SiteMap.CurrentNode["_languageId"]));
            }
            PublishFlag = ReadPublishFlag();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            

            var customBodyCode = "";
            var customHeadCode = "";

            if (CurrentWebsite.HasGoogleAnalytics)
            {
                string code = CurrentWebsite.Settings["GoogleAnalyticsWebPropertyID"].Value;
                var enableCookieConsent = false;
                if (CurrentWebsite.Settings.ContainsKey("GoogleAnalyticsConsent"))
                {
                    enableCookieConsent = FormatTool.GetBoolean(CurrentWebsite.Settings["GoogleAnalyticsConsent"].Value);
                }

                var gaCode = "<!-- Google Analytics UNIVERSAL will appear here on production servers -->";
                if (PublishFlag)
                {
                    gaCode += "<!-- Google Analytics -->";
                    gaCode += enableCookieConsent ? @"<script type=""text/plain"" class=""cc-onconsent-analytics"">" : @"<script type=""text/javascript"">";
                    gaCode += @"(function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
(i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
})(window,document,'script','//www.google-analytics.com/analytics.js','ga');

ga('create', '" + code + @"', 'auto');
ga('send', 'pageview');

</script>
<!-- End Google Analytics -->";
                }
                customHeadCode += gaCode;
            }


            if (CurrentWebsite.Settings.ContainsKey("CustomBodyJs"))
            {
                customBodyCode += CurrentWebsite.Settings["CustomBodyJs"].Value;
            }

            if (CurrentWebsite.Settings.ContainsKey("CustomHeadJs"))
            {
                customHeadCode = CurrentWebsite.Settings["CustomHeadJs"].Value;
            }

            if (!string.IsNullOrEmpty(customHeadCode) || !string.IsNullOrEmpty(customBodyCode))
            {
                StringBuilder sb = new StringBuilder();
                HtmlTextWriter tw = new HtmlTextWriter(new System.IO.StringWriter(sb));
                //Render the page to the new HtmlTextWriter which actually writes to the stringbuilder
                base.Render(tw);

                //Get the rendered content
                string sContent = sb.ToString();
                if (!string.IsNullOrEmpty(customBodyCode))
                    sContent = sContent.Replace("</body>", customBodyCode + "</body>");
                if (!string.IsNullOrEmpty(customHeadCode))
                    sContent = sContent.Replace("</head>", customHeadCode + "</head>");

                //Now output it to the page, if you want
                writer.Write(sContent);
            }
            else
            {
                base.Render(writer);
            }
        }

        protected void AddMetaTag(string name, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            { 
                var HtmlMetaTag = new HtmlMeta();
                HtmlMetaTag.Name = name;
                HtmlMetaTag.Content = StringTool.StripHtmlTags(content);
                Page.Header.Controls.Add(HtmlMetaTag);
            }
        }

        protected void AddMetaProperty(string name, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var HtmlMetaTag = new HtmlMeta();
                HtmlMetaTag.Attributes.Add("property", name);
                HtmlMetaTag.Content = StringTool.StripHtmlTags(content);
                Page.Header.Controls.Add(HtmlMetaTag);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.IsRedirected)
                {
                    log.Info("RedirectToUrl to external link: " + CurrentPage.RedirectToUrl);
                    Response.Redirect(CurrentPage.RedirectToUrl);
                }

                /*
                if (!PublishFlag && Master.Controls.Count > 2)
                {
                    var control = LoadControl("~/Controls/AdminWikiMenu.ascx");
                    Master.Controls[3].Controls.AddAt(0, control);
                } */

                if (CurrentPage.RequireSSL && !Request.IsSecureConnection)
                {
                    Response.StatusCode = 403;
                    Response.StatusDescription =
                        @"This error indicates that the page you are trying to access is secured with Secure Sockets Layer (SSL). In order to view it, you need to enable SSL by typing https:// at the beginning of the address you are attempting to reach.";
                    Response.Write("403.5 Forbidden: SSL Required\n<br/>");
                    Response.Write(Response.StatusDescription);
                    log.Error("-OnInit " + Response.Status);
                    Response.End();
                }

                if (!CurrentPage.IsViewableByCurrentPrincipal)
                {
                    if (!FormsAuthentication.LoginUrl.Contains("/Account/Login.aspx"))
                    {
                        // redirect to login page only if it is not the default one.
                        Response.Redirect(FormsAuthentication.LoginUrl, true);
                    }
                    else
                    {
                        Response.StatusCode = 403;
                        Response.StatusDescription = "Authorization required (IsViewableByCurrentPrincipal)";
                        Response.Write(Response.StatusDescription);
                        log.Info("-OnInit 403 (IsViewableByCurrentPrincipal)");
                        Response.End();
                    }
                }
                if (!PublishFlag)
                    InsertDebugBanner();

                try
                {
                    Form.Attributes.Add("class",
                        Thread.CurrentThread.CurrentCulture.Name + " page" + CurrentPage.Id + " depth" + CurrentPage.Level +
                        " " + CurrentPage.parentPagesSimpleList + " " + (Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated ? "isAuth" : "notAuth") + " T" + (CurrentPage.Template != null ? CurrentPage.Template.Name : ""));
                }
                catch
                { }

                log.Debug("-OnInit (load ModuleInstances)");

                Dictionary<int, ContentPlaceHolder> contentPlaceHolders = new Dictionary<int, ContentPlaceHolder>(10);

                foreach (BOPlaceHolder placeHolder in CurrentPage.PlaceHolders.Values)
                {
                    ContentPlaceHolder contentPlaceHolder = (ContentPlaceHolder)Master.FindControl(placeHolder.Name);
                    if (contentPlaceHolder != null)
                    {
                        contentPlaceHolders.Add(placeHolder.Id.Value, contentPlaceHolder);

                        foreach (BOModuleInstance module in placeHolder.ModuleInstances)
                        {
                            if (module.PersistFrom <= CurrentPage.Level && (PublishFlag || !module.PendingDelete))
                            {
                                var p = new Section();
                                p.CssClass = "mi " + module.Name.ToLower();
                                p.CssClass += " mi" + module.Id;
                                

                                Control control = null;
                                string relPath = "~/CommonModules/" + module.ModuleSource;
                                string relCustomPath = "~/";

                                if (!string.IsNullOrEmpty(CustomModulesFolder))
                                    relCustomPath += CustomModulesFolder + "/" + module.ModuleSource;

                                try
                                {
                                    if (!string.IsNullOrEmpty(CustomModulesFolder) &&
                                        File.Exists(OContext.Current.MapPath(relCustomPath)))
                                        control = LoadControl(relCustomPath);
                                    else if (File.Exists(OContext.Current.MapPath(relPath)))
                                        control = LoadControl(relPath);
                                    else
                                        control = LoadControl("~/Controls/Blank.ascx");
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Error while loading module", ex);
                                    Literal message = new Literal();
                                    message.Text = "<h3 style=\"color:red;\">Error while loading module</h3>";
                                    if (!PublishFlag)
                                    {
                                        message.Text += "<h4>" + ex.Message + "</h4>";
                                        p.BorderStyle = BorderStyle.Solid;
                                        p.BorderWidth = 1;
                                        p.BorderColor = Color.Red;
                                    }
                                    control = message;
                                }

                                if (null != control)
                                    p.Controls.Add(control);

                                MModule mod = null;

                                if (control is MModule)
                                    mod = (MModule)control;
                                else if (control is PartialCachingControl &&
                                         ((PartialCachingControl)control).CachedControl != null)
                                    mod = (MModule)((PartialCachingControl)control).CachedControl;

                                if (mod != null)
                                {
                                    if (!string.IsNullOrEmpty(mod.ExtraCssClass))
                                        p.CssClass += " " + mod.ExtraCssClass;

                                    mod.InstanceId = module.Id;
                                    mod.PageId = module.PageId;
                                    mod.WebSiteId = CurrentPage.WebSiteId;
                                    mod.WebSiteTitle = CurrentWebsite.Title;
                                    mod.Settings = module.Settings;
                                    mod.RelativePageUri = CurrentPage.URI;
                                    activeModules.Add(mod);
                                }
                                contentPlaceHolders[module.PlaceHolderId].Controls.Add(p);
                            }
                        }
                    }
                }

                base.OnInit(e);
                log.Debug("+OnInit (finished loading ModuleInstances)");
            }
            else
            {
                log.Error("-OnInit 404");
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if(!PublishFlag)
            { 
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
        }

        private void InsertDebugBanner()
        {
            Literal lit = new Literal();
            Version version = Page.GetType().BaseType.Assembly.GetName().Version;
            string szHtml = string.Format(@"<div class=""preview_banner"" style=""z-index: 100; padding: 10px 10px 10px 10px; Position: absolute;Bottom: 0;Right: 0; 
Background: transparent;Filter: Alpha(Opacity=60);-moz-opacity:.60;opacity:.60; background-color: Gray; "">
<span style=""font-size: 100%"">One.NET v{0}</span><br/>
<span style=""font-size: 100%"">{1} {2}</span>
</div>", version, Request.Browser.Browser, Request.Browser.Version);
            lit.Text = szHtml;
            if (Master.Controls.Count > 1)
                Master.Controls.AddAt(Master.Controls.Count - 2, lit);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            // Load ContentId for comments module
            // Only one module on a page can be a ICommentProvider


            var providedDescription = "";
            var providedKeywords = "";
            var providedTitle = "";
            var providedOgImage = "";
            var providedPageName = "";
            Dictionary<string, string> providedLinkTags = new Dictionary<string, string>();
            Dictionary<string, string> providedMetaTags = new Dictionary<string, string>();

            foreach (MModule mod in activeModules)
            {
                if (mod is IImageListConsumer)
                {
                    IImageListConsumer gallery = (IImageListConsumer)mod;
                    gallery.Images = imagesOnThisPage;
                }

                if (mod is IBasicSEOProvider)
                {
                    IBasicSEOProvider pageNameProvider = (IBasicSEOProvider)mod;
                    if (pageNameProvider.HasTitle)
                        providedPageName += pageNameProvider.Title+ " ";
                    if (pageNameProvider.HasDescription)
                        providedDescription += pageNameProvider.Description + " ";
                }

                if (mod is IImageListProvider)
                {
                    IImageListProvider imgListProv = (IImageListProvider)mod;
                    if (imgListProv.ListImages != null)
                        imagesOnThisPage.AddRange(imgListProv.ListImages);
                }
                
                if (mod is IMetaDataProvider)
                {
                    IMetaDataProvider tempMetaDataProvider = (IMetaDataProvider)mod;

                    if (tempMetaDataProvider.ExtraLinkTags != null)
                    {
                        if (providedLinkTags == null)
                            providedLinkTags = tempMetaDataProvider.ExtraLinkTags;
                        else
                        {
                            foreach (string key in tempMetaDataProvider.ExtraLinkTags.Keys)
                                providedLinkTags.Add(key, tempMetaDataProvider.ExtraLinkTags[key]);
                        }
                    }

                    if (tempMetaDataProvider.ExtraMetaData != null)
                    {
                        if (providedMetaTags == null)
                            providedMetaTags = tempMetaDataProvider.ExtraMetaData;
                        else
                        {
                            foreach (string key in tempMetaDataProvider.ExtraMetaData.Keys)
                                providedMetaTags.Add(key, tempMetaDataProvider.ExtraMetaData[key]);
                        }
                    }
                }
            }

            try
            {
                RenderDescription(providedDescription);
                RenderTitle(providedPageName);
                RenderOgImage(providedOgImage);
                RenderMetaData();
                RenderKeywords(providedKeywords);

                if (providedLinkTags != null && providedLinkTags.Keys != null && providedLinkTags.Keys.Count > 0)
                {
                    foreach (string key in providedLinkTags.Keys)
                    {
                        var linkTag = new HtmlLink();
                        linkTag.Attributes.Add("rel", HttpUtility.HtmlEncode(key));
                        linkTag.Href = HttpUtility.HtmlEncode(providedLinkTags[key]);
                        Header.Controls.Add(linkTag);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Header", ex);
            }

            base.OnLoadComplete(e);
        }

        protected void RenderKeywords(string provided)
        {
            var keywords = "";
            var webSiteKeywords = CurrentWebsite.GetSettingValue("MetaKeywords");
            var pageKeywords = CurrentPage.GetSettingValue("MetaKeywords");
            if (!string.IsNullOrWhiteSpace(webSiteKeywords))
                keywords = webSiteKeywords;
            if (!string.IsNullOrWhiteSpace(pageKeywords))
                keywords = pageKeywords;
            if (!string.IsNullOrWhiteSpace(provided))
                keywords = provided;
            AddMetaTag("keywords", keywords);
        }

        protected void RenderOgImage(string provided)
        {
            var ogImage = "";
            if (!string.IsNullOrWhiteSpace(CurrentWebsite.DefaultOgImage))
                ogImage = CurrentWebsite.DefaultOgImage;
            if (!string.IsNullOrWhiteSpace(CurrentPage.OgImage))
                ogImage = CurrentPage.OgImage;
            if (!string.IsNullOrWhiteSpace(provided))
                ogImage = provided;
            AddMetaProperty("og:image", ogImage);
        }

        protected void RenderTitle(string provided)
        {
            // 1. provided
            // 2. page
            var title = "";
            if (!string.IsNullOrWhiteSpace(CurrentPage.Title))
                title = CurrentPage.Title;
            if (!string.IsNullOrWhiteSpace(provided))
                title = provided;

            title = title.Replace('\n', ' ').Replace('\r', ' ');
            title = StringTool.StripHtmlTags(title);
            AddMetaProperty("og:title", title);
            Header.Title = title;
        }

        protected void RenderDescription(string provided)
        {
            var description = "";
            if (!string.IsNullOrWhiteSpace(CurrentPage.Teaser))
                description = CurrentPage.Teaser;
            if (!string.IsNullOrWhiteSpace(provided))
                description = provided;

            description = description.Replace('\n', ' ').Replace('\r', ' ');
            description = StringTool.StripHtmlTags(description);

            if (description.Length > 1000)
                description = description.Substring(0, 1000);
            AddMetaProperty("og:description", description);

            if (description.Length > 160)
                description = description.Substring(0, 160);
            AddMetaTag("description", description);
        }

        protected void RenderMetaData()
        {
            log.Debug("-OnInit (add Meta Data)");
            if(CurrentPage != null)
            { 
                HtmlMeta metaUnicode = new HtmlMeta();
                metaUnicode.HttpEquiv = "Content-Type";
                metaUnicode.Content = "text/html; charset=utf-8";
                Page.Header.Controls.Add(metaUnicode);

                AddMetaProperty("og:site_name", CurrentWebsite.Title);
                AddMetaProperty("og:locale", Thread.CurrentThread.CurrentCulture.Name.Replace('-', '_'));
                AddMetaProperty("og:url", Request.Url.AbsoluteUri);
                var appId = CurrentWebsite.GetSettingValue("FacebookApplicationID");
                AddMetaProperty("fb:app_id", appId);
                var webmasterToolsId = CurrentWebsite.GetSettingValue("GoogleSiteVerification");
                AddMetaTag("google-site-verification", webmasterToolsId);
                if (!PublishFlag)
                {
                    AddMetaTag("robots", "noindex,nofollow");
                }
                else
                {
                    AddMetaTag("robots", (CurrentPage.RobotsIndex ? "index" : "noindex") + "," + (bool.Parse(CurrentPage.Settings["RobotsFollow"].Value) ? "follow" : "nofollow"));
                }

                if (CurrentWebsite.Settings.ContainsKey("RSSChannels"))
                {
                    string rssChannels = CurrentWebsite.Settings["RSSChannels"].Value;
                    List<int> rssIds = StringTool.SplitStringToIntegers(rssChannels);
                    BRssFeed rssFeedB = new BRssFeed();
                    string pagePath = "/Utils/Rss.aspx?id=";
                    foreach (int id in rssIds)
                    {
                        BORssFeed feed = rssFeedB.Get(id);
                        if (feed != null)
                        {
                            HtmlLink rssLink = new HtmlLink();
                            rssLink.Attributes.Add("rel", "alternate");
                            rssLink.Attributes.Add("description", feed.Title);
                            rssLink.Attributes.Add("type", "application/rss+xml");
                            rssLink.Href = pagePath + feed.Id;
                            Page.Header.Controls.Add(rssLink);
                        }
                    }
                }
            }
        }
    }
}
