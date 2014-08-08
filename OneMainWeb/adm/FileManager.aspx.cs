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
                HiddenSelectedFolderId.Value = value.ToString();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                HiddenFieldLanguageId.Value = Thread.CurrentThread.CurrentCulture.LCID.ToString();

                if (SelectedFolderId < 1)
                {
                    var folders = fileB.ListFolders();
                    var rootFolder = folders.Where(f => !f.ParentId.HasValue).FirstOrDefault();
                    if (rootFolder != null)
                        SelectedFolderId = rootFolder.Id.Value;
                }
            }
        }

        protected void CmdRecursiveDelete_Click(object sender, EventArgs e)
        {
            if (CheckBoxConfirm.Checked && SelectedFolderId  > 0)
            {
                fileB.RecursiveFolderDelete(SelectedFolderId);
                Notifier1.Message = "$recursive_delete_success";
                CheckBoxConfirm.Checked = false;
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
            /*
            int searchId = FormatTool.GetInteger(TextBoxSearch.Text);

            BOFile existingFile = null;

            if (searchId > -1)
            {
                existingFile = fileB.Get(searchId);
            }

            if (existingFile != null)
            {
                categorization.SelectNode(existingFile.Folder);
                FileManager_DataBind();
                lblSearchMessage.Text = "$file_found");
            }
            else
            {
                lblSearchMessage.Text = "$file_not_found");
            }

            lblSearchMessage.Visible = true;

            GridViewFiles.DataBind(); */
        }

        protected void cmdUpload_Click(object sender, EventArgs e)
        {
            BOFile uploadedFile = RetrieveSubmittedFile();

            if (uploadedFile != null)
            {
                fileB.Change(uploadedFile);
                //GridViewFiles.DataBind();
                Notifier1.Message = "$file_successfully_uploaded";
            }
            else
            {
                Notifier1.Warning = "$file_upload_failed";
            }
        }

        /*
        protected void cmdOverwrite_Click(object sender, EventArgs e)
        {
            if (SelectedFileId.HasValue)
            {
                BOFile uploadedFile = RetrieveSubmittedFile();
                BOFile existingFile = fileB.Get(SelectedFileId.Value);

                if (uploadedFile != null && existingFile != null)
                {
                    existingFile.File = uploadedFile.File;
                    existingFile.Size = uploadedFile.Size;
                    existingFile.MimeType = uploadedFile.MimeType;
                    existingFile.Extension = uploadedFile.Extension;
                    existingFile.Content = null; // BLL will not change content if it is null

                    fileB.Change(existingFile);
                    GridViewFiles.DataBind();
                    LoadFiles();
                    Notifier1.Message = "$file_successfully_overwritten");
                }
                else
                {
                    Notifier1.Warning = "$file_overwritte_failed");
                }
            }
        }*/

        private BOFile RetrieveSubmittedFile()
        {
            BOFile file = null;

            if (fileUpload != null && fileUpload.HasFile && SelectedFolderId > 0)
            {
                BOCategory folder = fileB.GetFolder(SelectedFolderId);

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
            }

            return file;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        
        /*
        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in GridViewFiles.Rows)
            {
                CheckBox chkForPublish = row.FindControl("chkFor") as CheckBox;
                Literal litArticleId = row.FindControl("litId") as Literal;

                if (litArticleId != null && chkForPublish != null && chkForPublish.Checked)
                {
                    int articleId = FormatTool.GetInteger(litArticleId.Text);
                    if (articleId > 0)
                    {
                        result.Add(articleId);
                    }
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
                /*
                if (fileB.ListFileUses(selectedFile.Id.Value).Count > 0)
                    cmdDelete.OnClientClick = @"return confirm('" + "$label_file_is_linked_confirm_delete") + @"');";

                if (fileB.Delete(i))
                {
                    deletedCount++;
                }*//*
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} files", deletedCount);
                //RegularDataBind();
            }
        }
*/
    }
}
