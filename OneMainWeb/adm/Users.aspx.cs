using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OneMainWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
// using Microsoft.AspNet.Identity.Owin;
using NLog;
using One.Net.BLL;
using OtpSharp;
using Base32;
using System.Configuration;

namespace OneMainWeb.adm
{
    public partial class Users : OneBasePage
    {

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

        protected string SelectedUser
        {
            get 
            {
                return ViewState["SelectedUser"] != null ? (string)ViewState["SelectedUser"] : "";
            }
            set 
            {
                ViewState["SelectedUser"] = value;
            }
        }

        protected string SecretKey
        {
            get
            {
                return ViewState["SecretKey"] != null ? (string)ViewState["SecretKey"] : "";
            }
            set
            {
                ViewState["SecretKey"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MultiView1.ActiveViewIndex = 0;
            }
        }

        protected void Users_DataBind()
        {
            GridViewUsers.DataSource = IdentityManager.ListUsers();
            GridViewUsers.DataBind();
        }

        protected void User_DataBind()
        {
            LabelUsername.Text = SelectedUser;

            var regularRoles = IdentityManager.ListRoles().Where(r => !r.Name.StartsWith("http"));
            var websiteRoles = IdentityManager.ListRoles().Where(r => r.Name.StartsWith("http"));

            var roles = new List<IdentityRole>();
            roles.AddRange(regularRoles.OrderBy(r => r.Name));
            roles.AddRange(websiteRoles.OrderBy(r => r.Name));
            RepeaterRoles.DataSource = roles;
            RepeaterRoles.DataBind();
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            IdentityManager.ClearUserRoles(SelectedUser);
            for (int i = 0; i < RepeaterRoles.Items.Count; i++)
            {
                var Checkbox1 = (CheckBox)RepeaterRoles.Items[i].FindControl("Checkbox1");
                if (Checkbox1.Checked)
                {
                    IdentityManager.AddUserToRole(SelectedUser, Checkbox1.Text);
                }
            }
        }

        protected void GridViewUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GridViewUsers.SelectedValue != null)
            {
                SelectedUser = GridViewUsers.SelectedValue.ToString();
                     
                User_DataBind();
                MultiView1.ActiveViewIndex = 1;
            }
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {
            switch (MultiView1.ActiveViewIndex)
            { 
                case 0:
                    Users_DataBind();
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }

        }

        protected void RepeaterRoles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var item = e.Item.DataItem as IdentityRole;
            var userRoles = IdentityManager.ListUserRoles(SelectedUser);
            if (item != null)
            {
                var CheckBox1 = e.Item.FindControl("Checkbox1") as CheckBox;

                var temp = userRoles.Where(r => r.RoleId == item.Id).FirstOrDefault();
                if (temp != null)
                {
                    CheckBox1.Checked = true;
                }
                else
                {
                    CheckBox1.Checked = false;
                }
            }
        }

        protected void ButtonAddUser_Click(object sender, EventArgs e)
        {
            var publishFlag = PresentBasePage.ReadPublishFlag();

            if (!publishFlag)
            {
                MultiView1.ActiveViewIndex = 2;
            }
        }

        protected void ButtonSaveUser_Click(object sender, EventArgs e)
        {
            var manager = new UserManager();

            var user = new OneNetUser()
            {
                UserName = TextBoxUsername.Text,
                Email = TextBoxEmail.Text
            };

            IdentityResult result = manager.Create(user, TextBoxPassword.Text);

            Notifier1.Title = "Add user: ";

            if (result.Succeeded)
            {
                SelectedUser = TextBoxUsername.Text;
                User_DataBind();
                MultiView1.ActiveViewIndex = 1;

                Notifier1.Message = "User successfully created";
            }
            else
            {
                Notifier1.Warning = "Failed to create user.";
                foreach (var error in result.Errors)
                {
                    Notifier1.Warning += "<br />";
                    Notifier1.Warning += error;
                }
            }
        }

        protected void ButtonUpdateRoles_Click(object sender, EventArgs e)
        {
            var publishFlag = PresentBasePage.ReadPublishFlag();

            if (!publishFlag)
            {
                IdentityManager.CreateRoleIfNotExists("admin");
                IdentityManager.CreateRoleIfNotExists("ContentEdit");
                IdentityManager.CreateRoleIfNotExists("all");
                IdentityManager.CreateRoleIfNotExists("Structure");
                IdentityManager.CreateRoleIfNotExists("Publish");
                IdentityManager.CreateRoleIfNotExists("Scaffold");
                IdentityManager.CreateRoleIfNotExists("Forms");
                IdentityManager.CreateRoleIfNotExists("Articles");
                IdentityManager.CreateRoleIfNotExists("Regulars");
                IdentityManager.CreateRoleIfNotExists("Subscriptions");
                IdentityManager.CreateRoleIfNotExists("Dictionary");
                IdentityManager.CreateRoleIfNotExists("FileManager");
                IdentityManager.CreateRoleIfNotExists("Redirects");
                IdentityManager.CreateRoleIfNotExists("Website");
                IdentityManager.CreateRoleIfNotExists("PublishAllButton");
                IdentityManager.CreateRoleIfNotExists("AllowDeletePage");
                IdentityManager.CreateRoleIfNotExists("AllowDeleteFolder");
                IdentityManager.CreateRoleIfNotExists("Users");

                var websiteB = new BWebsite();
                var list = websiteB.List();

                foreach (var w in list)
                {
                    var previewUrl = w.PreviewUrl;
                    if (!string.IsNullOrWhiteSpace(previewUrl))
                    {
                        IdentityManager.CreateRoleIfNotExists(previewUrl);
                    }
                }

                log.Info("-------------- CreateRoleIfNotExists FINISHED --------------");
            }
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            Notifier1.Title = "Deleting users... ";

            var manager = new UserManager();
            int deletedCount = 0;
            var userIds = GetCheckedIds();

            foreach (var userId in userIds)
            {
                var user = manager.FindById(userId);

                if (user != null)
                {
                    if (!manager.IsInRole(userId, "admin"))
                    {
                        manager.Delete(user);
                        deletedCount++;
                    }
                }
            }

            if (deletedCount > 0)
            {
                Notifier1.Title += string.Format("Deleted {0} users", deletedCount);
                Users_DataBind();
            }
            else
            {
                Notifier1.Title += "Nothing found";
            }
        }

