using One.Net.BLL;
using One.Net.BLL.Model.Attributes;
using One.Net.BLL.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
                GridViewModules_DataBind();
            }
        }

        protected void GridViewModules_DataBind()
        {
            var modules = BWebsite.ListAvailibleModules(HttpContext.Current);

            foreach (DataRow dr in modules.Rows)
            {
                var moduleName = dr["Name"] + ".ascx";
                var moduleOnDisk = GetDiskModuleInfo(HttpContext.Current, moduleName);
                if (moduleOnDisk != null && moduleOnDisk.Item2 != null)
                {
                    dr["NoSettingsInModule"] = moduleOnDisk.Item2.Count();
                }
            }

            GridViewModules.DataSource = modules;
            GridViewModules.DataBind();
        }

        public Tuple<string, IEnumerable<Setting>> GetDiskModuleInfo(HttpContext context, string fileName)
        {
            Control control = null;

            string relPath = "~/CommonModules/" + fileName;
            string relCustomPath = "~/site_specific/custom_modules/" + fileName;

            try
            {
                if (File.Exists(HttpContext.Current.Server.MapPath(relCustomPath)))
                    control = LoadControl(relCustomPath);
                else if (File.Exists(HttpContext.Current.Server.MapPath(relPath)))
                    control = LoadControl(relPath);
                else
                    return null;
            }
            catch (Exception ex)
            {
                // "<li>error loading: " + f.Name + " " + ex.Message + "</li>";
                return new Tuple<string, IEnumerable<Setting>>(fileName, null);
            }

            var moduleSettings = new List<Setting>();
            var propCollection = control.GetType().GetProperties();
            foreach (PropertyInfo property in propCollection)
            {
                foreach (var att in property.GetCustomAttributes(true))
                {
                    if (att is Setting)
                    {
                        var a = (Setting)att;
                        a.Name = property.Name;
                        moduleSettings.Add(a);
                    }
                }
            }
            return new Tuple<string, IEnumerable<Setting>>(fileName, moduleSettings);
        }

        protected void GridViewModules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = 0;
            Int32.TryParse(e.CommandArgument.ToString(), out id);

            switch (e.CommandName.ToLower())
            {
                case "install":
                    var name1 = e.CommandArgument.ToString();
                    if (!string.IsNullOrWhiteSpace(name1))
                    {
                        var loadedModule = GetDiskModuleInfo(HttpContext.Current, name1 + ".ascx");

                        var result = BWebsite.AddModule(name1);
                        if (result)
                            Notifier1.Title = "Module added.";
                        else
                            Notifier1.Warning = "Not added.";
                        BWebsite webSiteB = new BWebsite();
                        webSiteB.ClearCache();
                        GridViewModules_DataBind();
                    }
                    break;
                case "updatesettings":
                    var name2 = e.CommandArgument.ToString();
                    if (!string.IsNullOrWhiteSpace(name2))
                    {
                        var loadedModule = GetDiskModuleInfo(HttpContext.Current, name2 + ".ascx");

                        var result2 = BWebsite.UpdateModuleSettings(name2, loadedModule.Item2);

                        Notifier1.Title = "Updated settings: " + result2;
                        if (result2 > 0)
                        {
                            BWebsite webSiteB = new BWebsite();
                            webSiteB.ClearCache();
                            GridViewModules_DataBind();
                        }
                    }
                    break;
                case "showinstances":
                    SelectedModule = BWebsite.ListModules(true).Where(m => m.Id == id).FirstOrDefault();
                    if (SelectedModule != null)
                    {
                        var usage = webSiteB.ListModuleUsage(SelectedModule.Id);
                        GridViewUsage.DataSource = usage;
                        GridViewUsage.DataBind();
                        Multiview1.ActiveViewIndex = 1;
                    }
                    break;
            }
        }

        protected void GridViewModules_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var LinkButtonUpdateSettings = e.Row.FindControl("LinkButtonUpdateSettings") as LinkButton;
                var LinkButtonShowInstances = e.Row.FindControl("LinkButtonShowInstances") as LinkButton;
                var LinkButtonInstall = e.Row.FindControl("LinkButtonInstall") as LinkButton;
                var LinkButtonDelete = e.Row.FindControl("LinkButtonDelete") as LinkButton;
                
                var row = e.Row.DataItem as DataRowView;

                LinkButtonShowInstances.Visible = !string.IsNullOrWhiteSpace(row["id"].ToString());
                var  noSettingsInModule = -1;
                if (!string.IsNullOrWhiteSpace(row["NoSettingsInModule"].ToString()))
                {
                    noSettingsInModule = int.Parse(row["NoSettingsInModule"].ToString());
                }


                var noUnpublishedInstances = -1;
                if (!string.IsNullOrWhiteSpace(row["NoUnpublishedInstances"].ToString()))
                {
                    noUnpublishedInstances = int.Parse(row["NoUnpublishedInstances"].ToString());
                }

                LinkButtonInstall.Visible = noUnpublishedInstances < 0;
                LinkButtonUpdateSettings.Visible = noUnpublishedInstances >= 0 && noSettingsInModule > 0;

                var noPublishedInstances = -1;
                if (!string.IsNullOrWhiteSpace(row["NoPublishedInstances"].ToString()))
                {
                    noPublishedInstances = int.Parse(row["NoPublishedInstances"].ToString());
                }

                LinkButtonDelete.Visible = false; // !(noPublishedInstances > 0 || noUnpublishedInstances > 0);

            }
        }
    }
}