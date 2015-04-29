using System;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using NLog;
using System.Web.Security;
using System.Web.SessionState;
using System.Configuration;

namespace One.Net.BLL
{
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.High)]
    public class TByNumberPathProvider : VirtualPathProvider
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private const string prefix = "~/_files";
        // string below is also used (copied) to BOInternalContent. If you change it here, make sure, you change it there, too.
        public const string fileMatcher = prefix + @"/([0-9]{1,6})/([a-zA-Z0-9čćðšž\,_\-\s\!]{1,255}\.[a-zA-Z0-9_]{2,5})";
        private const string pathMatcher = prefix + @"/([0-9]{1,6})/";

        public TByNumberPathProvider()
        {
            log.Info("TByNumberPathProvider constructor: " + prefix);
        }

        protected override void Initialize()
        {
            base.Initialize();
            log.Info("TByNumberPathProvider subsystem Initialized.. prefix: " + prefix);
        }

        private bool IsPathVirtual(string virtualPath)
        {
            String checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            Regex reg = new Regex(pathMatcher);
            bool exists = reg.IsMatch(checkPath);
            return exists;
        }

        public override bool FileExists(string virtualPath)
        {
            return this.IsPathVirtual(virtualPath) ? ((TVirtualFile) this.GetFile(virtualPath)).Exists : Previous.FileExists(virtualPath);
        }

        public override bool DirectoryExists(string virtualPath)
        {
            return this.IsPathVirtual(virtualPath) ? true : Previous.DirectoryExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return this.IsPathVirtual(virtualPath) ? new TVirtualFile(virtualPath) : Previous.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualPath)
        {
            return Previous.GetDirectory(virtualPath);
        }
    }

    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class TVirtualFile : VirtualFile
    {
        private static BFileSystem fileB =  new BFileSystem();
        private BOFile file;

        private FileInfo CachedFileInfo { get; set; }

        private static readonly object cacheLocking = new Object();

        public bool Exists
        {
            get {
                //bool hasPrivateAccess = (bool)HttpContext.Current.Items["has_private_access"];
                bool fileExistsOnDisk = false;

                fileExistsOnDisk = EnableDiskCache && CachedFileInfo.Exists;
                //return fileExistsOnDisk || (file != null && (!file.Folder.IsPrivate || hasPrivateAccess)); 
                return fileExistsOnDisk || (file != null); 
            }
        }

        public TVirtualFile(string virtualPath)
            : base(virtualPath)
        {   
            var checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            CachedFileInfo = new FileInfo(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, virtualPath.TrimStart('/')));
            Regex reg = new Regex(TByNumberPathProvider.fileMatcher, RegexOptions.IgnoreCase);
            Match matched = reg.Match(checkPath);
            int fileId = 0;
            int.TryParse(matched.Groups[1].Value, out fileId );
            if (fileId > 0)
            {
                file = fileB.Get(fileId);
            }
        }


        protected bool EnableDiskCache
        {
            get
            {
                if (IsImage)
                    return false;
                bool temp = false;
                bool.TryParse(ConfigurationManager.AppSettings["EnableDiskCache"], out temp);
                return temp;
            }
        }

        protected bool IsImage
        {
            get 
            {
                var extensionToCheck = CachedFileInfo.Extension.ToLower();
                return extensionToCheck.Contains(".png") || extensionToCheck.Contains(".jpg") || extensionToCheck.Contains(".jpeg") || extensionToCheck.Contains(".gif");
            }
        }

        public override Stream Open()
        {
            if (file != null)
            {
                if (EnableDiskCache && CachedFileInfo.Exists)
                {
                    return CachedFileInfo.OpenRead();
                }

                Stream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);
                byte[] fileData = null;
                fileData = BFileSystem.GetCachedFileBytes(file.Id.Value);
                bw.Write(fileData);
                bw.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                if (EnableDiskCache && CachedFileInfo != null && !file.Folder.IsPrivate)
                {
                    lock (cacheLocking)
                    {
                        if (!CachedFileInfo.Directory.Exists)
                            CachedFileInfo.Directory.Create();

                        using (var memoryStream = new MemoryStream(fileData))
                        {
                            var fileLength = TImageHandler.WriteFileStream(memoryStream, CachedFileInfo.FullName);
                        }
                    }
                }
                return stream;
            }
            else
                return null;
        }
    }
}
