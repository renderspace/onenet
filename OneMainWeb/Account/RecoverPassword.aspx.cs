using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;

using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using OneMainWeb.Models;
using NLog;

namespace OneMainWeb.Account
{
    public partial class RecoverPassword : System.Web.UI.Page
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected string Token { get; set; }
        protected string UserId { get; set; }
        protected string RecoverPasswordReturnLink
        {
            get
            {
                if (ConfigurationManager.AppSettings["RecoverPasswordReturnLink"] != null)
                    return ConfigurationManager.AppSettings["RecoverPasswordReturnLink"];
                else
                    return "Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode("/adm");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request[IdentityHelper.CodeKey] != null)
                Token = Request[IdentityHelper.CodeKey];
            if (Request[IdentityHelper.UserIdKey] != null)
                UserId = Request[IdentityHelper.UserIdKey];

            LoginHyperLink.NavigateUrl = RecoverPasswordReturnLink;

            if (!IsPostBack) 
            {
                if (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(UserId))
                    MultiView1.ActiveViewIndex = 2;
                else
                    MultiView1.ActiveViewIndex = 0;
            }
        }
        
        protected void Recover_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var manager = new UserManager();
                var user = manager.FindByEmail(EmailTextBox.Text);

                if (user != null)
                {
                    log.Info("Recover password request.. " + EmailTextBox.Text);

                    var code = manager.GeneratePasswordResetToken(user.Id);

                    string callbackUrl = IdentityHelper.GetResetPasswordRedirectUrl(code, user.Id, Request);

                    var success = true;
                    try
                    {
                        manager.SendEmail(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>.");
                    }
                    catch (SmtpException ex)
                    {
                        FailureLiteral.Text = "Failed to send email. SMTP exception thrown. Exception error:" + ex.Message;
                        PlaceHolderErrorMessage.Visible = true;
                        success = false;
                    }

                    if (success)
                        MultiView1.ActiveViewIndex = 1;
                }
                else
                {
                    log.Info("Invalid email. " + EmailTextBox.Text);
                    FailureLiteral.Text = "Invalid email.";
                    PlaceHolderErrorMessage.Visible = true;
                }
            }
        }

        protected void Reset_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var manager = new UserManager();
                var user = manager.FindById(UserId);

                if (user != null)
                {
                    log.Info("Reset password request... UserId:" + UserId);

                    var newPassword = NewPasswordTextBox.Text;

                    var result = manager.ResetPassword(user.Id, Token, newPassword);

                    if (result.Errors.Count() > 0)
                    {
                        FailureLiteral2.Text = "Reset failed. Please correct the following errors before proceeding:<br />";
                        foreach (var error in result.Errors)
                        {
                             FailureLiteral2.Text += error + "<br />";
                        }
                        PlaceHolderErrorMessage2.Visible = true;
                    }
                    else
                    {
                        try
                        {
                            manager.SendEmail(user.Id, "Password has been reset", "Your password was successfully reset.");
                        }
                        catch
                        {
                            // don't really want to do anything here, because we are only sending out an email to let the user know that their password was reset ok.
                        }

                        MultiView1.ActiveViewIndex = 3;
                    }
                }
                else
                {
                    log.Info("Invalid email for reset attempt. UserId" + UserId);
                    FailureLiteral2.Text = "Invalid user.";
                    PlaceHolderErrorMessage2.Visible = true;
                }
            }
        }
    }
}