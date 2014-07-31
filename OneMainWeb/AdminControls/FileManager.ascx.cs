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

using One.Net.BLL;
using One.Net.BLL.WebControls;

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

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool ExpandTree
        {
            get { return expandTree; }
            set { expandTree = value; }
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

        [Bindable(false), Category("Data"), DefaultValue("")]
        public BOCategory SelectedFolder { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileManager_DataBind();

                
            }
        }

        public void FileManager_DataBind()
        {

            var treeNodes = GenerateTreeNodes(fileB.ListFolders());
            TreeViewFolders.Nodes.Add(treeNodes[0]);
            TreeViewFolders.DataBind();

            lblSearchMessage.Visible = filesHolder.Visible = PanelUpload.Visible = false;

            if (SelectedFolder != null && SelectedFolder.Id.HasValue)
            {
                filesHolder.Visible = PanelUpload.Visible = true;
            }

            CmdRecursiveDelete.Enabled = true;
        }

        #region Helper methods

        public static TreeNodeCollection GenerateTreeNodes(List<BOCategory> categories)
        {
            TreeNodeCollection nodeColl = new TreeNodeCollection();

            TreeNode root = null;

            foreach (BOCategory category in categories)
            {
                if (!category.ParentId.HasValue)
                {
                    root = new TreeNode(category.Title, category.Id.ToString());
                    nodeColl.Add(root);
                }
            }

            AddChildren(root, categories);

            return nodeColl;
        }

        private static void AddChildren(TreeNode node, List<BOCategory> categories)
        {
            foreach (BOCategory category in categories)
            {
                if (category.ParentId.HasValue && category.ParentId.Value.ToString() == node.Value)
                {
                    node.ChildNodes.Add(new TreeNode(category.Title, category.Id.ToString()));
                }
            }
            if (node != null)
            {
                foreach (TreeNode childNode in node.ChildNodes)
                {
                    AddChildren(childNode, categories);
                }
            }
        }

        #endregion Helper methods

        protected void CmdRecursiveDelete_Click(object sender, EventArgs e)
        {
            if (CheckBoxConfirm.Checked && SelectedFolder != null && SelectedFolder.Id.HasValue)
            {
                fileB.RecursiveFolderDelete(SelectedFolder.Id.Value);
                Notifier1.Message = ResourceManager.GetString("$recursive_delete_success");
                FileManager_DataBind();
                CheckBoxConfirm.Checked = false;
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

        protected void TreeNodeAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxFolder.Text))
            {
                BOCategory folder = new BOCategory();

                folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
                folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                folder.Title = TextBoxFolder.Text;
                folder.Teaser = folder.SubTitle = folder.Html = "";
                folder.IsSelectable = true;
                folder.IsPrivate = false;
                folder.ChildCount = 0;

                if (SelectedFolder == null || !SelectedFolder.Id.HasValue)
                {
                    folder.ParentId = null;
                }
                else
                {
                    folder.ParentId = SelectedFolder.Id.Value;
                }

                fileB.ChangeFolder(folder);
                TextBoxFolder.Text = "";

                FileManager_DataBind();
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
                lblSearchMessage.Text = ResourceManager.GetString("$file_found");
            }
            else
            {
                lblSearchMessage.Text = ResourceManager.GetString("$file_not_found");
            }

            lblSearchMessage.Visible = true;

            fileGrid.DataBind(); */
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
                    fileGrid.DataBind();
                    LoadFiles();
                    Notifier1.Message = ResourceManager.GetString("$file_successfully_overwritten");
                }
                else
                {
                    Notifier1.Warning = ResourceManager.GetString("$file_overwritte_failed");
                }
            }
        }*/

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

        protected void FileListODS_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["filterExtensions"] = filterExtensions;
        }

        public void fileGrid_ForceDataBind()
        {
            fileGrid.DataBind();
        }

        protected void fileGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count >= 2)
            {
                BOFile selectedFile = e.Row.DataItem as BOFile;

                if (selectedFile != null)
                {
                    Literal litSelectButton = e.Row.Cells[3].FindControl("litSelectButton") as Literal;
                    if (litSelectButton != null)
                    {
                        litSelectButton.Visible = ShowSelectLink;
                    }

                    Literal imageSymbol = e.Row.Cells[1].FindControl("imageSymbol") as Literal;

                    if (imageSymbol != null)
                        imageSymbol.Text = GenerateFileIcon(selectedFile, 50);

                    if (selectedFile.Id == FormatTool.GetInteger(TextBoxSearch.Text))
                    {
                        e.Row.CssClass += " selFile";// imageSymbol.Text += "<hr/>";
                    }
                }
            }
        }

        private BOFile RetrieveSubmittedFile()
        {
            BOFile file = null;

            if (fileUpload != null && fileUpload.HasFile && SelectedFolder != null && SelectedFolder.Id.HasValue)
            {
                BOCategory folder = fileB.GetFolder(SelectedFolder.Id.Value);

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
            object[] cSThis = new object[5];
            cSThis[0] = base.SaveControlState();
            cSThis[1] = showSelectLink;
            cSThis[2] = containerControlType;
            cSThis[3] = filterExtensions;
            cSThis[4] = expandTree;
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
            base.LoadControlState(cSBase);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in fileGrid.Rows)
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
                    cmdDelete.OnClientClick = @"return confirm('" + ResourceManager.GetString("$label_file_is_linked_confirm_delete") + @"');";

                if (fileB.Delete(i))
                {
                    deletedCount++;
                }*/
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} files", deletedCount);
                //RegularDataBind();
            }
        }

        protected void TreeViewFolders_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedFolder = fileB.GetFolder(Int32.Parse(TreeViewFolders.SelectedNode.Value.ToString()), true);
            FileManager_DataBind();
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