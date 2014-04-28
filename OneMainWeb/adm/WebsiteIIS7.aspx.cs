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
            var names = DataBaseConnectionHelper.ListDatabases(InputDbServer.Value, MasterDbUserName, MasterDbPassword);
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
                    InputAppPoolName.Value = Websites[0].AppPoolName;
            }
            else
            {
                // new db has been created
                InputAppPoolName.Value = "";
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
            var siteSpecificName = InputSiteSpecificName.Value;
            var siteName = InputWebSiteName.Value + ".w.renderspace.net";
            var lcid = Int32.Parse(DropDownListLcid.SelectedValue);
            var hostHeaders = InputHostHeader.Value;
            var appPoolName = InputAppPoolName.Value;

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
                        IISHelper.CreateNewAppPool(sm, appPoolName, "v4.0");
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

                        ConfigurationHelper.AddAppSetting(sm, siteName, "aspxTemplatesFolder", "site_specific/" + siteSpecificName + "/aspx_templates");
                        ConfigurationHelper.AddAppSetting(sm, siteName, "WebSiteId", website.Id.ToString());
                        ConfigurationHelper.AddAppSetting(sm, siteName, "customModulesFolder", "site_specific/" + siteSpecificName + "/custom_modules");
                        ConfigurationHelper.AddAppSetting(sm, siteName, "emailTemplatesFolder", "site_specific/" + siteSpecificName + "/email_templates");

                        sm.CommitChanges();

                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "CustomRedir", "destinationUrl", "~/site_specific/" + siteSpecificName + "/$1", "^/(?!_files)(_.*)+");
                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "RobotsRedir", "destinationUrl", "~/site_specific/" + siteSpecificName + "/robots.txt", "^/robots.txt");
                        ConfigurationHelper.AddUrlRewriting(sm, siteName, "FavicoRedir", "destinationUrl", "~/site_specific/" + siteSpecificName + "/favicon.ico", "^/favicon.ico");
                    }

                    GridViewWebsitesLoad(true);

                    notifier.Visible = true;
                    notifier.Message = ResourceManager.GetString("$website_successfully_created");

                }
            }
        }

        private string GetFileContentFromResource(string fileName)
        {
            string result = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
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

    internal class IISHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(IISHelper));

        public static bool AppPoolExists(Microsoft.Web.Administration.ServerManager sm, string appPool)
        {
            return (sm.ApplicationPools.Any(t => t.Name == appPool));
        }

        public static void CreateIISWebSite(Microsoft.Web.Administration.ServerManager sm, string siteName, string poolName, string hostHeaders, string physicalPath)
        {
            var site = sm.Sites.CreateElement("site");
            site.SetAttributeValue("name", siteName);
            site.Name = siteName;
            site.Id = GenerateNewSiteId(sm);

            site.Applications.Add("/", physicalPath);
            site.Applications[0].ApplicationPoolName = poolName;

            site.Bindings.Clear();

            var headers = StringTool.SplitString(hostHeaders, ';');
            if (!headers.Contains(siteName))
                headers.Add(siteName); // add siteName.w.renderspace.net to make things work.

            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    var binding = site.Bindings.CreateElement();
                    binding.BindingInformation = "*" + ":80:" + header;
                    binding.Protocol = "http";
                    site.Bindings.Add(binding);
                }
            }            

            sm.Sites.Add(site);
        }

        public static void CreateNewAppPool(Microsoft.Web.Administration.ServerManager sm, string poolName, string aspNetRunTimeVersion)
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
            appPool.ManagedRuntimeVersion = aspNetRunTimeVersion;

            // Use the Network Service account
            appPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;

            // Shut down after being idle for 10 minutes.
            appPool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(10);

            // Max. number of IIS worker processes (W3WP.EXE)
            appPool.ProcessModel.MaxProcesses = 1;
        }

        private static long GenerateNewSiteId(Microsoft.Web.Administration.ServerManager sm)
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
    }

    internal class DirectoryHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(DirectoryHelper));

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool copyAdminFolders)
        {
            log.Info("About to start directory copy: sourceDirName " + sourceDirName + " - destDirName " + destDirName);

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new Exception(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if ((copyAdminFolders || (!copyAdminFolders && subdir.Name != "adm" &&
                        subdir.Name != "Languages" &&
                        subdir.Name != "AdminControls" &&
                        subdir.Name != "AdminExtensions")) && subdir.Name != "site_specific")
                    {
                        // Create the subdirectory.
                        string temppath = Path.Combine(destDirName, subdir.Name);

                        // Copy the subdirectories.
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs, copyAdminFolders);
                    }
                }
            }
        }
    }

    internal class ConfigurationHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(ConfigurationHelper));

        private static System.Configuration.Configuration OpenConfigFile(string configPath, string siteName)
        {
            var configFile = new FileInfo(configPath);
            var vdm = new System.Web.Configuration.VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
            var wcfm = new System.Web.Configuration.WebConfigurationFileMap();
            wcfm.VirtualDirectories.Add("/", vdm);
            return System.Web.Configuration.WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/", siteName);
        }

        public static void AddUrlRewriting(Microsoft.Web.Administration.ServerManager sm, string siteName, string elementName, string attributeName, string destinationUrl, string virtualUrl)
        {
            string configFilePath = sm.Sites[siteName].Applications[0].VirtualDirectories[0].PhysicalPath + "/web.config";
            var siteConfig = OpenConfigFile(configFilePath, siteName);

            XElement urlrewritingnet = XElement.Parse(siteConfig.Sections["urlrewritingnet"].SectionInformation.GetRawXml());

            if (urlrewritingnet != null && urlrewritingnet.HasElements)
            {
                var rewrites = urlrewritingnet.Elements().First(e => e.Name.LocalName == "rewrites");

                if (rewrites != null && rewrites.HasElements)
                {
                    foreach (var el in rewrites.Elements())
                    {
                        if (el.Attribute("name").Value == elementName)
                        {
                            el.SetAttributeValue("destinationUrl", destinationUrl);
                            el.SetAttributeValue("virtualUrl", virtualUrl);
                        }
                    }
                }
            }

            siteConfig.Sections["urlrewritingnet"].SectionInformation.SetRawXml(urlrewritingnet.ToString());
            siteConfig.Save();            
        }

        public static void AddCustomErrors(Microsoft.Web.Administration.ServerManager sm, string siteName, string siteSpecificName)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var customErrors = siteConfig.GetSection("system.web/customErrors");
            customErrors.SetAttributeValue("defaultRedirect", "~/site_specific/" + siteSpecificName + "/error.htm");

            var errors = customErrors.GetCollection();

            if (errors.Any(t => t.GetAttributeValue("statusCode").ToString() == "403"))
                errors.Remove(errors.First(t => t.GetAttributeValue("statusCode").ToString() == "403"));
            Microsoft.Web.Administration.ConfigurationElement element403 = errors.CreateElement("error");
            element403["statusCode"] = "403";
            element403["redirect"] = "~/site_specific/" + siteSpecificName + "/authorizationfailed.htm";
            errors.Add(element403);

            if (errors.Any(t => t.GetAttributeValue("statusCode").ToString() == "404"))
                errors.Remove(errors.First(t => t.GetAttributeValue("statusCode").ToString() == "404"));
            Microsoft.Web.Administration.ConfigurationElement element404 = errors.CreateElement("error");
            element404["statusCode"] = "404";
            element404["redirect"] = "~/site_specific/" + siteSpecificName + "/filenotfound.htm";
            errors.Add(element404);
        }

        public static void AddConnectionString(Microsoft.Web.Administration.ServerManager sm, string siteName, string attributeName, string attributeValue)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var connectionStrings = siteConfig.GetSection("connectionStrings").GetCollection();

            if (connectionStrings.Any(t => t.GetAttributeValue("name").ToString() == attributeName))
                connectionStrings.Remove(connectionStrings.First(t => t.GetAttributeValue("name").ToString() == attributeName));

            Microsoft.Web.Administration.ConfigurationElement addElement = connectionStrings.CreateElement("add");
            addElement["name"] = attributeName;
            addElement["connectionString"] = attributeValue;
            connectionStrings.Add(addElement);
        }

        public static void AddAppSetting(Microsoft.Web.Administration.ServerManager sm, string siteName, string key, string value)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var appSettings = siteConfig.GetSection("appSettings").GetCollection();

            if (appSettings.Any(t => t.GetAttributeValue("key").ToString() == key))
                appSettings.Remove(appSettings.First(t => t.GetAttributeValue("key").ToString() == key));

            Microsoft.Web.Administration.ConfigurationElement addElement = appSettings.CreateElement("add");
            addElement["key"] = key;
            addElement["value"] = value;
            appSettings.Add(addElement);
        }
    }

    internal class DataBaseConnectionHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(DataBaseConnectionHelper));

        public static List<string> ListDatabases(string serverName, string userName, string passWord)
        {
            List<string> list = new List<string>();
            var sql = @"SELECT name
                        FROM [sys].[databases]
                        WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";

            using (var reader = SqlHelper.ExecuteReader(BuildConnectionString(serverName, "master", userName, passWord) , CommandType.Text, sql))
            {
                while (reader.Read())
                {
                    list.Add((string)reader["name"]);
                }
            }

            return list;
        }

        public static bool WebSiteTableExists(string serverName, string dbName, string dbUsername, string dbPassword)
        {
            var sql = "SELECT count(*) ct FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[web_site]') AND type in (N'U')";

            var sb = new SqlConnectionStringBuilder();
            sb.DataSource = serverName;
            sb.InitialCatalog = dbName;
            sb.UserID = dbUsername;
            sb.Password = dbPassword;
            sb.Pooling = true;

            var ct = 0;

            using (var reader = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text, sql))
            {
                if (reader.Read())
                {
                    ct = (int)reader["ct"];
                }
            }

            return ct > 0;
        }

        public static bool CheckDbCredentials(string serverName, string dbName, string userName, string password)
        {
            var success = false;
            var sb = new SqlConnectionStringBuilder();
            sb.DataSource = serverName;
            sb.InitialCatalog = dbName;
            sb.UserID = userName;
            sb.Password = password;
            sb.Pooling = true;

            var connection = new SqlConnection(sb.ConnectionString);

            try
            {
                connection.Open();
                success = true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                connection.Close();
            }

            return success;
        }

        public static List<BOIISWebSite> ListWebsites(string serverName, string dbName, string dbUsername, string dbPassword)
        {
            List<BOIISWebSite> websiteList = new List<BOIISWebSite>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                @"SELECT w.id, w.content_fk_id, w.host_header, w.app_pool_name, 
	                    (SELECT p.id FROM [dbo].[pages] p 
	                     WHERE publish = 0 AND web_site_fk_id = w.id AND pages_fk_id IS NULL) AS rootPageId
                    FROM [dbo].[web_site] w"))
            {
                while (rdr.Read())
                {
                    BOIISWebSite website = new BOIISWebSite();
                    website.Id = rdr.GetInt32(0);
                    website.ContentId = rdr.GetInt32(1);
                    website.HostHeader = (string)rdr["host_header"];
                    website.AppPoolName = (string)rdr["app_pool_name"];
                    if (rdr[4] != DBNull.Value)
                        website.RootPageId = rdr.GetInt32(4);
                    websiteList.Add(website);
                }
            }

            foreach (BOWebSite site in websiteList)
            {
                SqlParameter paramsToPass = new SqlParameter("@webSiteID", site.Id);
                using (SqlDataReader rdr = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                @"SELECT sl.name, ISNULL( ws.value, sl.default_value), sl.type, sl.user_visibility
                    FROM [dbo].[settings_list] sl 
                    LEFT JOIN [dbo].[web_site_settings] ws ON  sl.id = ws.settings_list_fk_id AND web_site_fk_id = @webSiteID
                    WHERE sl.[subsystem] = 'WebSite'", paramsToPass))
                {
                    while (rdr.Read())
                    {
                        BOSetting setting = new BOSetting(rdr.GetString(0), rdr.GetString(2), rdr.GetString(1), rdr.GetString(3));
                        site.Settings.Add(rdr.GetString(0), setting);
                    }
                }

                if (site.RootPageId.HasValue)
                {
                    paramsToPass = new SqlParameter("@rootPageId", site.RootPageId);
                    using (SqlDataReader rdr = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                    @"SELECT language_fk_id, par_link 
                        FROM int_link 
                        WHERE pages_fk_id= @rootPageId AND pages_fk_publish = 0", paramsToPass))
                    {
                        while (rdr.Read())
                            site.Languages.Add(rdr.GetInt32(0));
                    }
                }
                else
                    site.Languages.Add(site.PrimaryLanguageId);
            }

            foreach (BOWebSite site in websiteList)
            {
                BOInternalContent content = null;

                var paramsToPass = new SqlParameter[2];
                paramsToPass[0] = new SqlParameter("@contentID", site.ContentId.Value);
                paramsToPass[1] = new SqlParameter("@languageID", site.PrimaryLanguageId);

                using (SqlDataReader reader = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                    @"SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, c.principal_modified_by, c.date_modified, c.votes, c.score
                    FROM [dbo].[content] c
                    INNER JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageID
                    WHERE c.id = @contentID", paramsToPass))
                {
                    if (reader.Read())
                    {
                        content = new BOInternalContent();
                        content.ContentId = site.ContentId.Value;
                        PopulateContent(reader, content, site.PrimaryLanguageId);
                    }
                }

                if (content != null)
                    site.LoadContent(content);
            }

            return websiteList;
        }

        internal static void PopulateContent(IDataReader reader, BOInternalContent content, int languageId)
        {
            if (reader[3] != DBNull.Value)
            {
                content.LanguageId = languageId;
                content.Title = reader.GetString(0);
                content.SubTitle = reader.GetString(1);
                content.Teaser = reader.GetString(2);
                content.Html = reader.GetString(3);
                content.PrincipalCreated = reader.GetString(4);
                content.DateCreated = reader.GetDateTime(5);

                if (reader[6] != DBNull.Value && reader[7] != DBNull.Value)
                {
                    content.PrincipalModified = reader.GetString(6);
                    content.DateModified = reader.GetDateTime(7);
                }

                content.Votes = reader.GetInt32(8);
                if (!reader.IsDBNull(9))
                    content.Score = Double.Parse(reader.GetValue(9).ToString());
            }
            else
            {
                content.MissingTranslation = true;
            }
        }

        public static BOIISWebSite GetWebSite(int id, string serverName, string dbName, string dbUsername, string dbPassword)
        {
            List<BOIISWebSite> webSiteList = ListWebsites(serverName, dbName, dbUsername, dbPassword);

            foreach (BOIISWebSite webSite in webSiteList)
                if (webSite.Id == id)
                    return webSite;
            return null;
        }

        public static BOIISWebSite FindWebSite(string siteName, string serverName, string dbName, string dbUsername, string dbPassword)
        {
            List<BOIISWebSite> webSiteList = ListWebsites(serverName, dbName, dbUsername, dbPassword);

            foreach (BOIISWebSite webSite in webSiteList)
                if (webSite.Title == siteName)
                    return webSite;
            return null;
        }

        public static List<int> ListLanguages(string serverName, string dbName, string dbUsername, string dbPassword)
        {
            List<int> langList = new List<int>();

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
            "SELECT DISTINCT id FROM [dbo].[language] WHERE id != 1279"))
            {
                while (rdr.Read())
                    langList.Add((int)rdr["id"]);
            }
            return langList;
        }

        public static void ChangeWebsite(BOIISWebSite website, string serverName, string dbName, string dbUsername, string dbPassword)
        {
            var websiteB = new BWebsite();

            // change content
            SqlParameter[] paramsToPass = new SqlParameter[9];
            paramsToPass[0] = SqlHelper.GetNullable("contentID", website.ContentId);
            paramsToPass[1] = new SqlParameter("@languageID", website.LanguageId);
            paramsToPass[2] = new SqlParameter("@title", website.Title);
            paramsToPass[3] = new SqlParameter("@subtitle", website.SubTitle);
            paramsToPass[4] = new SqlParameter("@teaser", website.Teaser);
            paramsToPass[5] = new SqlParameter("@html", website.Html);
            paramsToPass[6] = new SqlParameter("@principal", Thread.CurrentPrincipal.Identity.Name);
            paramsToPass[7] = SqlHelper.GetNullable("score", website.Score);
            paramsToPass[8] = new SqlParameter("@votes", website.Votes);

            object result = SqlHelper.ExecuteScalar(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.StoredProcedure, "[ChangeContent]", paramsToPass);

            if (website.ContentId == null) // Inserted
            {
                website.ContentId = (int)result;
                website.PrincipalCreated = Thread.CurrentPrincipal.Identity.Name;
                website.DateCreated = DateTime.Now;
            }
            else // Updated
            {
                website.PrincipalModified = Thread.CurrentPrincipal.Identity.Name;
                website.DateModified = DateTime.Now;
            }

            // audit content
            paramsToPass = new SqlParameter[8];
            paramsToPass[0] = new SqlParameter("@contentId", website.ContentId);
            paramsToPass[1] = new SqlParameter("@languageId", website.LanguageId);
            paramsToPass[2] = new SqlParameter("@title", website.Title);
            paramsToPass[3] = new SqlParameter("@subtitle", website.SubTitle);
            paramsToPass[4] = new SqlParameter("@teaser", website.Teaser);
            paramsToPass[5] = new SqlParameter("@html", website.Html);
            paramsToPass[6] = new SqlParameter("@principal", Thread.CurrentPrincipal.Identity.Name);
            paramsToPass[7] = new SqlParameter("@guid", System.Guid.NewGuid());

            string sql = @"INSERT INTO [dbo].[content_data_store_audit]
                           (guid, content_fk_id, language_fk_id, title, subtitle, teaser, html, principal_saved_by)
                           VALUES
                           (@guid, @contentId, @languageId, @title, @subtitle, @teaser, @html, @principal)";

            SqlHelper.ExecuteScalar(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text, sql, paramsToPass);

            paramsToPass = new SqlParameter[] { 
                    new SqlParameter("@ContentId", website.ContentId),
                    new SqlParameter("@id", DbType.Int32),
                    new SqlParameter("@hostHeader", website.HostHeader),
                    new SqlParameter("@appPoolName", website.AppPoolName),
                };
            paramsToPass[1].Direction = ParameterDirection.Output;
            paramsToPass[1].DbType = DbType.Int32;
            
            if (website.IsNew)
            {
                SqlHelper.ExecuteNonQuery(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                @"INSERT INTO [dbo].[web_site] (content_fk_id, host_header, app_pool_name) VALUES (@ContentId, @hostHeader, @appPoolName); SET @id=SCOPE_IDENTITY()", paramsToPass);
                website.Id = (int)paramsToPass[1].Value;
            }
            else
            {
                log.Info("Will update website id " + website.Id + " with host_header " + website.HostHeader);
                paramsToPass[1].Value = website.Id;
                paramsToPass[1].Direction = ParameterDirection.InputOutput;
                log.Info(SqlHelper.ExecuteNonQuery(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.Text,
                @"UPDATE [dbo].[web_site] SET host_header=@hostHeader WHERE id=@id", paramsToPass));
            }

            paramsToPass = new SqlParameter[] {
                    new SqlParameter("@Id", website.Id),
                    new SqlParameter("@Name", "PrimaryLanguageId"),
                    new SqlParameter("@Value", website.LanguageId)
                };

            SqlHelper.ExecuteNonQuery(BuildConnectionString(serverName, dbName, dbUsername, dbPassword), CommandType.StoredProcedure, "[dbo].[ChangeWebSiteSetting]", paramsToPass);
        }

        public static string BuildConnectionString(string serverName, string dbName, string dbUsername, string dbPassword)
        {
            var connBuilder = new SqlConnectionStringBuilder();

            connBuilder.UserID = dbUsername;
            connBuilder.Password = dbPassword;
            connBuilder.DataSource = serverName;
            connBuilder.InitialCatalog = dbName;
            connBuilder.Pooling = true;

            return connBuilder.ConnectionString;
        }

        public static bool RunSqlScript(string sql, string serverName, string dbName, string dbUsername, string dbPassword)
        {
            var success = false;

            try
            {
                SqlConnection conn = new SqlConnection(BuildConnectionString(serverName, dbName, dbUsername, dbPassword));
                Server server = new Server(new ServerConnection(conn));
                server.ConnectionContext.ExecuteNonQuery(sql);

                success = true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return success;
        }
    }

    [Serializable]
    public class BOIISWebSite : BOWebSite
    {
        public string AppPoolName { get; set; }
        public string HostHeader { get; set; }
    }
}