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
using OtpSharp;
using Base32;
using System.Configuration;

namespace OneMainWeb.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        protected bool Has2FA
        {
            get
            {
                if (ConfigurationManager.AppSettings["Enabled2FA"] == null)
                {
                    return false;
                }
                return bool.Parse(ConfigurationManager.AppSettings["Enabled2FA"].ToString());
            }
        }

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
            
            if (!IsPostBack)
            {
                Panel2FA.Visible = Has2FA;
            }
        }

        protected void LogIn(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var manager = new UserManager();
                OneNetUser user = manager.Find(UserName.Text, Password.Text);

                if (user == null)
                {
                    log.Info("Invalid username or password. " + UserName.Text);
                    FailureText.Text = "Invalid username or password.";
                    ErrorMessage.Visible = true;
                }
                else if (user.IsGoogleAuthenticatorEnabled && this.Has2FA)
                {
                    long timeStepMatched = 0;
                    var otp = new Totp(Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey));
                    bool valid = otp.VerifyTotp(TextBoxCode.Text, out timeStepMatched, new VerificationWindow(2, 2));
                    if (valid)
                    {
                        IdentityHelper.SignIn(manager, user, RememberMe.Checked);
                        IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                    }
                    else
                    {
                        log.Info("Two factor authentication code is not valid. ");
                        FailureText.Text = "Two factor authentication code is not valid. Please try again!";
                        ErrorMessage.Visible = true;
                    }
                }
                else
                {
                    log.Info("SignIn and RedirectToReturnUrl.. " + UserName.Text);
                    IdentityHelper.SignIn(manager, user, RememberMe.Checked);
                    IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                }
            }
        }
    }
}