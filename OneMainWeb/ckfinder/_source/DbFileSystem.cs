using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using One.Net.BLL;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Caching;
using CKFinder.Connector;
using System.Threading;

namespace CKFinder
{
	public class DbFileSystem
	{
		private static readonly Regex regex = new Regex(@"/_files/(?<fileId>[^/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static BFileSystem fileSystemB = new BFileSystem();

		private DbFileSystem()
		{ }

		public static BOCategory GetRootFolder()
		{
			var folderList = ListFolders();
			BOCategory rootFolder = null;
			if (folderList == null || folderList.Count == 0)
			{
				var folder = new BOCategory();
				folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
				folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
				folder.Title = "Files";
				folder.Teaser = folder.SubTitle = folder.Html = string.Empty;
				folder.IsSelectable = true;
				folder.IsPrivate = false;
				folder.ChildCount = 0;
				folder.ParentId = null;
				fileSystemB.ChangeFolder(folder);
				rootFolder = folder;
			}
			else
			{
				rootFolder = folderList[0];
			}
			return rootFolder;
		}

		public static List<BOCategory> ListFolders()
		{
			return fileSystemB.ListFolders();
		}

		public static List<BOCategory> ListChildFolders(int folderId)
		{
			var folderList = new List<BOCategory>();
			var childList = fileSystemB.ListFolders().FindAll(f => f.ParentId.HasValue && f.ParentId.Value == folderId);
			if (childList == null || childList.Count == 0)
				return folderList;
			return childList;
		}

		public static BOCategory GetFolder(int folderId)
		{
			return fileSystemB.GetFolder(folderId);
		}

		private static BOCategory GetChildFolder(string[] pathArray)
		{
			var childFolder = GetRootFolder();
			if (pathArray.Length == 0 || childFolder == null)
				return null;

			var tempFolder = childFolder;
			for (var i=0; i<pathArray.Length; i++)
			{
				var folderList = ListChildFolders(childFolder.Id.Value);
				tempFolder = folderList.Find(f => f.Title == pathArray[i]);
				if (tempFolder == null)
					break;

				childFolder = tempFolder;
			}
			return childFolder;
		}

		public static BOCategory GetFolder(string relativePath)
		{
			var foldersArray = relativePath.Split(new [] {  '/'  }, StringSplitOptions.RemoveEmptyEntries );
			var folder = GetChildFolder(foldersArray);
			if (folder == null) return GetRootFolder();
			return folder;
		}

		public static List<BOFile> ListFiles(int folderId)
		{
			return fileSystemB.List(folderId);
		}

		public static byte[] GetFileBytes(string fileName)
		{
			var fileId = RetrieveFileId(fileName);
			if (fileId.HasValue)
			{
				return BFileSystem.GetCachedFileBytes(fileId.Value);
			}
			return null;
		}

		private static int? RetrieveFileId(string uri)
		{
			var m = regex.Match(uri);
			if (m.Success) return Int32.Parse(m.Groups["fileId"].Value);
			return null;
		}

		public static void ProcessImage(string virtualPath, int width, int height, int quality)
		{
			TImageHandler.ProcessRequest(virtualPath, width, height, quality);
		}

		public static void DeleteFile(string virtualPath)
		{
			var fileId = RetrieveFileId(virtualPath);
			if (fileId.HasValue)
			{
				fileSystemB.Delete(fileId.Value);
			}
		}

		public static void DeleteFolder(string path)
		{
			var folder = GetFolder(path);
			if (folder.Id.HasValue)
			{
				var fileList = fileSystemB.List(folder.Id.Value);
				if (fileList.Count > 0)
				{
					for (var i=0; i<fileList.Count; i++)
					{
						fileSystemB.Delete(fileList[i].Id.Value);
					}
				}
				fileSystemB.DeleteFolder(folder.Id.Value);
			}
			else
			{
				ConnectorException.Throw( Errors.FolderNotFound );
			}
		}

		public static void AddFile(string folderPath, string fileName, HttpPostedFile postedFile)
		{
			postedFile.InputStream.Position = 0;
			BOCategory folder = GetFolder(folderPath);
            if (folder != null)
			{
				byte[] fileData;
				using (postedFile.InputStream)
				{
					fileData = new Byte[postedFile.InputStream.Length];
					postedFile.InputStream.Read(fileData, 0, (int)postedFile.InputStream.Length);
					postedFile.InputStream.Close();
				}
				fileSystemB.Change(new BOFile
				{
					File = fileData,
					Id = null,
					Folder = folder,
					Name = fileName,
					Extension = System.IO.Path.GetExtension(postedFile.FileName),
					MimeType = postedFile.ContentType,
					Size = ((int) fileData.Length)
				});
			}
		}

		public static void CopyFile(string folderPath, string filePath, /* not implemented since the files are always unique (unique filename) */bool overwrite)
		{
			BOCategory folder = GetFolder(folderPath);
            if (folder != null)
			{
				var fileId = RetrieveFileId(filePath);
				BOFile file = null;
				if (fileId.HasValue)
					file = fileSystemB.Get(fileId.Value);
				if (file == null)
					return;

				var fileData = GetFileBytes(filePath);
				fileSystemB.Change(new BOFile
				{
					File = fileData,
					Id = null,
					Folder = folder,
					Name = file.Name,
					Extension = file.Extension,
					MimeType = file.MimeType,
					Size = ((int) fileData.Length)
				});
			}
		}

		public static void MoveFile(string folderPath, string filePath)
		{
			BOCategory folder = GetFolder(folderPath);
            if (folder == null) return;

			var file = GetFile(filePath);
			if (file == null) return;

			fileSystemB.MoveFile(file, folder);
		}

		public static bool FolderExists(string parentFolderPath, string folderPath)
		{
			var parentFolder = GetFolder(parentFolderPath);
			if (parentFolder.Id.HasValue)
			{
				var folderList = ListChildFolders(parentFolder.Id.Value);
				if (folderList.FindAll(f => f.Title == folderPath).Count > 0)
					return true;
				return false;
			}
			return true;
		}

		public static bool FolderExists(string folderPath)
		{
			return GetFolder(folderPath) != null;
		}

		public static void CreateFolder(string parentFolderPath, string folderPath)
		{
			var folder = new BOCategory();
			folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
			folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
			folder.Title = folderPath;
			folder.Teaser = folder.SubTitle = folder.Html = string.Empty;
			folder.IsSelectable = true;
			folder.IsPrivate = false;
			folder.ChildCount = 0;

			if (parentFolderPath == null || parentFolderPath == "/")
			{
				folder.ParentId = GetRootFolder().Id;
			}
			else
			{
				folder.ParentId = GetFolder(parentFolderPath).Id;
			}
			fileSystemB.ChangeFolder(folder);
		}

		public static void RenameFolder(string folderPath, string newTitle)
		{
			var folder = GetFolder(folderPath);
			folder.Title = newTitle;
			fileSystemB.ChangeFolder(folder);
		}

		public static void RenameFile(string filePath, string newTitle)
		{
			var fileId = RetrieveFileId(filePath);
			if (fileId.HasValue)
			{
				var file = fileSystemB.Get(fileId.Value);
				file.Name = newTitle;
				fileSystemB.Change(file);
			}
		}

		public static bool FileExists(string filePath)
		{
			var fileId = RetrieveFileId(filePath);
			if (fileId.HasValue)
			{
				var file = fileSystemB.Get(fileId.Value);
				return file != null;
			}
			return false;
		}

		public static BOFile GetFile(string filePath)
		{
			var fileId = RetrieveFileId(filePath);
			BOFile file = null;
			if (fileId.HasValue)
			{
				file = fileSystemB.Get(fileId.Value);
			}
			return file;
		}
	}
}