using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OneMainWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using One.Net.BLL;

namespace OneMainWeb.adm
{
    public partial class Users : OneBasePage
    {
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
    }
}