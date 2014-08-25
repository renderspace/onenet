using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Reflection;
using System.Linq;

using One.Net.BLL;
using One.Net.BLL.WebControls;
using System.IO;

namespace OneMainWeb
{
    public partial class FileManager : OneBasePage
    {
        protected static BFileSystem fileB = new BFileSystem();
        private bool showSelectLink;

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool ShowSelectLink
        {
            get { return showSelectLink; }
            set { showSelectLink = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public int SelectedFolderId {
            get
            {
                var id = 0;
                if (int.TryParse(HiddenSelectedFolderId.Value, out id))
                {
                    return id;
                }
                return 0;
            }
            set
            {
                LabelFolderId.Text = value.ToString();
                HiddenSelectedFolderId.Value = value.ToString();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SelectedWebsite == null)
            {
                Notifier1.Warning = "You don't have permissions for any site or there are no websites defined in database.";
                return;
            }

            RetrieveSubmittedFiles();

            if (!IsPostBack)
            {
                HiddenFieldLanguageId.Value = Thread.CurrentThread.CurrentCulture.LCID.ToString();

                if (SelectedFolderId < 1)
                {
                    SetRootAsSelected();
                }
            }
        }

        private void RetrieveSubmittedFiles()
        {
            var count = 0;
            BOFile file = null;
            var parameterFolderId = 0;
            if (SelectedFolderId > 0 || (Request["SelectedFolderId"] != null && int.TryParse(Request["SelectedFolderId"], out parameterFolderId)))
            {
                if (parameterFolderId > 0)
                    SelectedFolderId = parameterFolderId;
                BOCategory folder = fileB.GetFolder(SelectedFolderId);
                if (folder != null)
                {
                    foreach (string s in Request.Files)
                    {
                        HttpPostedFile postedFile = Request.Files[s];

                        int fileSizeInBytes = postedFile.ContentLength;
                        string fileName = postedFile.FileName; // Request.Headers["X-File-Name"];
                        string fileExtension = "";

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            fileExtension = Path.GetExtension(fileName);

                            byte[] fileData = new Byte[postedFile.InputStream.Length];
                            postedFile.InputStream.Read(fileData, 0, (int)postedFile.InputStream.Length);
                            postedFile.InputStream.Close();

                            file = new BOFile
                            {
                                File = fileData,
                                Id = null,
                                Folder = folder,
                                Name = fileName,
                                Extension = fileExtension,
                                MimeType = postedFile.ContentType,
                                Size = ((int)fileData.Length)
                            };
                            fileB.Change(file);
                        }
                    }
                    Notifier1.Message = "Uploaded " + count + " files.";
                }
            }

            /*
            if (fileUpload != null && fileUpload.HasFile &&)
            {
                

                string filePath = fileUpload.PostedFile.FileName;
                var fi = new System.IO.FileInfo(filePath);

                if (folder != null)
                {
                    byte[] fileData;
                    using (fileUpload.PostedFile.InputStream)
                    {
                        fileData = new Byte[fileUpload.PostedFile.InputStream.Length];
                        fileUpload.PostedFile.InputStream.Read(fileData, 0, (int)fileUpload.PostedFile.InputStream.Length);
                        fileUpload.PostedFile.InputStream.Close();
                    }

                    file = new BOFile
                    {
                        File = fileData,
                        Id = null,
                        Folder = folder,
                        Name = fi.Name,
                        Extension = fi.Extension,
                        MimeType = fileUpload.PostedFile.ContentType,
                        Size = ((int)fileData.Length)
                    };
                }
            }*/
        }

        private void SetRootAsSelected()
        {
            var folders = fileB.ListFolders();
            var rootFolder = folders.Where(f => !f.ParentId.HasValue).FirstOrDefault();
            if (rootFolder != null)
                SelectedFolderId = rootFolder.Id.Value;
        }

        protected void CmdRecursiveDelete_Click(object sender, EventArgs e)
        {
            if (CheckBoxConfirm.Checked && SelectedFolderId > 0)
            {
                fileB.RecursiveFolderDelete(SelectedFolderId);
                Notifier1.Message = "Folder deleted";
                CheckBoxConfirm.Checked = false;
                SetRootAsSelected();
            }
            else
            {
                Notifier1.Warning = "Please use checkbox to confirm recursive delete";
            }
        }

        

        protected void TreeNodeAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextBoxFolder.Text))
            {
                BOCategory folder = new BOCategory();

                folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
                folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                folder.Title = TextBoxFolder.Text;
                folder.Teaser = folder.SubTitle = folder.Html = "";
                folder.IsSelectable = true;
                folder.IsPrivate = false;
                folder.ChildCount = 0;

                if (SelectedFolderId < 1)
                {
                    var folders = fileB.ListFolders();
                    var rootFolder = folders.Where(f => !f.ParentId.HasValue).FirstOrDefault();
                    if (rootFolder != null)
                    {
                        throw new Exception("Trying to add root folder where it already exists");
                    }
                    folder.ParentId = null;
                }
                else
                {
                    folder.ParentId = SelectedFolderId;
                }

                fileB.ChangeFolder(folder);
                Notifier1.Title = "Folder created.";
                TextBoxFolder.Text = "";
            }
        }

        protected void cmdSearch_Click(object sender, EventArgs e)
        {
            BOFile existingFile = null;
            int searchId = FormatTool.GetInteger(TextBoxSearch.Text);
            if (searchId > -1)
            {
                existingFile = fileB.Get(searchId);
            }
            if (existingFile != null)
            {
                SelectedFolderId = existingFile.Folder.Id.Value;
            }
            else
            {
                Notifier1.Warning = "File ID not found";
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            var post = Request.Form["fileIdToDelete"];
            if (!string.IsNullOrWhiteSpace(post))
            {
                var filesList = post.Split(',').ToList();
                foreach (var f in filesList)
                {
                    int fileId = FormatTool.GetInteger(f);
                    if (fileId > 0)
                        result.Add(fileId);
                }
            }
            return result;
        } 

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            int deletedCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                if (fileB.ListFileUses(i).Count == 0)
                {
                    if (fileB.Delete(i))
                    {
                        deletedCount++;
                    }
                }
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} files", deletedCount);
            }
        }
    }
}
