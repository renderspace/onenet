using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using log4net;
using Microsoft.Web.Administration;

namespace One.Net.BLL.Utility
{
    public class IISHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(IISHelper));

        public static void CreateWebSite(ServerManager sm, string siteName, string poolName, string hostHeaders, string physicalPath)
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

        private static long GenerateNewSiteId(ServerManager sm)
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

        public static bool AppPoolExists(ServerManager sm, string appPool)
        {
            return (sm.ApplicationPools.Any(t => t.Name == appPool));
        }

        public static void CreateNewAppPool(ServerManager sm, string poolName, string aspNetRunTimeVersion)
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

        private static System.Configuration.Configuration OpenConfigFile(string configPath, string siteName)
        {
            var configFile = new FileInfo(configPath);
            var vdm = new System.Web.Configuration.VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
            var wcfm = new System.Web.Configuration.WebConfigurationFileMap();
            wcfm.VirtualDirectories.Add("/", vdm);
            return System.Web.Configuration.WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/", siteName);
        }

        public static void AddUrlRewriting(ServerManager sm, string siteName, string elementName, string attributeName, string destinationUrl, string virtualUrl)
        {
            string configFilePath = sm.Sites[siteName].Applications[0].VirtualDirectories[0].PhysicalPath + "/web.config";
            var siteConfig = OpenConfigFile(configFilePath, siteName);

            var urlrewritingnet = XElement.Parse(siteConfig.Sections["urlrewritingnet"].SectionInformation.GetRawXml());

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

        public static void AddCustomErrors(ServerManager sm, string siteName)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var customErrors = siteConfig.GetSection("system.web/customErrors");
            customErrors.SetAttributeValue("defaultRedirect", "~/site_specific/error.htm");

            var errors = customErrors.GetCollection();

            if (errors.Any(t => t.GetAttributeValue("statusCode").ToString() == "403"))
                errors.Remove(errors.First(t => t.GetAttributeValue("statusCode").ToString() == "403"));
            ConfigurationElement element403 = errors.CreateElement("error");
            element403["statusCode"] = "403";
            element403["redirect"] = "~/site_specific/authorizationfailed.htm";
            errors.Add(element403);

            if (errors.Any(t => t.GetAttributeValue("statusCode").ToString() == "404"))
                errors.Remove(errors.First(t => t.GetAttributeValue("statusCode").ToString() == "404"));
            ConfigurationElement element404 = errors.CreateElement("error");
            element404["statusCode"] = "404";
            element404["redirect"] = "~/site_specific/filenotfound.htm";
            errors.Add(element404);
        }

        public static void AddConnectionString(ServerManager sm, string siteName, string attributeName, string attributeValue)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var connectionStrings = siteConfig.GetSection("connectionStrings").GetCollection();

            if (connectionStrings.Any(t => t.GetAttributeValue("name").ToString() == attributeName))
                connectionStrings.Remove(connectionStrings.First(t => t.GetAttributeValue("name").ToString() == attributeName));

            ConfigurationElement addElement = connectionStrings.CreateElement("add");
            addElement["name"] = attributeName;
            addElement["connectionString"] = attributeValue;
            connectionStrings.Add(addElement);
        }

        public static void AddAppSetting(ServerManager sm, string siteName, string key, string value)
        {
            var siteConfig = sm.Sites[siteName].GetWebConfiguration();
            var appSettings = siteConfig.GetSection("appSettings").GetCollection();

            if (appSettings.Any(t => t.GetAttributeValue("key").ToString() == key))
                appSettings.Remove(appSettings.First(t => t.GetAttributeValue("key").ToString() == key));

            ConfigurationElement addElement = appSettings.CreateElement("add");
            addElement["key"] = key;
            addElement["value"] = value;
            appSettings.Add(addElement);
        }

        public static void DirectoryCopy(string sourceDirName, DirectoryInfo destDir, bool copyAdminFolders)
        {
            log.Info("About to start directory copy: sourceDirName " + sourceDirName + " - destDirName " + destDir.FullName);

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
            if (!destDir.Exists)
            {
                destDir.Create();
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                if (copyAdminFolders || (!copyAdminFolders && subdir.Name != "adm" &&
                    subdir.Name != "Languages" &&
                    subdir.Name != "AdminControls" &&
                    subdir.Name != "AdminExtensions"))
                {
                    string temppath = Path.Combine(destDir.FullName, subdir.Name);
                    DirectoryCopy(subdir.FullName, new DirectoryInfo(temppath), copyAdminFolders);
                }
            }
        }
    }
}
