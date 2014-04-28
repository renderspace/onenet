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


namespace OneMainWeb
{
    public class PresentBasePage : Page
    {
        protected string SelectedUICulture
        {
            get
            {
                if (Session["SelectedUICulture"] != null)
                    return Session["SelectedUICulture"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                Session["SelectedUICulture"] = value;
            }
        }

        BOPage page;
        BWebsite websiteB;
        private readonly string customModulesFolder;
        private BOWebSite webSite;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("PresentBasePage");

        private readonly List<MModule> activeModules = new List<MModule>();
        private readonly List<BOIntContImage> imagesOnThisPage = new List<BOIntContImage>();

        private bool publishFlag = false;

        public int PageId
        {
            get
            {
                return (SiteMap.CurrentNode != null) ? FormatTool.GetInteger(SiteMap.CurrentNode["_pageID"]) : -1;
            }
        }

        public bool PublishFlag
        {
            get { return publishFlag; }
            set { publishFlag = value; }
        }

        protected internal string CustomModulesFolder
        {
            get { return customModulesFolder; }
        }

        public PresentBasePage()
        {
            customModulesFolder = ConfigurationManager.AppSettings["customModulesFolder"];
            // main language setup.
            if (SiteMap.CurrentNode != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(int.Parse(SiteMap.CurrentNode["_languageId"]));
            }

            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);

            PreInit += Page_PreInit;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var websiteB = new BWebsite();
            var page = websiteB.GetPage(PageId);
            var webSite = websiteB.Get(page.WebSiteId);

            var customBodyCode = string.Empty;
            var customHeadCode = string.Empty;

            if (webSite.Settings.ContainsKey("GoogleAnalyticsCode"))
            {
                string code = webSite.Settings["GoogleAnalyticsCode"].Value;
                if (!(code.Equals("UA-xxxx-x") || code.Length < 6))
                {
                    var enableCookieConsent = false;
                    if (webSite.Settings.ContainsKey("GAEnableCookieConsent"))
                    {
                        enableCookieConsent = FormatTool.GetBoolean(webSite.Settings["GAEnableCookieConsent"].Value);
                    }

                    Literal gaCode = new Literal();
                    if (PublishFlag)
                    {
                        gaCode.Text += enableCookieConsent ? @"<script type=""text/plain"" class=""cc-onconsent-analytics"">" : @"<script type=""text/javascript"">";
                        gaCode.Text += @"
//<![CDATA[
var _gaq = _gaq || [];
  _gaq.push(['_setAccount', '" + code + @"']);
  _gaq.push(['_trackPageview']);

  (function() {
    var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
    ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
  })();
//]]>
</script>";
                    }
                    else
                        gaCode.Text = "<!-- GoogleAnalyticsCode will appear here on production servers -->";

                    customBodyCode += gaCode.Text;
                }
            }


            if (webSite.Settings.ContainsKey("CustomBodyJs"))
            {
                customBodyCode += webSite.Settings["CustomBodyJs"].Value;
            }

            if (webSite.Settings.ContainsKey("CustomHeadJs"))
            {
                customHeadCode = webSite.Settings["CustomHeadJs"].Value;
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

        protected void Page_PreInit(object sender, EventArgs e)
        {
            websiteB = new BWebsite();
            page = websiteB.GetPage(PageId);
            webSite = websiteB.Get(page.WebSiteId);
        }

        protected override void OnInit(EventArgs e)
        {
            if (page != null)
            {
                log.Debug("-OnInit (add Meta Data)");

                HtmlMeta metaUnicode = new HtmlMeta();
                metaUnicode.HttpEquiv = "Content-Type";
                metaUnicode.Content = "text/html; charset=utf-8";
                Page.Header.Controls.Add(metaUnicode);


                if (page.Settings.ContainsKey("MetaDescription"))
                {
                    HtmlMeta metaDescription = new HtmlMeta();
                    metaDescription.ID = "HtmlMetaDescription";
                    metaDescription.Name = "description";
                    metaDescription.Content = StringTool.StripHtmlTags(page.Settings["MetaDescription"].Value);

                    if (metaDescription.Content.Length > 1)
                        Page.Header.Controls.Add(metaDescription);
                }

                if (page.Settings.ContainsKey("MetaKeywords"))
                {
                    HtmlMeta metaKeywords = new HtmlMeta();
                    metaKeywords.ID = "HtmlMetaKeywords";
                    metaKeywords.Name = "keywords";
                    metaKeywords.Content = StringTool.StripHtmlTags(page.Settings["MetaKeywords"].Value);
                    if (metaKeywords.Content.Length > 1)
                        Page.Header.Controls.Add(metaKeywords);
                }

                if (page.Settings.ContainsKey("PageOgDescription"))
                {
                    HtmlMeta metaOgDescription = new HtmlMeta();
                    metaOgDescription.ID = "OgDescription1";
                    metaOgDescription.Attributes.Add("property", "og:description");
                    metaOgDescription.Content = HttpUtility.HtmlEncode(page.Settings["PageOgDescription"].Value);
                    if (metaOgDescription.Content.Length > 0)
                        Page.Header.Controls.Add(metaOgDescription);
                }

                if (page.Settings.ContainsKey("RobotsIndex") && page.Settings.ContainsKey("RobotsFollow"))
                {
                    HtmlMeta metaRobots = new HtmlMeta();
                    metaRobots.Name = "robots";
                    if (PublishFlag)
                    {
                        metaRobots.Content = bool.Parse(page.Settings["RobotsIndex"].Value) ? "index" : "noindex";
                        metaRobots.Content += ",";
                        metaRobots.Content += bool.Parse(page.Settings["RobotsFollow"].Value) ? "follow" : "nofollow";
                    }
                    else
                        metaRobots.Content = "noindex,nofollow";
                    Page.Header.Controls.Add(metaRobots);
                }

                if (webSite.Settings.ContainsKey("RSSChannels"))
                {
                    string rssChannels = webSite.Settings["RSSChannels"].Value;
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
                            rssLink.Attributes.Add("title", feed.Title);
                            rssLink.Attributes.Add("type", "application/rss+xml");
                            rssLink.Href = pagePath + feed.Id;
                            Page.Header.Controls.Add(rssLink);
                        }
                    }
                }

                if (webSite.Settings.ContainsKey("DefaultOgImage") && !string.IsNullOrEmpty(webSite.Settings["DefaultOgImage"].Value) && !string.IsNullOrEmpty(webSite.Settings["DefaultOgImage"].Value.Trim()))
                {
                    HtmlMeta metaOgImage = new HtmlMeta();
                    metaOgImage.ID = "OgImage";
                    metaOgImage.Attributes.Add("property", "og:image");
                    metaOgImage.Content = HttpUtility.HtmlEncode(webSite.Settings["DefaultOgImage"].Value);
                    if (metaOgImage.Content.Length > 0)
                        Page.Header.Controls.Add(metaOgImage);
                }

                if (page.Settings.ContainsKey("PageOgImage") && !string.IsNullOrEmpty(page.Settings["PageOgImage"].Value.Trim()))
                {
                    var metaOgImage = Page.Header.FindControl("OgImage") as HtmlMeta;

                    if (metaOgImage == null)
                    {
                        metaOgImage = new HtmlMeta();
                        metaOgImage.ID = "OgImage";
                        if (page.Settings["PageOgImage"].Value.Length > 0)
                            Header.Controls.Add(metaOgImage);
                    }

                    metaOgImage.Attributes.Clear();
                    metaOgImage.Attributes.Add("property", "og:image");
                    metaOgImage.Content = HttpUtility.HtmlEncode(page.Settings["PageOgImage"].Value);
                }
                if (page.IsRedirected)
                {
                    log.Info("RedirectToUrl to external link: " + page.RedirectToUrl);
                    Response.Redirect(page.RedirectToUrl);
                }

                if (page.RequireSSL && !Request.IsSecureConnection)
                {
                    Response.StatusCode = 403;
                    Response.StatusDescription =
                        @"This error indicates that the page you are trying to access is secured with Secure Sockets Layer (SSL). In order to view it, you need to enable SSL by typing https:// at the beginning of the address you are attempting to reach.";
                    Response.Write("403.5 Forbidden: SSL Required\n<br/>");
                    Response.Write(Response.StatusDescription);
                    log.Error("-OnInit " + Response.Status);
                    Response.End();
                }

                if (!page.IsViewableByCurrentPrincipal)
                {
                    if (!FormsAuthentication.LoginUrl.Contains("/login.aspx"))
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
                if (!publishFlag)
                    InsertDebugBanner();

                try
                {
                    Form.Attributes.Add("class",
                        Thread.CurrentThread.CurrentCulture.Name + " page" + page.Id + " depth" + page.Level +
                        " " + page.parentPagesSimpleList + " " + (Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated ? "isAuth" : "notAuth") + " T" + (page.Template != null ? page.Template.Name : ""));
                }
                catch
                { }

                log.Debug("-OnInit (load ModuleInstances)");

                Dictionary<int, ContentPlaceHolder> contentPlaceHolders = new Dictionary<int, ContentPlaceHolder>(10);

                foreach (BOPlaceHolder placeHolder in page.PlaceHolders.Values)
                {
                    ContentPlaceHolder contentPlaceHolder = (ContentPlaceHolder)Master.FindControl(placeHolder.Name);
                    if (contentPlaceHolder != null)
                    {
                        contentPlaceHolders.Add(placeHolder.Id.Value, contentPlaceHolder);

                        foreach (BOModuleInstance module in placeHolder.ModuleInstances)
                        {
                            if (module.PersistFrom <= page.Level && (PublishFlag || !module.PendingDelete))
                            {
                                Panel p = new Panel();
                                p.CssClass = "mi " + module.Name.ToLower();
                                //p.ToolTip = "ModuleID: " + module.ModuleId.ToString();
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
                                    if (!publishFlag)
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

                                bool isViewable = true;

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
                                    mod.WebSiteId = page.WebSiteId;
                                    mod.WebSiteTitle = webSite.Title;
                                    mod.Settings = module.Settings;
                                    mod.RelativePageUri = this.page.URI;
                                    isViewable = mod.IsViewableByCurrentPrincipal;

                                    if (isViewable)
                                        activeModules.Add(mod);
                                }
                                if (isViewable)
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

        protected override void InitializeCulture()
        {
            if (string.IsNullOrEmpty(SelectedUICulture))
            {
                SelectedUICulture = ConfigurationManager.AppSettings["PreferredUICulture"].ToString();
            }

            if (!string.IsNullOrEmpty(SelectedUICulture) && SelectedUICulture != "Auto")
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(SelectedUICulture);
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
        }

        private void InsertDebugBanner()
        {
            Literal lit = new Literal();
            Version version = Page.GetType().BaseType.Assembly.GetName().Version;
            string szHtml = string.Format(@"<div class=""preview_banner"" style=""z-index: 100; padding: 10px 10px 10px 10px; Position: absolute;Top: 0;Right: 0; 
Background: transparent;Filter: Alpha(Opacity=60);-moz-opacity:.60;opacity:.60; background-color: Gray; "">
<span style=""color:Red; font-size: 300%"">{0}</span><br/>
<span style=""font-size: 100%"">One.NET v{1}</span><br/>
<span style=""font-size: 100%"">{2} {3}</span>
</div>", BContent.GetMeaning("site_preview"), version, Request.Browser.Browser, Request.Browser.Version);
            lit.Text = szHtml;
            if (Master.Controls.Count > 1)
                Master.Controls.AddAt(Master.Controls.Count - 2, lit);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            // Load ContentId for comments module
            // Only one module on a page can be a ICommentProvider
            IContentIdProvider contentIdProvider = null;
            IArticleIdProvider articleIdProvider = null;
            IDefaultArticleIdProvider defaultArticleIdProvider = null;
            IRegularIdProvider regularIdProvider = null;

            string providedPageName = "";
            string providedDescription = "";
            string providedKeywords = "";
            string providedTitle = "";
            Dictionary<string, string> providedLinkTags = new Dictionary<string, string>();
            Dictionary<string, string> providedMetaTags = new Dictionary<string, string>();

            foreach (MModule mod in activeModules)
            {
                if (mod is IImageListProvider)
                {
                    IImageListProvider imgListProv = (IImageListProvider)mod;
                    if (imgListProv.ListImages != null)
                        imagesOnThisPage.AddRange(imgListProv.ListImages);
                }
                if (mod is IPageNameProvider)
                {
                    IPageNameProvider pageNameProvider = (IPageNameProvider)mod;
                    if (pageNameProvider.HasPageName)
                        providedPageName += pageNameProvider.PageName + " ";
                }
                if (mod is IContentIdProvider)
                {
                    IContentIdProvider tempContentIdProvider = (IContentIdProvider)mod;
                    if (tempContentIdProvider.EnableContentIdProvider)
                        contentIdProvider = tempContentIdProvider;
                }
                if (mod is IArticleIdProvider)
                {
                    IArticleIdProvider tempArticleIdProvider = (IArticleIdProvider)mod;
                    if (tempArticleIdProvider.EnableArticleIdProvider)
                        articleIdProvider = tempArticleIdProvider;
                }
                if (mod is IRegularIdProvider)
                {
                    IRegularIdProvider tempRegularIdProvider = (IRegularIdProvider)mod;
                    if (tempRegularIdProvider.EnableRegularIdProvider)
                        regularIdProvider = tempRegularIdProvider;
                }
                if (mod is IDefaultArticleIdProvider)
                {
                    IDefaultArticleIdProvider tempArticleIdProvider = (IDefaultArticleIdProvider)mod;
                    if (tempArticleIdProvider.EnableDefaultArticleIdProvider)
                        defaultArticleIdProvider = tempArticleIdProvider;
                }
                if (mod is ILeadImageProvider)
                {
                    ILeadImageProvider tempLeadImageProvider = (ILeadImageProvider)mod;
                    if (tempLeadImageProvider.EnableLeadImageProvider)
                    {
                        if (providedLinkTags == null)
                            providedLinkTags = new Dictionary<string, string>();
                        providedLinkTags.Add("image_src", tempLeadImageProvider.ImageUri);
                        if (providedMetaTags == null)
                            providedMetaTags = new Dictionary<string, string>();
                        providedMetaTags.Add("og:image", tempLeadImageProvider.ImageUri);
                        providedMetaTags.Add("og:description", tempLeadImageProvider.ImageDescription);
                        providedMetaTags.Add("description", tempLeadImageProvider.ImageDescription);
                    }
                }
                if (mod is IMetaDataProvider)
                {
                    IMetaDataProvider tempMetaDataProvider = (IMetaDataProvider)mod;

                    if (tempMetaDataProvider.HasDescription)
                        providedDescription += tempMetaDataProvider.MetaDescription + " ";
                    if (tempMetaDataProvider.HasKeyWords)
                        providedKeywords += tempMetaDataProvider.MetaKeyWords + " ";
                    if (tempMetaDataProvider.HasTitle)
                        providedTitle += tempMetaDataProvider.MetaTitle + " ";


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

            if (defaultArticleIdProvider != null)
            {
                foreach (MModule mod in activeModules)
                {
                    if (mod is IDefaultArticleIdConsumer)
                    {
                        IDefaultArticleIdConsumer consumer = mod as IDefaultArticleIdConsumer;
                        consumer.DefaultArticleId = defaultArticleIdProvider.DefaultArticleId;
                        log.Info("IDefaultArticleIdConsumer wired to module instance " + mod.InstanceId);
                    }
                }
            }

            // find a contentid consumer module and assign it the contentIdProvider contentId
            if (contentIdProvider != null)
            {
                foreach (MModule mod in activeModules)
                {
                    if (mod is IContentIdConsumer)
                    {
                        IContentIdConsumer consumer = mod as IContentIdConsumer;
                        consumer.ContentId = contentIdProvider.ContentId;
                        consumer.Title = contentIdProvider.Title;
                        consumer.TeaserImageId = contentIdProvider.TeaserImageId;
                        consumer.Teaser = contentIdProvider.Teaser;
                        log.Info("IContentIdProvider wired to module instance " + mod.InstanceId);
                    }
                }
            }

            if (articleIdProvider != null)
            {
                foreach (MModule mod in activeModules)
                {
                    if (mod is IArticleIdConsumer)
                    {
                        IArticleIdConsumer consumer = mod as IArticleIdConsumer;
                        consumer.ArticleId = articleIdProvider.ArticleId;
                        log.Info("IArticleIdConsumer wired to module instance " + mod.InstanceId);
                    }
                }
            }

            if (regularIdProvider != null)
            {
                foreach (MModule mod in activeModules)
                {
                    if (mod is IRegularIdConsumer)
                    {
                        IRegularIdConsumer consumer = mod as IRegularIdConsumer;
                        consumer.RegularIds = regularIdProvider.RegularIds;
                        log.Info("IRegularIdConsumer wired to module instance " + mod.InstanceId);
                    }
                }
            }

            log.Info("IImageListProviders provided " + imagesOnThisPage.Count + " images");
            foreach (MModule mod in activeModules)
            {
                if (mod is IImageListConsumer)
                {
                    IImageListConsumer gallery = (IImageListConsumer)mod;
                    gallery.Images = imagesOnThisPage;
                }
            }

            try
            {
                if (providedDescription.Length > 0)
                {
                    var metaDescription = FindControl("HtmlMetaDescription") as HtmlMeta;
                    if (metaDescription == null)
                    {
                        metaDescription = new HtmlMeta();
                        metaDescription.Name = "description";
                        metaDescription.Content = StringTool.StripHtmlTags(providedDescription);
                        Header.Controls.Add(metaDescription);
                    }
                    else
                    {
                        metaDescription.Content += " " + StringTool.StripHtmlTags(providedDescription);
                    }
                }
                if (providedKeywords.Length > 0)
                {
                    var metaKeywords = FindControl("HtmlMetaKeywords") as HtmlMeta;
                    if (metaKeywords == null)
                    {
                        metaKeywords = new HtmlMeta();
                        metaKeywords.Name = "keywords";
                        metaKeywords.Content = StringTool.StripHtmlTags(providedKeywords);
                        Header.Controls.Add(metaKeywords);
                    }
                    else
                    {
                        metaKeywords.Content += " " + StringTool.StripHtmlTags(providedKeywords);
                    }
                }
                //var tempTitle = DetermingPageTitle(webSite, providedTitle);
                //if (tempTitle.Length > 0)
                //{
                //    var metaTitle = new HtmlMeta();
                //    metaTitle.Name = "title";
                //    metaTitle.Content = StringTool.StripHtmlTags(tempTitle);
                //    Header.Controls.Add(metaTitle);
                //}

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

                if (providedMetaTags != null && providedMetaTags.Keys != null && providedMetaTags.Keys.Count > 0)
                {
                    foreach (string key in providedMetaTags.Keys)
                    {
                        HtmlMeta metaControl = null;
                        string controlName = string.Empty;

                        if (key.ToLowerInvariant() == "og:image")
                            controlName = "OgImage";
                        else if (key.ToLowerInvariant() == "og:description")
                            controlName = "OgDescription";

                        if (!string.IsNullOrEmpty(controlName))
                        {
                            metaControl = Page.Header.FindControl(controlName) as HtmlMeta;

                            if (metaControl == null)
                            {
                                metaControl = new HtmlMeta();
                                metaControl.ID = controlName;
                                Header.Controls.Add(metaControl);
                            }

                            metaControl.Attributes.Clear();
                            metaControl.Attributes["property"] = HttpUtility.HtmlEncode(key);
                            metaControl.Content = HttpUtility.HtmlEncode(providedMetaTags[key]);
                        }
                    }
                }

                var pageTitle = DetermingPageTitle(webSite, providedPageName);

                bool addMetaOgTitle = false;
                var metaOgTitle = Page.Header.FindControl("OgTitle") as HtmlMeta;
                if (metaOgTitle == null)
                {
                    addMetaOgTitle = true;
                    metaOgTitle = new HtmlMeta();
                }
                metaOgTitle.ID = "OgTitle";
                metaOgTitle.Attributes.Add("property", "og:title");
                if (string.IsNullOrEmpty(page.Settings["OgTitle"].Value) && string.IsNullOrEmpty(page.Settings["OgTitle"].Value.Trim()))
                    metaOgTitle.Content = pageTitle;
                else
                    metaOgTitle.Content = HttpUtility.HtmlEncode(page.Settings["OgTitle"].Value);

                if (addMetaOgTitle)
                    Page.Header.Controls.Add(metaOgTitle);
                Header.Title = pageTitle;

            }
            catch (Exception ex)
            {
                log.Fatal("Header", ex);
            }

            base.OnLoadComplete(e);
        }

        private static string DetermingPageTitle(BOWebSite webSite, string providedPageName)
        {
            var pageTitle = "";
            var titlePrefix = "";
            if (SiteMap.CurrentNode["_pageID"] == SiteMap.RootNode["_pageID"])
            {
                if (webSite.Settings.ContainsKey("Headline") && !string.IsNullOrEmpty(webSite.Settings["Headline"].Value) && !string.IsNullOrEmpty(webSite.Settings["Headline"].Value.Trim()))
                    titlePrefix = webSite.Settings["Headline"].Value;
            }

            var CondensedPageTitle = false;
            if (webSite.Settings.ContainsKey("CondensedPageTitle") && !string.IsNullOrEmpty(webSite.Settings["CondensedPageTitle"].Value) && !string.IsNullOrEmpty(webSite.Settings["CondensedPageTitle"].Value.Trim()))
                Boolean.TryParse(webSite.Settings["CondensedPageTitle"].Value, out CondensedPageTitle);

            var MaxPageTitleDepth = 6;
            if (webSite.Settings.ContainsKey("MaxPageTitleDepth") && !string.IsNullOrEmpty(webSite.Settings["MaxPageTitleDepth"].Value) && !string.IsNullOrEmpty(webSite.Settings["MaxPageTitleDepth"].Value.Trim()))
                Int32.TryParse(webSite.Settings["MaxPageTitleDepth"].Value, out MaxPageTitleDepth);

            if (CondensedPageTitle)
            {
                var currentLevel = FormatTool.GetInteger(SiteMap.CurrentNode["_absDepth"]);

                if (currentLevel == 0)
                    pageTitle = webSite.Title;
                else if (currentLevel == 1)
                    pageTitle = SiteMap.CurrentNode["_pageTitle"] + " - " + webSite.Title;
                else if (currentLevel <= MaxPageTitleDepth)
                    pageTitle = SiteMap.CurrentNode["_pageTitle"] + " - " + FindLevelTitle(Int32.Parse(SiteMap.CurrentNode["_pageID"]), 1) + " - " + webSite.Title;
                else
                {
                    if (MaxPageTitleDepth == 1)
                        pageTitle = FindLevelTitle(Int32.Parse(SiteMap.CurrentNode["_pageID"]), 1) + " - " + webSite.Title;
                    else
                        pageTitle = FindLevelTitle(Int32.Parse(SiteMap.CurrentNode["_pageID"]), MaxPageTitleDepth) + " - " + FindLevelTitle(Int32.Parse(SiteMap.CurrentNode["_pageID"]), 1) + " - " + webSite.Title;
                }

            }
            else
            {
                if (providedPageName.Length == 0)
                    pageTitle = webSite.Title + " - " + SiteMap.CurrentNode["_pageTitle"];
                else
                    pageTitle = webSite.Title + " - " + providedPageName;
            }

            return titlePrefix + pageTitle;
        }

        private static string FindLevelTitle(int pageId, int level)
        {
            var websiteB = new BWebsite();
            var page = websiteB.GetPage(pageId);
            if (page.IsRoot)
                return "";
            if (page.Level == level)
                return page.Title;
            return FindLevelTitle(page.ParentId.Value, level);
        }
    }
}
