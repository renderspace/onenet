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
using TwoControlsLibrary;
using One.Net.BLL;

namespace OneMainWeb.AdminControls
{
    public partial class FileManager : System.Web.UI.UserControl
    {
        protected static BFileSystem fileB = new BFileSystem();
        protected static FileHelper helper = new FileHelper();
        private bool expandTree;
        private bool showSelectLink;
        private string filterExtensions;
        private string containerControlType;
        private bool enableDelete;

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool ExpandTree
        {
            get { return expandTree; }
            set { expandTree = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool EnableDelete
        {
            get { return enableDelete; }
            set { enableDelete = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool ShowSelectLink
        {
            get { return showSelectLink; }
            set { showSelectLink = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public string FilterExtensions
        {
            get { return filterExtensions; }
            set { filterExtensions = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public string ContainerControlType
        {
            get { return containerControlType; }
            set { containerControlType = value; }
        }

        protected int? SelectedFileId
        {
            get { return (ViewState["SelectedFileId"] == null ? (int?)null : (int?)Int32.Parse(ViewState["SelectedFileId"].ToString())); }
            set { ViewState["SelectedFileId"] = value; }
        }

        protected int SelectedFileIdForMove
        {
            get { return ViewState["SelectedFileIdForMove"] == null ? -1 : FormatTool.GetInteger(ViewState["SelectedFileIdForMove"]); }
            set { ViewState["SelectedFileIdForMove"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDefaults();
            }
        }

        private void LoadDefaults()
        {
            SelectedFileIdForMove = -1;

            CmdRecursiveDelete.Enabled = EnableDelete;
            categorization.ExpandTree = ExpandTree;

            categorization.CategoryType = BOFile.FOLDER_CATEGORIZATION_TYPE;
            categorization.ChangeCategory = fileB.ChangeFolder;
            categorization.ListCategories = fileB.ListFolders;
            categorization.GetCategory = fileB.GetFolder;

            categoryEditor.CategoryType = BOFile.FOLDER_CATEGORIZATION_TYPE;
            categoryEditor.GetCategory = fileB.GetFolder;
            categoryEditor.ChangeCategory = fileB.ChangeFolder;
            categoryEditor.DeleteCategory = fileB.DeleteFolder;
            categoryEditor.ListCategorizedItems = helper.ListCategorizable;
            categoryEditor.ListCategories = fileB.ListFolders;
            categoryEditor.MoveCategory = fileB.MoveFolder;
            categoryEditor.EnableDelete = EnableDelete;

            categorization.LoadControls();
            categoryEditor.SelectedCategory = categorization.SelectedCategory;
            categoryEditor.LoadControls();
            LoadFiles();            
        }

        protected void CmdRecursiveDelete_Click(object sender, EventArgs e)
        {
            if (CheckBoxConfirm.Checked && categorization.SelectedCategory != null && categorization.SelectedCategory.Id.HasValue)
            {
                fileB.RecursiveFolderDelete(categorization.SelectedCategory.Id.Value);
                Notifier1.Message = ResourceManager.GetString("$recursive_delete_success");

                categorization.LoadControls();
                categoryEditor.SelectedCategory = categorization.SelectedCategory;
                categoryEditor.LoadControls();
                LoadFiles();
                CheckBoxConfirm.Checked = false;
            }
        }

        public void LoadControls()
        {
            categorization.ExpandTree = ExpandTree;
            CmdRecursiveDelete.Enabled = EnableDelete;
            categorization.LoadControls();
            categorization.TreeDataBind();
            categoryEditor.EnableDelete = EnableDelete;
            categoryEditor.LoadControls();
        }

        private void LoadFiles()
        {
            lblSearchMessage.Visible = filesHolder.Visible = divShowFile.Visible = divUploadFile.Visible = false;

            if (categorization.SelectedCategory != null && categorization.SelectedCategory.Id.HasValue)
            {
                filesHolder.Visible = divShowFile.Visible = divUploadFile.Visible = true;

                if (SelectedFileId.HasValue)
                {
                    BOFile selectedFile = fileB.Get(SelectedFileId.Value);
                    if (selectedFile != null)
                    {
                        
                        cmdOverwrite.Visible = true;
                        imagePreview.Text = GenerateFileIcon(selectedFile, 150);
                        imagePreview.Visible = true;
                        imagePreviewSize.Text = String.Format("{0:F} kb", ((float)selectedFile.Size / (float)1024));
                    }
                }
                else
                {
                    cmdOverwrite.Visible = false;
                    imagePreview.Visible = false;
                    imagePreview.Text = imagePreviewSize.Text = string.Empty;
                }
            }
        }

        private string GenerateFileIcon(BOFile file, int width)
        {
            string extension = file.Extension.ToLower().Replace(".", "");
            string ret;

            ret = "<a href=\"/_files/" + file.Id.Value + "/" + file.Name + "\" target=\"_blank\">";

            if (extension == "jpg" || extension == "gif" || extension == "png" || extension == "jpeg")// ||
            {
                ret += "<img src=\"/_files/" + file.Id.Value + "/" + file.Name + "?w=" + width + "\" /></a>";
            }
            else
            {
                ret += OneHelper.GetFileIcon(this.Page, "OneMainWeb.Res.mime_icons.", extension.Trim('.').ToLower());
            }

            return ret;
        }

        protected void categorization_NodeAdded(object sender, EventArgs e)
        {
            categoryEditor.SelectedCategory = categorization.SelectedCategory;
            categoryEditor.LoadControls();
            LoadFiles();
            Notifier1.Message = ResourceManager.GetString("$folder_successfully_added");
        }

        protected void categoryEditor_CategoryMoved(object sender, EventArgs e)
        {
            categorization.LoadControls();
            categoryEditor.LoadControls();
            Notifier1.Message = ResourceManager.GetString("$folder_successfully_moved");
        }

        protected void categoryEditor_CategoryFailedToMove(object sender, EventArgs e)
        {
            Notifier1.Warning = ResourceManager.GetString("$folder_move_failed");
        }

        protected void categoryEditor_CategoryDeleted(object sender, EventArgs e)
        {
            // load data from db in case node has been deleted
            categorization.LoadControls();

            // if node has been deleted, select parent ( note that categoryEditor.SelectedCategory is used, since it hasnt been reset )
            if (categorization.FindNode(categoryEditor.SelectedCategory) == null)
            {
                categorization.SelectedCategory = categoryEditor.SelectedCategory;
                categorization.SelectParent();
            }

            categoryEditor.SelectedCategory = categorization.SelectedCategory;
            categoryEditor.LoadControls();
            LoadFiles();
            Notifier1.Message = ResourceManager.GetString("$folder_successfully_deleted");
        }

        protected void categoryEditor_CategoryUpdated(object sender, EventArgs e)
        {
            categorization.LoadControls();
            Notifier1.Message = ResourceManager.GetString("$folder_successfully_updated");
        }

        protected void cmdSearch_Click(object sender, EventArgs e)
        {
            int searchId = FormatTool.GetInteger(InputWithButtonSearch.Value);

            BOFile existingFile = null;

            if (searchId > -1)
            {
                existingFile = fileB.Get(searchId);
            }

            if (existingFile != null)
            {
                SelectedFileId = searchId;
                categorization.SelectNode(existingFile.Folder);
                categoryEditor.SelectedCategory = categorization.SelectedCategory;
                categoryEditor.LoadControls();
                LoadFiles();
                lblSearchMessage.Text = ResourceManager.GetString("$file_found");
            }
            else
            {
                lblSearchMessage.Text = ResourceManager.GetString("$file_not_found");
            }

            lblSearchMessage.Visible = true;

            fileGrid.DataBind();
        }

        public void categorization_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedFileId = null;
            categoryEditor.SelectedCategory = categorization.SelectedCategory;
            categoryEditor.LoadControls();
            LoadFiles();
        }

        protected void cmdUpload_Click(object sender, EventArgs e)
        {
            BOFile uploadedFile = RetrieveSubmittedFile();

            if (uploadedFile != null)
            {
                fileB.Change(uploadedFile);
                fileGrid.DataBind();
                Notifier1.Message = ResourceManager.GetString("$file_successfully_uploaded");
            }
            else
            {
                Notifier1.Warning = ResourceManager.GetString("$file_upload_failed");
            }
        }

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
                    fileGrid.DataBind();
                    LoadFiles();
                    Notifier1.Message = ResourceManager.GetString("$file_successfully_overwritten");
                }
                else
                {
                    Notifier1.Warning = ResourceManager.GetString("$file_overwritte_failed");
                }
            }
        }

        protected void moveFoldersPanel_WindowClosed(object sender, EventArgs e)
        {
            ModalPanel panel = (ModalPanel) sender;
            if (panel != null)
            {
                panel.Visible = false;
            }
        }

        protected void fileGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string command = e.CommandName;
            int rowIndex = FormatTool.GetInteger(e.CommandArgument);

            if (rowIndex < 0)
                return;

            Literal LiteralHiddenFileId = fileGrid.Rows[rowIndex].FindControl("LiteralHiddenFileId") as Literal;

            if (LiteralHiddenFileId != null)
            {
                int id = FormatTool.GetInteger(LiteralHiddenFileId.Text);

                if (id > 0)
                {
                    BOFile file = fileB.Get(id);
                    TextContentControl txtFileContent = fileGrid.Rows[rowIndex].FindControl("txtFileContent") as TextContentControl;
                    ModalPanel moveFoldersPanel = fileGrid.Rows[rowIndex].FindControl("moveFoldersPanel") as ModalPanel;

                    switch (command)
                    {
                        case "MoveFile":
                            ClosePreviousPanels();
                            categoryEditor.HideMovePanel();
                            SelectedFileIdForMove = id;
                            if ( moveFoldersPanel != null)
                                moveFoldersPanel.Visible = true;
                            break;
                        case "EditRow":
                            SelectedFileId = id;
                            fileGrid.EditIndex = rowIndex;
                            break;
                        case "UpdateFile":
                            SelectedFileId = id;

                            if (txtFileContent != null && file != null)
                            {
                                if (file.Content == null)
                                {
                                    file.Content = new BOInternalContent();
                                    file.Content.Html = string.Empty;
                                }

                                file.Content.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                file.Content.Title = txtFileContent.Title;
                                file.Content.SubTitle = txtFileContent.SubTitle;
                                file.Content.Teaser = txtFileContent.Teaser;
                                fileB.Change(file);
                                Notifier1.Message = ResourceManager.GetString("$file_successfully_updated");
                            }
                            else
                            {
                                Notifier1.Message = ResourceManager.GetString("$file_update_failed");
                            }

                            fileGrid.EditIndex = -1;
                            break;
                        case "Cancel":
                            {
                                fileGrid.EditIndex = -1;
                                break;
                            }
                    }
                }
            }
        }

        private void ClosePreviousPanels()
        {
            if ( SelectedFileIdForMove > -1 )
            {
                foreach (GridViewRow row in fileGrid.Rows )
                {
                    if ( row.RowType == DataControlRowType.DataRow )
                    {
                        ModalPanel moveFoldersPanel = row.FindControl("moveFoldersPanel") as ModalPanel;
                        if ( moveFoldersPanel != null)
                            moveFoldersPanel.Visible = false;
                    }
                }
            }
        }

        protected string GetFileUrl(object objID)
        {
            string strReturn = "";
            if (ContainerControlType == "fck")
            {
                int fileID = FormatTool.GetInteger(objID);
                BOFile file = fileB.Get(fileID);
                strReturn += "<a href=\"#\" onclick=\"OpenFile('";
                strReturn += "/_files/" + fileID + "/" + file.Name;
                strReturn += "');return false;\">";
                strReturn += ResourceManager.GetString("$label_select_file");
                strReturn += "</a>";
            }
            return strReturn;
        }

        protected void FileListODS_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            SelectedFileId = null;
            LoadFiles();
            Notifier1.Message = ResourceManager.GetString("$file_successfully_deleted");
        }

        protected void FileListODS_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["filterExtensions"] = filterExtensions;
        }

        public void moveFoldersTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            bool moved = false;

            TreeView tree = sender as TreeView;
            if (tree != null)
            {
                HtmlGenericControl treeHolder = tree.Parent as HtmlGenericControl;
                if (treeHolder != null)
                {
                    ModalPanel panel = treeHolder.Parent as ModalPanel;
                    if (panel != null)
                    {
                        Literal fileIdLit = panel.FindControl("fileIdLit") as Literal;

                        if (fileIdLit != null)
                        {
                            int fileId = FormatTool.GetInteger(fileIdLit.Text);

                            BOFile file = fileB.Get(fileId);
                            int newFolderId = FormatTool.GetInteger(tree.SelectedValue);
                            BOCategory newFolder = fileB.GetFolder(newFolderId);
                            fileB.MoveFile(file, newFolder);
                            fileGrid.DataBind();
                            moved = true;
                            Notifier1.Message = ResourceManager.GetString("$file_successfully_moved");
                        }
                    }
                }
            }

            if (!moved)
                Notifier1.Message = ResourceManager.GetString("$file_moved_failed");
        }

        public void fileGrid_ForceDataBind()
        {
            fileGrid.DataBind();
        }

        protected void fileGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count >= 3)
            {
                BOFile selectedFile = e.Row.DataItem as BOFile;
                ModalPanel moveFoldersPanel = e.Row.FindControl("moveFoldersPanel") as ModalPanel;
                LinkButton cmdDelete = e.Row.FindControl("cmdDelete") as LinkButton;

                if (moveFoldersPanel != null)
                {
                    moveFoldersPanel.Title = ResourceManager.GetString("$move_file_to_folder");

                    HtmlGenericControl treeHolder = moveFoldersPanel.FindControl("treeHolder") as HtmlGenericControl;

                    if (treeHolder != null)
                    {
                        TreeView moveFoldersTree = treeHolder.FindControl("moveFoldersTree") as TreeView;
                        if (moveFoldersTree != null)
                        {
                            TreeNodeCollection treeNodes = TreeCategorization.GenerateTreeNodes(fileB.ListFolders());
                            moveFoldersTree.Nodes.Clear();
                            moveFoldersTree.Nodes.Add(treeNodes[0]);
                            moveFoldersTree.ExpandAll();
                        }
                    }
                }

                if (selectedFile != null && cmdDelete != null)
                {
                    if (fileB.ListFileUses(selectedFile.Id.Value).Count > 0)
                        cmdDelete.OnClientClick = @"return confirm('" + ResourceManager.GetString("$label_file_is_linked_confirm_delete") + @"');";

                    cmdDelete.Enabled = EnableDelete;

                    Literal litSelectButton = e.Row.Cells[3].FindControl("litSelectButton") as Literal;
                    if (litSelectButton != null)
                    {
                        litSelectButton.Visible = ShowSelectLink;
                    }

                    Literal imageSymbol = e.Row.Cells[0].FindControl("imageSymbol") as Literal;

                    if (imageSymbol != null)
                        imageSymbol.Text = GenerateFileIcon(selectedFile, 50);

                    if (selectedFile.Id == FormatTool.GetInteger(InputWithButtonSearch.Value))
                    {
                        e.Row.CssClass += " selFile";// imageSymbol.Text += "<hr/>";
                    }

                    if (e.Row.RowState == (DataControlRowState.Edit | DataControlRowState.Alternate))
                    {
                        TextContentControl txtFileContent = e.Row.FindControl("txtFileContent") as TextContentControl;
                        if (txtFileContent != null && selectedFile.Content != null)
                        {
                            txtFileContent.Title = selectedFile.Content.Title;
                            txtFileContent.SubTitle = selectedFile.Content.SubTitle;
                            txtFileContent.Teaser = selectedFile.Content.Teaser;
                        }
                    }
                }
            }
        }

