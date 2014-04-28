using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using One.Net.BLL;

namespace OneMainWeb
{
    public partial class Login : System.Web.UI.Page
    {
        protected Version version;

        protected bool HasBrowserError { get; set; }

        protected string AppVersion
        {
            get 
            {
                if (version == null)
                    version = Page.GetType().BaseType.Assembly.GetName().Version;

                return version.Major + "." + version.Minor + "." + version.Build; //+ version.
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            CheckBrowser();

            Page.Title = ResourceManager.GetString("$login");
        }

        private void CheckBrowser()
        {
            // Note: We don't support opera at all
            HasBrowserError = true;
            if (Request.Browser.Type.ToLower().Contains("ie") && Request.Browser.MajorVersion > 8)
            {
                // We support IE9 and up
                HasBrowserError = false;
            }
            else if (Request.Browser.Type.ToLower().Contains("safari") && Request.Browser.MajorVersion >= 5)
            {
                // Safari 5 and up.
                HasBrowserError = false;                
            }
            else if (Request.Browser.Type.ToLower().Contains("chrome") && Request.Browser.MajorVersion > 25)
            {
                // Chrome 26 and up
                HasBrowserError = false;
            }
            else if (Request.Browser.Type.ToLower().Contains("firefox") && Request.Browser.MajorVersion > 21)
            {
                // Firefox 21 and up
                HasBrowserError = false;
            }
        }

        protected void Login1_LoggedIn(object sender, EventArgs e)
        {
            if (Request["ReturnUrl"] != null && Request["ReturnUrl"].Contains("site_specific"))
            {
                Response.Redirect("/");
            }
        }
    }
}
