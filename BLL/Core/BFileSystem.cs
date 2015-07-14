using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Transactions;
using System.Web.Caching;
using One.Net.BLL.DAL;

namespace One.Net.BLL
{
    [Serializable]
    public class BFileSystem : BusinessBaseClass
    {
        private static readonly BLICategorization categorizationB = new BLICategorization();
        private static readonly BInternalContent contentB = new BInternalContent();
        private static readonly DbFileSystem fileDb = new DbFileSystem();

        protected const string CACHE_ID = "BOFile1_";
        protected const string BYTES_CACHE_ID = "BOFile_bytes";

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingFile = new Object();
        private static readonly object cacheLockingFileBytes = new Object();

        public int UploadFileFromDisk(string inputFilename, int folderId, string localizedTitle, string mimeType)
        {
            var fi = new FileInfo(inputFilename);
            var folder = GetFolder(folderId);

            if (!fi.Exists || folder == null)
                return 0;

            byte[] fileData = File.ReadAllBytes(fi.FullName);

            if (fileData.Length < 1)
                return 0;

            var file = new BOFile
            {
                File = fileData,
                Id = null,
                Folder = folder,
                Name = fi.Name,
                Extension = fi.Extension,
                MimeType = mimeType,
                Size = ((int)fileData.Length)
            };

            if (!string.IsNullOrWhiteSpace(localizedTitle))
            {
                file.Content = new BOInternalContent();
                file.Content.Html = "";
                file.Content.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                file.Content.Title = localizedTitle;
                file.Content.SubTitle = "";
                file.Content.Teaser = "";
            }
            Change(file);
            return file.Id.Value;
        }

        /// <summary>
        /// Changes enclosed BOInternalContent object.
        /// Changes BOFile object ( meta data and file data depending on BinaryDataMissing).
        /// Clears BelongsTo relationships. Assigns newly provided BelongsTo relationships.
        /// Categorizes file against given folder.
        /// Neither should it be pulled out in BelongsToList.
        /// </summary>
        /// <param name="file">
        /// File object must contain initialized Folder object that file belongs to.
        /// </param>
        public void Change(BOFile file)
        {
            if (file == null)
                throw new ArgumentException("file object is not initialized so no change can occur.");

            if (file.Folder == null)
                throw new ArgumentException("file.Folder is null.");

            if (file.Content != null)
                contentB.Change(file.Content);

            fileDb.ChangeMeta(file);

            // Only change the binary data if the binary data is provided
            if (!file.BinaryDataMissing)
                fileDb.ChangeData(file);

            // Categorize the item against folder info
            categorizationB.Categorize(file.Folder, file.Id.Value);

            cache.Remove(CACHE_ID + file.Id.Value);
            // the following string must be the same as
            cache.RemoveWithPartialKey(DYNAMIC_FOLDER_CACHE_ID(file.Folder.Id.Value, LanguageId));
            cache.Remove(BYTES_CACHE_ID + file.Id.Value);
        }

        private static string DYNAMIC_FOLDER_CACHE_ID(int folderId, int languageId)
        {
            return "FOLDER_L_" + folderId + "_" + languageId;
        }