        protected IEnumerable<string> GetCheckedIds()
        {
            var userIds = new List<string>();

            foreach (GridViewRow row in GridViewUsers.Rows)
            {
                var CheckBoxDelete = row.FindControl("CheckBoxDelete") as CheckBox;
                var LiteralUserId = row.FindControl("LiteralUserId") as Literal;

                if (LiteralUserId != null && CheckBoxDelete != null && CheckBoxDelete.Checked)
                {
                    string userId = LiteralUserId.Text;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        userIds.Add(userId);
                    }
                }
            }
            return userIds;
        }

        protected void GridViewUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var CheckBoxDelete = e.Row.FindControl("CheckBoxDelete") as CheckBox;
                var LiteralUserId = e.Row.FindControl("LiteralUserId") as Literal;
                var LinkButton2FA = e.Row.FindControl("LinkButton2FA") as LinkButton;
                var LinkButtonDisable2FA = e.Row.FindControl("LinkButtonDisable2FA") as LinkButton;

                var manager = new UserManager();
                OneNetUser user = null;
                if (LiteralUserId != null)
                {
                    user = manager.FindById(LiteralUserId.Text);
                }
                
                if (CheckBoxDelete != null && LiteralUserId != null && manager.IsInRole(LiteralUserId.Text, "admin"))
                {
                    CheckBoxDelete.Visible = false;
                }
                if (LinkButton2FA != null && user != null && Has2FA)
                {
                    LinkButton2FA.Visible = !user.IsGoogleAuthenticatorEnabled;
                    LinkButtonDisable2FA.Visible = user.IsGoogleAuthenticatorEnabled;
                }  else
                {
                    LinkButton2FA.Visible = LinkButtonDisable2FA.Visible = false;
                }

            }
        }

        protected void GridViewUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Enable2FA")
            {
                MultiView1.ActiveViewIndex = 3;
                SelectedUser = e.CommandArgument.ToString();
                LabelUsername2.Text = SelectedUser;

                byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
                SecretKey = Base32Encoder.Encode(secretKey);
                string barcodeUrl = KeyUrl.GetTotpUrl(secretKey, SelectedUser) + "&issuer=OneNet";

                LiteralCode.Text = string.Format("<img src=\"https://qrcode.kaywa.com/img.php?s=4&d={0}\"/>", barcodeUrl);
            }
            else if (e.CommandName == "Disable2FA")
            {
                var manager = new UserManager();
                var user = manager.FindByName(e.CommandArgument.ToString());
                user.GoogleAuthenticatorSecretKey = SecretKey;
                user.IsGoogleAuthenticatorEnabled = false;
                var result = manager.Update(user);
                Notifier1.Title = "2FA Authentication";
                if (result.Succeeded)
                {
                    Users_DataBind();
                    MultiView1.ActiveViewIndex = 0;
                    Notifier1.Message = "2FA Disabled";
                }
                else
                {
                    Notifier1.Warning = "2FA disable Failed";
                    foreach (var error in result.Errors)
                    {
                        Notifier1.Warning += "<br />";
                        Notifier1.Warning += error;
                    }
                }
            }
        }

        protected void LinkButtonEnable2FA_Click(object sender, EventArgs e)
        {
            var manager = new UserManager();
            var user = manager.FindByName(SelectedUser);
            if (!string.IsNullOrWhiteSpace(TextBox2FACode.Text) && !string.IsNullOrWhiteSpace(SelectedUser) && user != null)
            {
                byte[] secretKey = Base32Encoder.Decode(SecretKey);

                long timeStepMatched = 0;
                var otp = new Totp(secretKey);
                if (otp.VerifyTotp(TextBox2FACode.Text, out timeStepMatched, new VerificationWindow(2, 2)))
                {
                    user.GoogleAuthenticatorSecretKey = SecretKey;
                    user.IsGoogleAuthenticatorEnabled = true;
                    var result = manager.Update(user);
                    Notifier1.Title = "2FA Authentication";

                    if (result.Succeeded)
                    {
                        Users_DataBind();
                        MultiView1.ActiveViewIndex = 0;
                        Notifier1.Message = "2FA Enabled";
                    }
                    else
                    {
                        Notifier1.Warning = "2FA Failed";
                        foreach (var error in result.Errors)
                        {
                            Notifier1.Warning += "<br />";
                            Notifier1.Warning += error;
                        }
                    }
                }
                else
                {
                    Notifier1.Warning = "VerifyTotp Failed";
                }
            }
            else
            {
                throw new Exception("empty code or SelectedUser?");
            }
        }
    }
}