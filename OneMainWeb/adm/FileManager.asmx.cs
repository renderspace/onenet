using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using One.Net.BLL;

namespace OneMainWeb.adm
{
    /// <summary>
    /// Summary description for FileManager
    /// </summary>
    [WebService(Namespace = "http://ws.renderspace.si/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class FileManager : WebService
    {
        private readonly string UploadPath;
        protected static BFileSystem fileB = new BFileSystem();

        public FileManager()
        {
            UploadPath = Server.MapPath(ConfigurationManager.AppSettings["UploadPath"].ToString());
            if (!Directory.Exists(UploadPath))
                CustomSoapException("Upload Folder not found", "The folder " + UploadPath + " does not exist");
        }

        [WebMethod]
        public string Ping()
        {
            string res = "not authenticated + exception";
            try
            {
                if (User.Identity.IsAuthenticated)
                    res = User.Identity.Name.ToString();
                else
                    res = "not authenticated";
            }
            catch { }

            return res;
        }

        [WebMethod]
        public string ValidateUser(string userName, string password)
        {
            var encTicket = "";
            if (Membership.Provider.ValidateUser(userName, password))
            {
                /*
                var x = new System.Xml.XmlDocument();
                x.Load(UrlPath.GetBasePhysicalDirectory() + "web.config");
                System.Xml.XmlNode node = x.SelectSingleNode("/configuration/system.web/authentication/forms");
                int Timeout = int.Parse(node.Attributes["timeout"].Value,
                                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                */
                var ticket = new FormsAuthenticationTicket(1,
                         userName,
                         DateTime.Now,
                         DateTime.Now.AddMinutes(30),
                         false,
                         "",
                         FormsAuthentication.FormsCookiePath);
                encTicket = FormsAuthentication.Encrypt(ticket);
            }
            return encTicket;
        }

        public static void CustomSoapException(string exceptionName, string message)
        {
            throw new SoapException(exceptionName + ": " + message, new System.Xml.XmlQualifiedName("FileManager"));
        }

        [WebMethod]
        public void AppendChunk(string fileName, byte[] buffer, long Offset, int BytesRead)
        {
            string FilePath = Path.Combine(UploadPath, fileName);

            bool FileExists = File.Exists(FilePath);
            if (!FileExists || (File.Exists(FilePath) && Offset == 0))
                File.Create(FilePath).Close();
            long FileSize = new FileInfo(FilePath).Length;

            if (FileSize != Offset)
                CustomSoapException("Transfer Corrupted", String.Format("The file size is {0}, expected {1} bytes", FileSize, Offset));
            else
            {
                using (var fs = new FileStream(FilePath, FileMode.Append))
                    fs.Write(buffer, 0, BytesRead);
            }
        }

        /// <summary>
        /// Get the number of bytes in a file in the Upload folder on the server.
        /// The client needs to know this to know when to stop downloading
        /// </summary>
        [WebMethod]
        public long GetFileSize(string fileName)
        {
            string FilePath = Path.Combine(UploadPath, fileName);

            if (!File.Exists(FilePath))
                CustomSoapException("File not found", "The file " + FilePath + " does not exist");

            return new FileInfo(FilePath).Length;
        }

        [WebMethod]
        public string CheckFileHash(string fileName)
        {
            return CalcFileHash(fileName);
        }

        private string CalcFileHash(string fileName)
        {
            var FilePath = Path.Combine(UploadPath, fileName);
            var fi = new FileInfo(FilePath);

            if (!fi.Exists)
                CustomSoapException("File", "File doesn't exist!");

            var sha1 = new SHA1CryptoServiceProvider();
            byte[] hash;
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                hash = sha1.ComputeHash(fs);

            return BitConverter.ToString(hash);
        }

        private void Authenticate(string authTicket)
        {
            if (string.IsNullOrEmpty(authTicket))
                CustomSoapException("AUTHENTICATION", "EMPTY_TICKET");

            var ticket = FormsAuthentication.Decrypt(authTicket);

            if (null == ticket || ticket.Expired)
                CustomSoapException("AUTHENTICATION", "EXPIRED");
            else
            {
                var id = new FormsIdentity(ticket);
                var principal = new GenericPrincipal(id, null);
                Context.User = principal;
                System.Threading.Thread.CurrentPrincipal = principal;
            }
        }

        [WebMethod]
        public bool SaveFile(string authTicket, string fileName, int folderId, string expectedHash, string mimeType)
        {
            Authenticate(authTicket);

            var FilePath = Path.Combine(UploadPath, fileName);
            var fi = new FileInfo(FilePath);

            if (!fi.Exists)
                CustomSoapException("FILE", "File doesn't exist!");

            if (expectedHash.Equals(CalcFileHash(fileName)))
            {
                BOCategory folder = fileB.GetFolder(folderId);
                if (null == folder)
                    CustomSoapException("FOLDER", "Folder doesn't exist!");

                using (var iStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                {
                    var fileData = new Byte[iStream.Length];
                    iStream.Read(fileData, 0, (int)iStream.Length);
                    var file = new BOFile
                    {
                        File = fileData,
                        Id = null,
                        Folder = folder,
                        Name = fi.Name,
                        Extension = fi.Extension,
                        MimeType = mimeType,
                        Size = fileData.Length
                    };
                    fileB.Change(file);
                }
                return true;
            }
            // hashes do not match:
            return false;
        }

        [WebMethod]
        public DirectoryNode[] GetFileTree(string authTicket)
        {
            Authenticate(authTicket);

            var folders = fileB.ListFolders();

            var directories = new DirectoryNode[folders.Count];

            for (int i = 0; i < folders.Count; i++)
            {
                directories[i] = new DirectoryNode
                                     {
                                         Id = folders[i].Id.Value,
                                         ParentId = folders[i].ParentId.HasValue ? folders[i].ParentId.Value : folders[i].Id.Value,
                                         Name = folders[i].Title
                                     };
            }
            return directories;
        }

        public class DirectoryNode
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; }
        }
    }
}
