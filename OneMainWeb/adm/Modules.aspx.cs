using One.Net.BLL;
using One.Net.BLL.Model.Attributes;
using One.Net.BLL.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.adm
{
    public partial class Modules : OneBasePage
    {
        protected static BWebsite webSiteB = new BWebsite();

        protected BOModule SelectedModule
        {
            get { return ViewState["SelectedModule"] as BOModule; }
            set { ViewState["SelectedModule"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
            }
        }

        protected void Multiview1_ActiveViewChanged(object sender, EventArgs e)
        {
            if (Multiview1.ActiveViewIndex == 0)
            {
                var modules = BWebsite.ListModules(true).OrderByDescending(m => m.NoUnpublishedInstances);
                GridViewModules.DataSource = modules;
                GridViewModules.DataBind();
            }
        }

        protected void GridViewModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            var grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedModule = BWebsite.ListModules(true).Where(m => m.Id == int.Parse(grid.SelectedValue.ToString())).FirstOrDefault();
                if (SelectedModule != null)
                {
                    var usage = webSiteB.ListModuleUsage(SelectedModule.Id);
                    GridViewUsage.DataSource = usage;
                    GridViewUsage.DataBind();
                    Multiview1.ActiveViewIndex = 1;
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            LiteralResult.Text = "";

           // string relPath = "~/CommonModules/" + module.ModuleSource;
           // string relCustomPath = "~/site_specific/custom_modules/" + module.ModuleSource;

            /*
        if (File.Exists(HttpContext.Current.Server.MapPath(relCustomPath)))
            control = LoadControl(relCustomPath);
        else if (File.Exists(HttpContext.Current.Server.MapPath(relPath)))
            control = LoadControl(relPath);*/

            var files = BFileSystem.ListPhysicalFolder(HttpContext.Current.Server.MapPath("~/CommonModules/"), HttpContext.Current.Server.MapPath("~/"));

            LiteralResult.Text += "<ul>";

            foreach (var f in files.Where(fi => fi.Extension == ".ascx"))
            {
                Control control = null;

                try
                {
                    control = Page.LoadControl("~/CommonModules/" + f.Name);
                }
                catch (Exception ex)
                {
                    LiteralResult.Text += "<li>error loading: " + f.Name + " " + ex.Message + "</li>";
                    continue;
                }

                LiteralResult.Text += "<li>" + f.Name + (control is MModule).ToString();

                var propCollection = control.GetType().GetProperties();
                LiteralResult.Text += "<ul>";
                foreach (PropertyInfo property in propCollection)
                {
                    // LiteralResult.Text += "<li>" + property.Name + "</li>";
                    foreach (var att in property.GetCustomAttributes(true))
                    {
                        if (att is Setting)
                        {
                            LiteralResult.Text += "<li>" + property.Name + " [ " + ((Setting)att).DefaultValue + " / " + Enum.GetName(typeof(SettingType), ((Setting)att).Type) + "] ";
                            if (!string.IsNullOrWhiteSpace(((Setting)att).Options))
                            {
                                LiteralResult.Text += "Options: " + ((Setting)att).Options;
                            }
                            LiteralResult.Text += "</li>";

                        }
                    }
                }
                LiteralResult.Text += "</ul>";
            }
            LiteralResult.Text += "</ul>";
            

            // control.Settings = null;
        }
    }
}