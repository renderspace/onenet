using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using OneMainWeb.Models;
using NLog;

namespace OneMainWeb.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "Register.aspx";
            RecoverPasswordHyperLink.NavigateUrl = "RecoverPassword.aspx";

            // OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
                RecoverPasswordHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }
        }

        protected void LogIn(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var manager = new UserManager();
                OneNetUser user = manager.Find(UserName.Text, Password.Text);
                
                if (user != null)
                {
                    log.Info("SignIn and RedirectToReturnUrl.. " + UserName.Text);
                    IdentityHelper.SignIn(manager, user, RememberMe.Checked);
                    IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                }
                else
                {
                    log.Info("Invalid username or password. " + UserName.Text);
                    FailureText.Text = "Invalid username or password.";
                    ErrorMessage.Visible = true;
                }
            }
        }
    }
}