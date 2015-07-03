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
            var control = Page.LoadControl("~/CommonModules/ArticleList.ascx");


            var propCollection = control.GetType().GetProperties();
            Console.WriteLine("Properties of System.Type are:");
            LiteralResult.Text = "<ul>";
            foreach (PropertyInfo property in propCollection)
            {
                // LiteralResult.Text += "<li>" + property.Name + "</li>";
                foreach (var att in property.GetCustomAttributes(true))
                {
                    if (att is Setting)
                    {
                        LiteralResult.Text += "<li>" + property.Name +  " [ " + ((Setting)att).Name + " / " + Enum.GetName(typeof(SettingType), ((Setting)att).Type) + "] </li>";
                    }
                }
            }
            LiteralResult.Text += "<ul>";

            // control.Settings = null;
        }
    }
}