        private BOFile RetrieveSubmittedFile()
        {
            BOFile file = null;

            if (fileUpload != null && fileUpload.HasFile && categorization.SelectedCategory != null && categorization.SelectedCategory.Id.HasValue)
            {
                BOCategory folder = fileB.GetFolder(categorization.SelectedCategory.Id.Value);

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
                                   Size = ((int) fileData.Length)
                               };
                }
            }

            return file;
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object[] cSThis = new object[6];
            cSThis[0] = base.SaveControlState();
            cSThis[1] = showSelectLink;
            cSThis[2] = containerControlType;
            cSThis[3] = filterExtensions;
            cSThis[4] = expandTree;
            cSThis[5] = enableDelete;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            showSelectLink = (bool)cSThis[1];
            containerControlType = (string)cSThis[2];
            filterExtensions = (string)cSThis[3];
            expandTree = (bool)cSThis[4];
            enableDelete = (bool)cSThis[5];
            base.LoadControlState(cSBase);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
    }

    [Serializable]
    public class FileHelper
    {
        readonly BFileSystem fileB = new BFileSystem();

        public List<BOFile> List(int folderId, string filterExtensions)
        {
            List<BOFile> files = fileB.List(folderId);

            if (string.IsNullOrEmpty(filterExtensions))
            {
                return files;
            }
            else
            {
                filterExtensions = filterExtensions.Trim();
                filterExtensions = filterExtensions.ToLower();
                string[] extensionsArray = filterExtensions.Split(',');
                List<string> extensionsList = new List<string>();

                if (extensionsArray.Length == 0)
                {
                    return files;
                }
                else
                {
                    foreach (string extension in extensionsArray)
                        extensionsList.Add(extension);

                    files.RemoveAll
                    (
                        delegate(BOFile file)
                        {
                            return !extensionsList.Contains(file.Extension.ToLower());
                        }
                    );

                    return files;
                }
            }
        }

        public void Delete(int Id)
        {
            fileB.Delete(Id);
        }

        public List<ICategorizable> ListCategorizable(int categoryId)
        {
            List<BOFile> files = fileB.List(categoryId);
            List<ICategorizable> categorizables = new List<ICategorizable>();
            foreach (BOFile file in files)
            {
                categorizables.Add(file);
            }
            return categorizables;
        }
    }
}