using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Profile;
using System.Globalization;
using System.Threading;
using System.Web.Optimization;

using One.Net.BLL;
using System.Linq;
using System.Net;
using One.Net.BLL.Utility;
using OneMainWeb.adm;

namespace OneMainWeb
{
    public partial class OneMain : System.Web.UI.MasterPage
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OneMain));

        protected Version version;

        protected string AppVersion
        {
            get
            {
                if (version == null)
                    version = Page.GetType().BaseType.Assembly.GetName().Version;
                return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision; //+ version.
            }
        }

        protected int SelectedWebSiteId
        {
            get
            {
                return authorizationHelper.SelectedWebSiteId;
            }
            set
            {
                authorizationHelper.SelectedWebSiteId = value;
            }
        }

        protected string TracingFlag
        {
            get { return (!PresentBasePage.ReadPublishFlag()).ToString().ToLower(); }
        }

        private AuthorizationHelper authorizationHelper = null;


        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        public OneMain()
        {
            authorizationHelper = new AuthorizationHelper(Context);
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            Context.GetOwinContext().Authentication.SignOut();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LiteralAppVersion.Text = AppVersion;
            LiteralHead.Text = Styles.Render("~/Bundles/BoostrapCSS").ToHtmlString();
            LiteralHead.Text += Styles.Render("~/Bundles/JqueryUI").ToHtmlString();
            LiteralHead.Text += Styles.Render("~/Bundles/AdmCSS").ToHtmlString();
            LiteralHead.Text += @"
    <script>
        tracing = " + TracingFlag + @"; 
        languageId = " + Thread.CurrentThread.CurrentCulture.LCID.ToString() + @";
    </script>";
            LiteralHead.Text += Scripts.Render("~/Bundles/Scripts").ToHtmlString();

            LiteralFoot.Text = Scripts.Render("~/Bundles/AdmJS").ToHtmlString(); 

            if (!IsPostBack)
            {
                var webSiteList = authorizationHelper.ListAllowedWebsites();

                bool webSiteIsSelected = false;
                foreach (BOWebSite webSite in webSiteList)
                {
                    ListItem DDListItem = new ListItem(webSite.DisplayName, webSite.Id.ToString());
                    if (SelectedWebSiteId == webSite.Id)
                    {
                        DDListItem.Selected = webSiteIsSelected = true;
                    }
                    DropDownListWebSiteCombined.Items.Add(DDListItem);
                }
                DropDownListWebSiteCombined.DataBind();

                if (!webSiteIsSelected && !Request.Url.AbsoluteUri.Contains("/adm/Website.aspx?newsite=true"))
                {
                    if (!webSiteList.Any() && (authorizationHelper.IsInRole("admin") || authorizationHelper.IsInRole("Website")) )
                    {
                        Response.Redirect("/adm/Website.aspx?newsite=true");
                    }
                    else if (!webSiteList.Any())
                    {
                        Response.Redirect("/");
                    }
                    else
                    {
                        SelectedWebSiteId = webSiteList.First().Id;
                    }
                }
                Menu1.Visible = MainContent.Visible = !PresentBasePage.ReadPublishFlag();
                VirtualTableList1.Visible = false;
                if (MainContent.Visible  && (Page.User.IsInRole("Scaffold") || Page.User.IsInRole("admin")))
                    VirtualTableList1.Visible = true;
            }
        }

        protected void DropDownListWebSiteCombined_SelectedIndexChanged(object sender, EventArgs e)
        {
            var numberOfRedirects = 0;
            if (Request["redirect"] != null)
            {
                int.TryParse(Request["redirect"].ToString(), out numberOfRedirects);
            }

            var websiteId = 0;
            int.TryParse(DropDownListWebSiteCombined.SelectedValue, out websiteId);
            if (websiteId > 0)
            {
                SelectedWebSiteId = websiteId;
                if (numberOfRedirects < 30)
                {
                    numberOfRedirects++;
                    UrlBuilder builder = new UrlBuilder(Request.Url);
                    builder.QueryString["redirect"] = numberOfRedirects.ToString();
                    log.Info("redirecting from SelectSiteFromEncodedString");
                    builder.Navigate();
                }
                else
                {
                    throw new Exception("Looks like infite redirect. Please refresh page");
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            OCache.Clear();
            OneSiteMapProvider.ReloadSiteMap();
        }
    }
}
