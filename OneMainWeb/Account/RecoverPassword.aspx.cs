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
    public partial class RecoverPassword : System.Web.UI.Page
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void Recover_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var manager = new UserManager();
                var user = manager.FindByEmail(Email.Text);

                if (user != null)
                {
                    log.Info("Recover password request.. " + Email.Text);

                    var code = manager.GeneratePasswordResetToken(user.Id);

                    string callbackUrl = IdentityHelper.GetResetPasswordRedirectUrl(code, user.Id, Request);
                    manager.SendEmail(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>.");
                }
                else
                {
                    log.Info("Invalid email. " + Email.Text);
                    FailureText.Text = "Invalid email.";
                    ErrorMessage.Visible = true;
                }
            }
        }
    }
}