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

using One.Net.BLL;
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

        protected string SelectedCulture
        {
            get
            {
                return authorizationHelper.SelectedCulture;
            }
        }

        protected int SelectedCultureId
        {
            get
            {
                return authorizationHelper.SelectedCultureId;
            }
            set
            {
                authorizationHelper.SelectedCultureId = value;
            }
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

            if (!IsPostBack)
            {
                BWebsite webSiteB = new BWebsite();
                List<BOWebSite> webSiteList = webSiteB.List();

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

                if (!webSiteIsSelected && webSiteList.Count > 0)
                {
                    var requestedWebSiteId = Int32.Parse(ConfigurationManager.AppSettings["WebSiteId"]);
                    var webSite = webSiteB.Get(requestedWebSiteId);
                    if (webSite != null)
                    {
                        SelectedWebSiteId = webSite.Id;
                    }
                    Response.Redirect(Request.Url.LocalPath + "?newsite=true");
                }
            }
                 

            VirtualTableList1.Visible = false;
            if (Page.User.IsInRole("ScaffoldVirtualTables") || Page.User.IsInRole("admin"))
                VirtualTableList1.Visible = true;
        }

        protected void DropDownListWebSiteCombined_SelectedIndexChanged(object sender, EventArgs e)
        {
            var websiteId = 0;
            int.TryParse(DropDownListWebSiteCombined.SelectedValue, out websiteId);
            if (websiteId > 0)
            {
                SelectedWebSiteId = websiteId;
                if (Request["redirect"] == null)
                {
                    UrlBuilder builder = new UrlBuilder(Request.Url);
                    builder.QueryString["redirect"] = "true";
                    log.Info("redirecting from SelectSiteFromEncodedString");
                    builder.Navigate();
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
