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

namespace OneMainWeb
{
    public partial class OneMain : System.Web.UI.MasterPage
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OneMain));
        WebProfile profile = null;
        private AuthenticationMode currentMode;

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
                if (Session["SelectedWebSiteId"] != null)
                {
                    return Int32.Parse(Session["SelectedWebSiteId"].ToString());
                }
                else
                {
                    Session["SelectedWebSiteId"] = profile.SelectedWebSiteId;
                    return profile.SelectedWebSiteId;
                }
            }
            set
            {
                Session["SelectedWebSiteId"] = value;
                profile.SelectedWebSiteId = value;
                profile.Save();
            }
        }

        protected string SelectedCulture
        {
            get
            {
                if (Session["SelectedCulture"] != null)
                {
                    return Session["SelectedCulture"].ToString();
                }
                else
                {
                    Session["SelectedCulture"] = profile.SelectedCulture;
                    return profile.SelectedCulture;
                }
            }
            set
            {
                Session["SelectedCulture"] = value;
                profile.SelectedCulture = value;
                profile.Save();
            }
        }

        protected int SelectedCultureId
        {
            get
            {
                if (Session["SelectedCultureId"] != null)
                {
                    return Int32.Parse(Session["SelectedCultureId"].ToString());
                }
                else
                {
                    Session["SelectedCultureId"] = profile.SelectedCultureId;
                    return profile.SelectedCultureId;
                }
            }
            set
            {
                Session["SelectedCultureId"] = value;
                profile.SelectedCultureId = value;
                profile.Save();
            }
        }

        protected int SelectedPageId
        {
            get
            {
                if (Session["SelectedPageId"] != null)
                {
                    return Int32.Parse(Session["SelectedPageId"].ToString());
                }
                else
                {
                    Session["SelectedPageId"] = profile.SelectedPageId;
                    return profile.SelectedPageId;
                }
            }
            set
            {
                Session["SelectedPageId"] = value;
                profile.SelectedPageId = value;
                profile.Save();
            }
        }

        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

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

            profile = new WebProfile(ProfileBase.Create(Page.User.Identity.Name, Page.User.Identity.IsAuthenticated));
            if (!IsPostBack || SelectedCulture != CultureInfo.CurrentCulture.Name)
            {
                SelectedCulture = CultureInfo.CurrentCulture.Name;
                SelectedCultureId = CultureInfo.CurrentCulture.LCID;
            }    

            if (!IsPostBack)
            {
                BWebsite webSiteB = new BWebsite();
                List<BOWebSite> webSiteList = webSiteB.List();

                var DropDownListWebSiteCombined = LoginView1.FindControl("DropDownListWebSiteCombined") as DropDownList;
                var LabelWebSiteDescription = LoginView1.FindControl("LabelWebSiteDescription") as Label;

                DropDownListWebSiteCombined.Items.Clear();
                bool webSiteIsSelected = false;
                foreach (BOWebSite webSite in webSiteList)
                {
                    foreach (int languageId in webSite.Languages)
                    {
                        CultureInfo TempCultureInfo = new CultureInfo(languageId);
                        int selectedRootPageId = webSite.RootPageId.HasValue ? webSite.RootPageId.Value : 0;

                        ListItem DDListItem =
                            new ListItem(webSite.Title + " [" + TempCultureInfo.ThreeLetterISOLanguageName + "]",
                                         webSite.Id + "/" + TempCultureInfo.Name + "/" + selectedRootPageId);
                        if (SelectedWebSiteId == webSite.Id && SelectedCultureId == languageId)
                        {
                            DDListItem.Selected = webSiteIsSelected = true;
                        }
                        bool adminRole = Roles.IsUserInRole("admin");
                        if (adminRole || currentMode == AuthenticationMode.Windows || Roles.IsUserInRole("website_" + webSite.Title.ToLower()))
                        {
                            DropDownListWebSiteCombined.Items.Add(DDListItem);
                        }
                    }
                }
                if (DropDownListWebSiteCombined.Items.Count < 2)
                {
                    DropDownListWebSiteCombined.Visible = false;
                    if (DropDownListWebSiteCombined.Items.Count == 1)
                    {
                        LabelWebSiteDescription.Visible = true;
                        LabelWebSiteDescription.Text = DropDownListWebSiteCombined.Items[0].Text;
                        SelectSiteFromEncodedString(DropDownListWebSiteCombined.Items[0].Value);
                        webSiteIsSelected = true;
                    }
                }

                if (!webSiteIsSelected && webSiteList.Count > 0)
                {
                    SelectedWebSiteId = Int32.Parse(ConfigurationManager.AppSettings["WebSiteId"]);
                    CheckWebSiteRole();
                    int selectedLanguageId = webSiteB.Get(SelectedWebSiteId).PrimaryLanguageId;
                    SelectedCultureId = selectedLanguageId;
                    SelectedCulture = (new CultureInfo(SelectedCultureId)).Name;
                    if (Request["redirect"] == null)
                    {
                        log.Info("redirecting from !webSiteIsSelected");
                        Response.Redirect(Request.Url.LocalPath + "?redirect=true");
                    }
                }
            }

            VirtualTableList1.Visible = false;
            if (Page.User.IsInRole("ScaffoldVirtualTables") || Page.User.IsInRole("admin"))
                VirtualTableList1.Visible = true;
        }
        
        private void CheckWebSiteRole()
        {
            var webSiteB = new BWebsite();
            // the following code is used to fix specific issue where web.config specified website is 
            // loaded on first load to users that may not have permissions to view this website.
            var website = webSiteB.Get(SelectedWebSiteId);
            if (website != null)
            {
                AuthenticationSection authenticationSection = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
                AuthenticationMode currentMode = AuthenticationMode.Windows;
                if (authenticationSection != null)
                    currentMode = authenticationSection.Mode;

                bool adminRole = Roles.IsUserInRole("admin");
                if (!(adminRole || currentMode == AuthenticationMode.Windows || Roles.IsUserInRole("website_" + website.Title.ToLower())))
                {
                    // website with id SelectedWebSiteId is not accessible to user - so use another website id.
                    List<BOWebSite> webSiteList = webSiteB.List();
                    foreach (BOWebSite webSite2 in webSiteList)
                    {
                        if (Roles.IsUserInRole("website_" + webSite2.Title.ToLower()))
                        {
                            SelectedWebSiteId = webSite2.Id;
                            break;
                        }
                    }
                }
            }
        }

        protected void DropDownListWebSiteCombined_SelectedIndexChanged(object sender, EventArgs e)
        {
            var DropDownListWebSiteCombined = LoginView1.FindControl("DropDownListWebSiteCombined") as DropDownList;
            SelectSiteFromEncodedString(DropDownListWebSiteCombined.SelectedValue);
        }

        private void SelectSiteFromEncodedString(string encoded)
        {
            string[] Temp = encoded.Split(new char[] { '/' });
            CultureInfo selectedCulture = new CultureInfo(Temp[1]);

            if (SelectedCulture == selectedCulture.Name && SelectedCultureId == selectedCulture.LCID && SelectedWebSiteId == Int32.Parse(Temp[0]))
                return; // no change required
            else 
            {
                SelectedCultureId = selectedCulture.LCID;
                SelectedCulture = selectedCulture.Name;
                SelectedWebSiteId = Int32.Parse(Temp[0]);
                SelectedPageId = Int32.Parse(Temp[2]);
                if (Request["redirect"] == null)
                {
                    UrlBuilder builder = new UrlBuilder(Request.Url);
                    builder.QueryString["redirect"] = "true";
                    log.Info("redirecting from SelectSiteFromEncodedString");
                    builder.Navigate();
                    //Response.Redirect(builder.ToString());
                    //Response.Redirect(Request.Url.LocalPath + "?redirect=true");
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            OCache.Clear();
            OneSiteMapProvider.ReloadSiteMap();
        }


        public static bool FileExists(string searchString, Page page)
        {
            WebClient client = new WebClient();
            UrlBuilder builder = new UrlBuilder(page);
            builder.Path = searchString;
            try
            {
                byte[] data = client.DownloadData(builder.ToString());
                return data.Length > -1;
            }
            catch (Exception)
            {
                // Debug here?
            }
            return false;
        }
    }
}
