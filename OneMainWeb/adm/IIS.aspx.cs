using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices;
using Microsoft.Web;
using Microsoft.Web.Administration;
using One.Net.BLL;

namespace OneMainWeb.adm
{
    public partial class IIS : OneBasePage
    {
        private const string POOLPREFIX = "ONE_NET_POOL_";

        protected string IISRootDirectory { get {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["IISRootDirectory"]))
                throw new Exception("Missing IISRootDirectory from web.config");
            return ConfigurationManager.AppSettings["IISRootDirectory"]; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                tabMultiview.SetActiveIndex(0);
            }
        }

        private static long GetNewSiteId(Microsoft.Web.Administration.ServerManager sm)
        {
            long id = 1;
            foreach (Site site in sm.Sites)
            {
                if (site.Id > id)
                    id = site.Id;
            }
            id++;
            return id;
        }

        protected void TabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            MultiView multiView = sender as MultiView;
            if (multiView.ActiveViewIndex == 0)
            {
                GridViewAppPoolsDataBind();
            }
            else if (multiView.ActiveViewIndex == 1)
            {
                var sm = new Microsoft.Web.Administration.ServerManager();
                GridViewWebsitesDataBind();

                DropDownListAppPools.DataSource = sm.ApplicationPools;
                DropDownListAppPools.DataTextField = "Name";
                DropDownListAppPools.DataValueField = "Name";
                DropDownListAppPools.DataBind();

                LabelAppPools.Text = ResourceManager.GetString("$select_app_pool");
            }
        }

        private void GridViewWebsitesDataBind()
        {
            var sm = new Microsoft.Web.Administration.ServerManager();
            GridViewWebsites.DataSource = sm.Sites;
            GridViewWebsites.DataBind();
        }

        private void GridViewAppPoolsDataBind()
        {
            var sm = new Microsoft.Web.Administration.ServerManager();
            GridViewAppPools.DataSource = sm.ApplicationPools;
            GridViewAppPools.DataBind();
        }

        protected void GridViewWebsites_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var LiteralPath = e.Row.FindControl("LiteralPath") as Literal;
            var LiteralAppPool = e.Row.FindControl("LiteralAppPool") as Literal;
            var Site = e.Row.DataItem as Site;

            if (LiteralPath != null && LiteralAppPool != null && Site != null)
            {
                LiteralAppPool.Text = Site.Applications[0].ApplicationPoolName;
                LiteralPath.Text = Site.Applications[0].Path;
            }
        }

        protected void GridViewAppPools_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var LiteralState = e.Row.FindControl("LiteralState") as Literal;
            var AppPool = e.Row.DataItem as ApplicationPool;

            if (LiteralState != null && AppPool != null)
            {
                // LiteralState.Text = AppPool.State.ToString();
            }
        }

        protected void CreateNewAppPool_Click(object sender, EventArgs e)
        {
            var sm = new Microsoft.Web.Administration.ServerManager();
            var poolName = POOLPREFIX + InputAppPoolName.Value;

            if (sm.ApplicationPools.AllowsAdd)
            {
                try
                {
                    if (!sm.ApplicationPools.Any(t => t.Name == poolName))
                    {
                        // Add a new application pool
                        var appPool = sm.ApplicationPools.Add(poolName);

                        // Configure my new app pool to start automatically.
                        appPool.AutoStart = true;
                        // What action should IIS take when my app pool exceeds 
                        // the CPU limit specified by the Limit property
                        appPool.Cpu.Action = ProcessorAction.KillW3wp;

                        // Use the Integrated Pipeline mode
                        appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

                        // Set the runtime version of ASP.NET
                        appPool.ManagedRuntimeVersion = InputAppPoolRuntime.Value;

                        // Use the Network Service account
                        appPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;

                        // Shut down after being idle for 10 minutes.
                        appPool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(10);

                        // Max. number of IIS worker processes (W3WP.EXE)
                        appPool.ProcessModel.MaxProcesses = 1;

                        // Commit the changes
                        sm.CommitChanges();
                        GridViewAppPoolsDataBind();

                        notifier.Message = string.Format(ResourceManager.GetString("$application_pool_added"));
                        notifier.Visible = true;
                    }
                    else
                    {
                        notifier.Warning = string.Format(ResourceManager.GetString("$application_pool_exists_already"));
                        notifier.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    notifier.Warning = string.Format(ResourceManager.GetString("$application_pool_add_failed"), poolName, ex.Message);
                    notifier.Visible = true;
                }
            }
            else
            {
                notifier.Warning = ResourceManager.GetString("$application_pool_add_not_allowed");
                notifier.Visible = true;
            }
        }

        protected void CreateNewWebSite_Click(object sender, EventArgs e)
        {
            var sm = new Microsoft.Web.Administration.ServerManager();
            var siteName = InputWebSiteName.Value;
            var poolName = DropDownListAppPools.SelectedValue;

            if (sm.Sites.AllowsAdd)
            {
                try
                {
                    if (!sm.Sites.Any(t => t.Name == siteName))
                    {
                        var site = sm.Sites.CreateElement("site");
                        site.SetAttributeValue("name", siteName);
                        site.Name = siteName;
                        site.Id = GetNewSiteId(sm);
                        sm.Sites.Add(site);

                        var app = site.Applications.CreateElement();
                        app.ApplicationPoolName = poolName;
                        app.Path = "/";
                        site.Applications.Add(app);

                        app.VirtualDirectories.Add("/", IISRootDirectory + @"\" + siteName);

                        site.Bindings.Clear();
                        site.Bindings.Add(":80:" + siteName, "http");

                        sm.CommitChanges();

                        notifier.Message = string.Format(ResourceManager.GetString("$web_site_added"));
                        notifier.Visible = true;

                        GridViewWebsitesDataBind();
                    }
                    else
                    {
                        notifier.Warning = string.Format(ResourceManager.GetString("$website_exists_already"));
                        notifier.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    notifier.Warning = string.Format(ResourceManager.GetString("$web_site_add_failed"), siteName, ex.Message);
                    notifier.Visible = true;
                }
            }
            else
            {
                notifier.Warning = ResourceManager.GetString("$web_site_add_not_allowed");
                notifier.Visible = true;
            }
        }
    }
}