        /// <summary>
        /// Deletes content, belongsto and file objects based on given file id.
        /// </summary>
        /// <param name="Id"></param>
        public bool Delete(int Id) 
        {
            BOFile file = GetUnCached(Id);
            if (file != null)
            {
                if (file.Folder != null)
                {
                    // TODO: clear all languages from cache!
                    cache.RemoveWithPartialKey(DYNAMIC_FOLDER_CACHE_ID(file.Folder.Id.Value, LanguageId));
                    categorizationB.RemoveCategorizationFromItem(file.Folder, Id);
                }
                if (file.Content != null)
                {
                    // we have to remove the connection before delete, otherwise FK will fail.
                    int tempContentId = file.Content.ContentId.Value;
                    file.Content = null;
                    fileDb.ChangeMeta(file);
                    contentB.Delete(tempContentId);
                }

                //// Check and clear file uses if any
                //List<string> fileUses = ListFileUses(Id);
                //foreach (string fileUse in fileUses)
                //{
                //    switch (fileUse)
                //    {
                //        case BOForm.FILE_USE_FORMS: fileDb.ClearFormFileUse(Id); break;
                //    }
                //}

                fileDb.Delete(Id);
                cache.Remove(CACHE_ID + Id);
                cache.Remove(BYTES_CACHE_ID + Id);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Retrieves file object, it's internal content, its folder.
        /// Method is fully cached.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BOFile Get(int id)
        {
            BOFile file = cache.Get<BOFile>(CACHE_ID + id);
            if (file == null)
            {
                file = GetUnCached(id);
                if (file != null)
                {
                    lock (cacheLockingFile)
                    {
                        BOFile tempFile = cache.Get<BOFile>(CACHE_ID + id);
                        if (null == tempFile)
                            cache.Put(CACHE_ID + id, file);
                    }
                }
            }
            return file;
        }

        /// <summary>
        /// Retrieves file object, it's internal content,  its folder.
        /// Method is not cached.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private BOFile GetUnCached(int id) 
        {
            //CacheDependency cacheDep;

            BOFile file = fileDb.Get(id, LanguageId);
            if (file != null )
            {
                List<BOCategory> folders = categorizationB.ListAssignedToItem(BOFile.FOLDER_CATEGORIZATION_TYPE, file.Id.Value, false);
                if ( folders.Count > 0)
                    file.Folder = folders[0];
                if (file.ContentId.HasValue)
                {
                    // changed from get to getuncached
                    file.Content = contentB.GetUnCached(file.ContentId.Value);
                }
            }

            return file;        
        }

        /// <summary>
        /// This method returns the file data. It is used primarily when metadata is not required.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static byte[] GetCachedFileBytes(int fileId)
        {
            byte[] bytes = cache.Get<byte[]>(BYTES_CACHE_ID + fileId);
            if (bytes == null)
            {
                bytes = fileDb.GetFileBytes(fileId);
                if (bytes != null)
                {
                    lock (cacheLockingFileBytes)
                    {
                        byte[] tempBytes = cache.Get<byte[]>(BYTES_CACHE_ID + fileId);
                        if (null == tempBytes)
                        {
                            cache.Put(BYTES_CACHE_ID + fileId, bytes);
                        }
                    }
                }
            }

            return bytes;
        }

        public static byte[] GetUnCachedFileBytes(int fileId)
        {
            return fileDb.GetFileBytes(fileId);
        }
        
        /// <summary>
        /// List files based on given folder
        /// Method is fully cached.
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public List<BOFile> List(int folderId) 
        {
            var list = cache.Get<List<BOFile>>(DYNAMIC_FOLDER_CACHE_ID(folderId, LanguageId));
            if (list == null)
            {
                list = fileDb.ListFolder(folderId, LanguageId, "f.id", SortDir.Descending);
                cache.Put(DYNAMIC_FOLDER_CACHE_ID(folderId, LanguageId), list);
            }
            // return clone, because the will sort it later.
            return new List<BOFile>(list);
        }

        /// <summary>
        /// List files based on given folder
        /// Method is fully cached.
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public List<BOFile> List(int folderId, string sortBy, SortDir sortDir)
        {
            var list = cache.Get<List<BOFile>>(DYNAMIC_FOLDER_CACHE_ID(folderId, LanguageId) + ":" + sortBy + ":" + sortDir.ToString());
            if (list == null)
            {
                list = fileDb.ListFolder(folderId, LanguageId, sortBy, sortDir);
                cache.Put(DYNAMIC_FOLDER_CACHE_ID(folderId, LanguageId) + ":" + sortBy + ":" + sortDir.ToString(), list);
            }
            // return clone, because the will sort it later.
            return new List<BOFile>(list);
        }

        /// <summary>
        /// Move the file from one folder to the other.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newFolder"></param>
        public void MoveFile(BOFile file, BOCategory newFolder)
        {
            categorizationB.MoveItem(file.Id.Value, file.Folder, newFolder);

            cache.Remove(CACHE_ID + file.Id.Value);
            // the following string must be the same as
            cache.RemoveWithPartialKey(DYNAMIC_FOLDER_CACHE_ID(newFolder.Id.Value, LanguageId));
            cache.RemoveWithPartialKey(DYNAMIC_FOLDER_CACHE_ID(file.Folder.Id.Value, LanguageId));
        }

        //public List<string> ListFileUses(int fileId)
        //{
        //    return fileDb.ListFileUses(fileId);
        //}
        
        /// <summary>
        /// Allows change of folder if BOCategory is of BOFile.FOLDER_CATEGORIZATION_TYPE type only.
        /// </summary>
        /// <param name="folder"></param>
        public void ChangeFolder(BOCategory folder) 
        {
            categorizationB.ChangeCategory(folder, BOFile.FOLDER_CATEGORIZATION_TYPE);
        }

        public void RecursiveFolderDelete(int folderId)
        {
            List<BOCategory> children = categorizationB.ListChildren(folderId, true);
            List<BOFile> files = this.List(folderId);
            
            foreach ( BOFile file in files )
            {
                this.Delete(file.Id.Value);
            }

            foreach (BOCategory child in children)
            {
                this.RecursiveFolderDelete(child.Id.Value);
            }

            this.DeleteFolder(folderId);
        }

        /// <summary>
        /// Allows delete of folder if BOCategory is of BOFile.FOLDER_CATEGORIZATION_TYPE type only.
        /// </summary>
        /// <param name="folderId"></param>
        public bool DeleteFolder(int folderId) 
        {
            List<BOFile> files = List(folderId);
            BOCategory folder = GetFolder(folderId);
            if (files.Count == 0 && folder.ChildCount == 0)
            {
                categorizationB.DeleteCategory(folderId, BOFile.FOLDER_CATEGORIZATION_TYPE);
                return true;
            }
            else
                return false;
        }
        
        /// <summary>
        /// Allows move of folder if BOCategory is of BOFile.FOLDER_CATEGORIZATION_TYPE type only.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="newParent"></param>
        public void MoveFolder(BOCategory folder, BOCategory newParent) 
        {
            categorizationB.MoveCategory(folder, newParent, BOFile.FOLDER_CATEGORIZATION_TYPE);
        }

        /// <summary>
        /// List folders where BOCategory is of BOFile.FOLDER_CATEGORIZATION_TYPE type only.
        /// </summary>
        /// <returns></returns>
        public List<BOCategory> ListFolders()
        {
            return categorizationB.List(BOFile.FOLDER_CATEGORIZATION_TYPE, false);
        }

        /// <summary>
        /// Retrieves BOCategory folder object from BInternalCategorization
        /// Category caching is based on publish flag set in config
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public BOCategory GetFolder(int folderId)
        {
            return categorizationB.Get(folderId, BOFile.FOLDER_CATEGORIZATION_TYPE, false);
        }
        
        public BOCategory GetFolder(int folderId, bool voidDontEverUseThisParameterItDoesNothing)
        {
            return categorizationB.Get(folderId, BOFile.FOLDER_CATEGORIZATION_TYPE, false);
        }

        public static List<FileSystemInfo> ListPhysicalFolder(string path, string rootPath)
        {
            bool isRootPath = String.Compare(path, rootPath) == 0;

            DirectoryInfo diri = new DirectoryInfo(path);
            if (!diri.Exists)
                return null;

            List<FileSystemInfo> filesList = new List<FileSystemInfo>();

            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                FileAttributes attributeReader = di.Attributes;

                if (((attributeReader & FileAttributes.Hidden) != FileAttributes.Hidden) &&
                    ((attributeReader & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) &&
                    ((attributeReader & FileAttributes.System) != FileAttributes.System) &&
                    !(isRootPath && di.Name == "...")
                    )
                    filesList.Add(di);
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                FileAttributes attributeReader = fi.Attributes;

                if (((attributeReader & FileAttributes.Hidden) != FileAttributes.Hidden) &&
                    ((attributeReader & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) &&
                    ((attributeReader & FileAttributes.System) != FileAttributes.System))
                    filesList.Add(fi);
            }

            DirectoryInfo current = new DirectoryInfo(path);
            filesList.Insert(0, current);

            return filesList;
        }

        public static byte[] GetFileIcon(string extension)
        {
            var location = "Utility.mime_icons.";
            byte[] ret = null;
            switch (extension)
            {
                case "ai":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "avi":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "cs":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "dll":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "doc":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "exe":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "fla":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "gif":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "htm":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "html":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "jpg":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "js":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "mdb":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "mp3":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "pdf":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "ppt":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "rdp":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "swf":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "swt":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "txt":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "vsd":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "xls":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "xml":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                case "zip":
                    ret = StringTool.GetFileContentFromResource( location + extension + ".gif");
                    break;
                default:
                    ret = StringTool.GetFileContentFromResource( location + "default.icon.gif");
                    break;
            }
            return ret;
        }
    }
}
