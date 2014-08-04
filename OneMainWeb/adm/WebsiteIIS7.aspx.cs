using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using Microsoft.Web;
using Microsoft.Web.Administration;
using System.Threading;
using System.Security.AccessControl;

using One.Net.BLL;
using MsSqlDBUtility;
using log4net;
using One.Net.BLL.DAL;
using One.Net.BLL.Utility;
using System.Text.RegularExpressions;

namespace OneMainWeb.adm
{
    public partial class WebsiteIIS7 : OneBasePage
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(WebsiteIIS7));

        protected string IISRoot
        {
            get
            {
                return new DirectoryInfo(Server.MapPath("~")).Parent.FullName;
            }        
        }

        protected bool UsingExistingDb
        {
            get
            {
                return ViewState["UsingExistingDb"] != null ? (bool)ViewState["UsingExistingDb"] : false;
            }
            set
            {
                ViewState["UsingExistingDb"] = value;
            }
        }

        protected string DbServer
        {
            get
            {
                return ViewState["DbServer"] != null ? (string)ViewState["DbServer"] : "";
            }
            set
            {
                ViewState["DbServer"] = value;
            }
        }

        protected string DbName
        {
            get
            {
                return ViewState["DbName"] != null ? (string)ViewState["DbName"] : "";
            }
            set
            {
                ViewState["DbName"] = value;
            }
        }

        protected string DbUserName
        {
            get
            {
                return ViewState["DbUserName"] != null ? (string)ViewState["DbUserName"] : "";
            }
            set
            {
                ViewState["DbUserName"] = value;
            }
        }

        protected string DbPassword
        {
            get
            {
                return ViewState["DbPassword"] != null ? (string)ViewState["DbPassword"] : "";
            }
            set
            {
                ViewState["DbPassword"] = value;
            }
        }

        protected string MasterDbUserName
        {
            get
            {
                return ViewState["MasterDbUserName"] != null ? (string)ViewState["MasterDbUserName"] : "";
            }
            set
            {
                ViewState["MasterDbUserName"] = value;
            }
        }

        protected string MasterDbPassword
        {
            get
            {
                return ViewState["MasterDbPassword"] != null ? (string)ViewState["MasterDbPassword"] : "";
            }
            set
            {
                ViewState["MasterDbPassword"] = value;
            }
        }

        private List<BOIISWebSite> Websites
        {
            get { return ViewState["Websites"] != null ? ViewState["Websites"] as List<BOIISWebSite> : null; }
            set { ViewState["Websites"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStep0();
            }
        }

        protected void Wizard1_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (Wizard1.ActiveStepIndex)
            {
                case 0: LoadStep0(); break;
                case 1: LoadStep1(); break;
                case 2: LoadStep2(); break;
                case 3: LoadStep3(); break;
                default: break;
            }
        }

        private void LoadStep0()
        {
            AssignValidationGroup("database_mode");
        }

        private void LoadStep1()
        {
            var button = GetNextButton();
            if (button != null)
            {
                button.Text = ResourceManager.GetString("$create_new_db");
                button.Visible = true;
            }

            AssignValidationGroup("database_new");
        }

        private void LoadStep2()
        {
            var button = GetNextButton();
            if (button != null)
            {
                button.Text = ResourceManager.GetString("$next");
                button.Visible = true;
            }

            DropDownListExistingDatabaseName.Items.Clear();
            var names = DataBaseConnectionHelper.ListDatabases(InputDbServer.Text, MasterDbUserName, MasterDbPassword);
            foreach (var name in names)
                DropDownListExistingDatabaseName.Items.Add(new ListItem(name, name));

            AssignValidationGroup("database_existing");
        }
        
        private void LoadStep3()
        {
            if (DropDownListLcid.Items.Count == 0)
            {
                var languages = DataBaseConnectionHelper.ListLanguages(DbServer, DbName, DbUserName, DbPassword);
                foreach (var language in languages)
                    DropDownListLcid.Items.Add(new ListItem(language.ToString(), language.ToString()));
            }

            GridViewWebsitesLoad(true);

            if (UsingExistingDb)
            {
                // using an existing db
                if (Websites != null && Websites.Count > 0)
                    InputAppPoolName.Text = Websites[0].AppPoolName;
            }
            else
            {
                // new db has been created
                InputAppPoolName.Text = "";
            }

            // next button not needed any more
            var button = GetNextButton();
            button.Visible = false;
        }

        private void GridViewWebsitesLoad(bool reloadData)
        {
            if (Websites == null || reloadData)
                Websites = DataBaseConnectionHelper.ListWebsites(DbServer, DbName, DbUserName, DbPassword);
            GridViewWebsites.DataSource = Websites;
            GridViewWebsites.DataBind();
        }

        protected void GridViewWebsites_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridViewWebsites.EditIndex = e.NewEditIndex;
            GridViewWebsitesLoad(false);
        }

        protected void GridViewWebsites_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridViewWebsites.EditIndex = -1;
            GridViewWebsitesLoad(false);
        }

        protected void GridViewWebsites_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = GridViewWebsites.Rows[e.RowIndex];
            
            BOIISWebSite website = null;
            if (Websites != null)
                website = Websites[row.DataItemIndex];

            var hostHeaderBox = row.Cells[3].Controls[0] as TextBox;

            if (website != null && hostHeaderBox != null)
            {
                var id = website.Id;
                var hostHeaders = hostHeaderBox.Text;

                website = DataBaseConnectionHelper.GetWebSite(id, DbServer, DbName, DbUserName, DbPassword);

                if (website != null)
                {
                    website.HostHeader = hostHeaders;
                    DataBaseConnectionHelper.ChangeWebsite(website, DbServer, DbName, DbUserName, DbPassword);

                    var sm = new Microsoft.Web.Administration.ServerManager();
                    if (sm.Sites.Any(s => s.Name == website.Title))
                    {
                        var site = sm.Sites.First(s => s.Name == website.Title);
                        if (site != null)
                        {
                            site.Bindings.Clear();

                            var headers = StringTool.SplitString(hostHeaders, ';');
                            foreach (string header in headers) {
                                if (!string.IsNullOrWhiteSpace(header))
                                {
                                    var binding = site.Bindings.CreateElement();
                                    binding.BindingInformation = "*" + ":80:" + header;
                                    binding.Protocol = "http";
                                    site.Bindings.Add(binding);
                                }
                            }

                            sm.CommitChanges();
                        }
                    }

                    GridViewWebsites.EditIndex = -1;
                    GridViewWebsitesLoad(true);
                }
            }
        }

        protected void ButtonAddWebsite_Click(object sender, EventArgs e)
        {
            var siteSpecificName = InputSiteSpecificName.Text;
            var siteName = InputWebSiteName.Text + ".w.renderspace.net";
            var lcid = Int32.Parse(DropDownListLcid.SelectedValue);
            var hostHeaders = InputHostHeader.Text;
            var appPoolName = InputAppPoolName.Text;

            using (var sm = new Microsoft.Web.Administration.ServerManager())
            {
                if (!sm.Sites.AllowsAdd)
                {
                    notifier.Visible = true;
                    notifier.Warning = ResourceManager.GetString("$iis_doesnt_allow_site_add");
                }
                else if (sm.Sites.Any(t => t.Name == siteName))
                {
                    notifier.Visible = true;
                    notifier.Warning = ResourceManager.GetString("$iis_site_already_exists");
                }
                else if (DataBaseConnectionHelper.FindWebSite(siteName, DbServer, DbName, DbUserName, DbPassword) != null)
                {
                    notifier.Visible = true;
                    notifier.Warning = ResourceManager.GetString("$website_already_exists_in_database");
                }
                else
                {
                    var website = new BOIISWebSite();
                    website.Title = siteName;
                    website.SubTitle = "";
                    website.Teaser = "";
                    website.Html = "";
                    website.LanguageId = lcid;
                    website.ContentId = null;
                    website.PrincipalCreated = User.Identity.Name;
                    website.HostHeader = hostHeaders;
                    website.AppPoolName = appPoolName;

                    DataBaseConnectionHelper.ChangeWebsite(website, DbServer, DbName, DbUserName, DbPassword);

                    // copy over files and folders
                    DirectoryHelper.DirectoryCopy(Server.MapPath("~"), Path.Combine(IISRoot, siteName), true, UsingExistingDb ? false : true);

                    DirectoryInfo appRoot = new DirectoryInfo(Path.Combine(IISRoot, siteName));

                    var dirSec = appRoot.GetAccessControl();
                    // Remove inherited permissions
                    dirSec.SetAccessRuleProtection(false, true);
                    dirSec.AddAccessRule(new FileSystemAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
                    appRoot.SetAccessControl(dirSec);

                    // create site_specific directory
                    var siteSpecificInfo = new DirectoryInfo(Path.Combine(Path.Combine(IISRoot, siteName), "site_specific"));
                    if (!siteSpecificInfo.Exists)
                        siteSpecificInfo.Create();

                    DirectoryInfo insideSiteSpecificInfo = new DirectoryInfo(Path.Combine(Path.Combine(Path.Combine(IISRoot, siteName), "site_specific"), siteSpecificName));
                    if (!insideSiteSpecificInfo.Exists)
                        insideSiteSpecificInfo.Create();

                    DirectoryInfo aspxTemplatesInfo = new DirectoryInfo(Path.Combine(Path.Combine(Path.Combine(Path.Combine(IISRoot, siteName), "site_specific"), siteSpecificName), "aspx_templates"));
                    if (!aspxTemplatesInfo.Exists)
                        aspxTemplatesInfo.Create();

                    DirectoryHelper.DirectoryCopy(Path.Combine(Path.Combine(IISRoot, siteName), "aspx_templates"), aspxTemplatesInfo.FullName, false, false);

                    if (!IISHelper.AppPoolExists(sm, appPoolName))
                    {
                        IISHelper.CreateNewAppPool(sm, appPoolName, "v4.5");
                    }

                    if (!string.IsNullOrEmpty(appPoolName))
                    {
                        IISHelper.CreateIISWebSite(sm, siteName, appPoolName, hostHeaders, Path.Combine(IISRoot, siteName));
                    }

                    sm.CommitChanges();

                    // modify web.config
                    if (sm.Sites.Any(s => s.Name == siteName))
                    {
                        log.Info("site exists");
                        ConfigurationHelper.AddCustomErrors(sm, siteName, siteSpecificName);
                        log.Info("wrote custom errors");

                        ConfigurationHelper.AddConnectionString(sm, siteName, "MsSqlConnectionString", DataBaseConnectionHelper.BuildConnectionString(DbServer, DbName, DbUserName, DbPassword));
                        ConfigurationHelper.AddConnectionString(sm, siteName, "MsSqlConnectionStringCustom", ";pooling=true");
                        ConfigurationHelper.AddConnectionString(sm, siteName, "Bamboo", ";pooling=true");

                        ConfigurationHelper.AddAppSetting(sm, siteName, "aspxTemplatesFolder", "site_specific/aspx_templates");
                        ConfigurationHelper.AddAppSetting(sm, siteName, "WebSiteId", website.Id.ToString());
                        ConfigurationHelper.AddAppSetting(sm, siteName, "customModulesFolder", "site_specific/custom_modules");
                        ConfigurationHelper.AddAppSetting(sm, siteName, "emailTemplatesFolder", "site_specific/email_templates");

                        sm.CommitChanges();

                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "CustomRedir", "destinationUrl", "~/site_specific/$1", "^/(?!_files)(_.*)+");
                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "RobotsRedir", "destinationUrl", "~/site_specific/robots.txt", "^/robots.txt");
                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "FavicoRedir", "destinationUrl", "~/site_specific/favicon.ico", "^/favicon.ico");
                    }

                    GridViewWebsitesLoad(true);

                    notifier.Visible = true;
                    notifier.Message = ResourceManager.GetString("$website_successfully_created");

                }
            }
        }

        

        protected void Wizard1_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            if (Page.IsValid)
            {
                if (Wizard1.ActiveStepIndex == 0)
                {
                    MasterDbUserName = InputMasterDbUsername.Value;
                    MasterDbPassword = InputMasterDbPassword.Value;
                    DbServer = InputDbServer.Value;

                    if (DataBaseConnectionHelper.CheckDbCredentials(DbServer, "master", MasterDbUserName, MasterDbPassword))
                    {
                        if (DropDownListDatabaseMode.SelectedValue == "new")
                        {
                            UsingExistingDb = false;
                            Wizard1.ActiveStepIndex = 1;

                        }
                        else
                        {
                            UsingExistingDb = true;
                            Wizard1.ActiveStepIndex = 2;
                        }
                    }
                    else
                    {
                        notifier.Warning = ResourceManager.GetString("$database_connection_failed_wrong_credentials");
                        notifier.Visible = true;
                        e.Cancel = true;
                    }
                }
                else if (Wizard1.ActiveStepIndex == 1)
                {
                    // database doesn't exist, we need to create it.
                    // first create database, than can create website and do everything else.

                    DbName = InputNewDbName.Value;
                    DbUserName = InputNewDbName.Value;
                    DbPassword = InputNewDbName.Value;

                    var databaseSql = GetFileContentFromResource("OneMainWeb.Res.Sql.1_database.sql.template").Replace("@INITIAL_CATALOG@", DbName).Replace("@DBPATH@", InputSqlPhysicalPath.Value);
                    var userSql = GetFileContentFromResource("OneMainWeb.Res.Sql.1_1_user.sql.template").Replace("@INITIAL_CATALOG@", DbName).Replace("@PASS@", DbPassword);
                    var tablesSql = GetFileContentFromResource("OneMainWeb.Res.Sql.2_tables.sql.template").Replace("@INITIAL_CATALOG@", DbName);
                    var insertsSql = GetFileContentFromResource("OneMainWeb.Res.Sql.3_standard_inserts.sql.template").Replace("@INITIAL_CATALOG@", DbName);
                    var spsSql = GetFileContentFromResource("OneMainWeb.Res.Sql.4_stored_procedures.sql.template").Replace("@INITIAL_CATALOG@", DbName);
                    var securitySql = GetFileContentFromResource("OneMainWeb.Res.Sql.5_security.sql.template").Replace("@INITIAL_CATALOG@", DbName);


                    if (!DataBaseConnectionHelper.RunSqlScript(databaseSql, DbServer, "master", MasterDbUserName, MasterDbPassword)) {
                        notifier.Warning = ResourceManager.GetString("$failed_to_create_database");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }

                    if (!DataBaseConnectionHelper.RunSqlScript(userSql, DbServer, "master", MasterDbUserName, MasterDbPassword))
                    {
                        notifier.Warning = ResourceManager.GetString("$failed_to_create_database_user");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }

                    if (!DataBaseConnectionHelper.RunSqlScript(tablesSql, DbServer, DbName, DbUserName, DbPassword))
                    {
                        notifier.Warning = ResourceManager.GetString("$failed_to_create_database_tables");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }

                    if (!DataBaseConnectionHelper.RunSqlScript(insertsSql, DbServer, DbName, DbUserName, DbPassword))
                    {
                        notifier.Warning = ResourceManager.GetString("$failed_to_insert_standard_inserts");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }


                    if (!DataBaseConnectionHelper.RunSqlScript(spsSql, DbServer, DbName, DbUserName, DbPassword))
                    {
                        notifier.Warning = ResourceManager.GetString("$failed_to_create_database_sps");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }

                    if (!DataBaseConnectionHelper.RunSqlScript(securitySql, DbServer, DbName, DbUserName, DbPassword))
                    {
                        notifier.Warning = ResourceManager.GetString("$failed_to_run_security_template");
                        notifier.Visible = true;
                        e.Cancel = true;
                        return;
                    }

                    Wizard1.ActiveStepIndex = 3;
                }
                else if (Wizard1.ActiveStepIndex == 2)
                {
                    DbUserName = InputExistingDbUsername.Value;
                    DbPassword = InputExistingDbPassword.Value;
                    DbName = DropDownListExistingDatabaseName.SelectedValue;

                    try
                    {
                        if (DataBaseConnectionHelper.WebSiteTableExists(DbServer, DbName, DbUserName, DbPassword))
                        {
                            // we can connect and web_site table exists
                            // proceed to website creation step
                        }
                        else
                        {
                            notifier.Warning = ResourceManager.GetString("$website_table_missing_in_db");
                            notifier.Visible = true;
                            e.Cancel = true;
                        }
                    }
                    catch
                    {
                        notifier.Warning = ResourceManager.GetString("$login_failed_for_existing_database");
                        notifier.Visible = true;
                        e.Cancel = true;
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private Button GetNextButton()
        {
            WebControl stepNavTemplate = this.Wizard1.FindControl("StepNavigationTemplateContainerID") as WebControl;
            if (stepNavTemplate != null)
                stepNavTemplate = this.Wizard1.FindControl("StartNavigationTemplateContainerID") as WebControl;
            if (stepNavTemplate != null)
                return stepNavTemplate.FindControl("StepNextButton") as Button;            
            return null;
        }

        private void AssignValidationGroup(string group)
        {
            var button = GetNextButton();

            if (button != null)
            {
                button.ValidationGroup = group;
                button.CausesValidation = true;
            }
        }
    }

    

    internal class DirectoryHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(DirectoryHelper));

        
    }

    internal class ConfigurationHelper
    {
       

        

        

        

        
    }

    [Serializable]
    public class BOIISWebSite : BOWebSite
    {
        public string AppPoolName { get; set; }
        public string HostHeader { get; set; }
    }
}