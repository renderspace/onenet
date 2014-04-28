using System;
using System.Collections.Generic;
using System.Text;

using One.Net.BLL;

namespace OneMaintenance
{
    public class FolderManagement
    {
        BFileSystem fileSystem = new BFileSystem();

        public void RecursiveDeleteFolders(int folderId)
        {
            DeleteSubFolder(folderId);
        }

        private void DeleteSubFolder(int parentFolderId)
        {
            List<BOCategory> allFolders = fileSystem.ListFolders();

            foreach (BOCategory folder in allFolders)
            {
                if (folder.ParentId == parentFolderId)
                {
                    DeleteSubFolder(folder.Id.Value);
                    /*
                    List<BOFile> files = fileSystem.List(folder.Id.Value);
                    foreach (BOFile file in files)
                    {
                        fileSystem.Delete(file.Id.Value);
                        Console.WriteLine("Deleted file: " + file.ToString());
                    }*/
                    fileSystem.DeleteFolder(folder.Id.Value);
                    Console.WriteLine("* Deleted folder: " + folder.Id.Value);
                }
            }
        }
    }
}
