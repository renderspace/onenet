using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OneMainWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OneMainWeb.adm
{
    public partial class Users : System.Web.UI.Page
    {
        protected string SelectedUser
        {
            get 
            {
                if (GridViewUsers.SelectedValue == null)
                    return "";
                return GridViewUsers.SelectedValue.ToString(); 
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
            if (!string.IsNullOrWhiteSpace(SelectedUser))
            {
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
            }

        }

        protected void RepeaterRoles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var item = e.Item.DataItem as IdentityRole;
            var userRoles = IdentityManager.ListUserRoles(SelectedUser);
            if (item != null)
            {
                var CheckBox1 = e.Item.FindControl("Checkbox1") as CheckBox;

                var temp = userRoles.Where(r => r.Role.Name == item.Name).FirstOrDefault();
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

        protected void ButtonCancel_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }
    }
}