using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Utility;

namespace OneMainWeb.adm
{
    public partial class Editor : OneBasePage
    {
        protected static readonly BFileSystem fileSystemB = new BFileSystem();
        private const string TOP_LEVEL_DIR = "site_specific";

        protected string Dir
        {
            get { return ViewState["Dir"] != null ? ViewState["Dir"].ToString() : string.Empty; }
            set { ViewState["Dir"] = value; }
        }

        protected string EditableTypes
        {
            get { return "css;aspx;Master;html;htm;txt;tpl"; }
        }

        protected string SelectedFileName
        {
            get { return ViewState["SelectedFileName"] != null ? ViewState["SelectedFileName"].ToString() : string.Empty; }
            set { ViewState["SelectedFileName"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                Dir = Server.MapPath("~/" + TOP_LEVEL_DIR);
                

                TabularMultiView1.Views[0].Selectable = true;
                TabularMultiView1.Views[1].Selectable = false;
                TabularMultiView1.SetActiveIndex(0);
            }
        }

        private static List<string> GetEditableTypes(string accepted)
        {
            List<string> types = new List<string>();
            string[] typeStrings = accepted.Split(';');
            foreach (string t in typeStrings)
                types.Add(t);
            return types;
        }

        protected void DataGridFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "MoveInto":
                    {
                        Dir = e.CommandArgument.ToString();
                        uploadDiv.Visible = FolderRightsReader.IsWriteable(Dir);
                        DataGridFiles_DataBind();
                        break;
                    }
                case "EditFile":
                    {
                        SelectedFileName = e.CommandArgument.ToString();
                        TabularMultiView1.Views[1].Selectable = true;
                        TabularMultiView1.SetActiveIndex(1);
                        break;
                    }
            }
        }

        protected void DataGridFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                LinkButton cmdMoveInto = e.Row.FindControl("cmdMoveInto") as LinkButton;
                LinkButton cmdDelete = e.Row.FindControl("cmdDelete") as LinkButton;
                LinkButton cmdEdit = e.Row.FindControl("cmdEdit") as LinkButton;
                Literal imgLit = e.Row.FindControl("imgLit") as Literal;

                FileInfo fileInfo = e.Row.DataItem as FileInfo;
                DirectoryInfo dirInfo = e.Row.DataItem as DirectoryInfo;

                if (imgLit != null && cmdEdit != null && cmdDelete != null && cmdMoveInto != null && (fileInfo != null || dirInfo != null))
                {
                    if (dirInfo != null )
                    {
                        // this is a directory
                        if (e.Row.DataItemIndex == 0)
                        {
                            cmdEdit.Visible = cmdDelete.Visible = false;
                            if (dirInfo.Name != TOP_LEVEL_DIR)
                            {
                                cmdMoveInto.CommandArgument = dirInfo.Parent.FullName;
                                cmdMoveInto.Text = "...";
                                cmdMoveInto.CommandName = "MoveInto";
                            }
                        }
                        else
                        {
                            cmdDelete.Visible = dirInfo.GetDirectories().Length == 0 && dirInfo.GetFiles().Length == 0;
                            cmdEdit.Visible = false;
                            cmdMoveInto.Text = dirInfo.Name;
                            cmdMoveInto.CommandArgument = dirInfo.FullName;
                            cmdMoveInto.CommandName = "MoveInto";
                        }
                    }
                    else
                    {
                        imgLit.Text += OneHelper.GetFileIcon(this.Page, "OneMainWeb.Res.mime_icons.x16.", fileInfo.Extension.Trim('.').ToLower());

                        cmdDelete.Visible = false;
                        cmdEdit.Visible = GetEditableTypes(EditableTypes).Contains(fileInfo.Extension.Trim('.'));
                        cmdEdit.CommandArgument = fileInfo.FullName;
                        cmdMoveInto.Text = fileInfo.Name;
                    }
                    
                    // not sure if it's needed
                    cmdDelete.Visible = false;
                }
            }
        }

        protected void TabularMultiView1_OnViewIndexChanged(object sender, EventArgs e)
        {
            TabularMultiView1.Views[1].Selectable = TabularMultiView1.Views[1].Visible;

            if (((MultiView) sender).ActiveViewIndex == 0)
            {
                uploadDiv.Visible = FolderRightsReader.IsWriteable(Dir);
                DataGridFiles_DataBind();
            }
            else if (((MultiView) sender).ActiveViewIndex == 1)
            {
                if (FileRightsReader.IsWriteable(SelectedFileName))
                {
                    txtFileContent.Visible = true;
                    txtFileContent.Value = File.ReadAllText(SelectedFileName);
                    lblFilePath.Value = SelectedFileName;
                    DivSaveButtons.Visible = true;
                }
                else
                {
                    DivSaveButtons.Visible = false;
                    txtFileContent.Visible = false;
                    lblFilePath.Text = ResourceManager.GetString("$not_writable");
                }
            }
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            FileInfo selectedFile = new FileInfo(SelectedFileName);
            File.WriteAllText(selectedFile.FullName, txtFileContent.Value);
            Notifier1.Message = ResourceManager.GetString("$file_write_success");
        }

        protected void UpdateCloseButton_Click(object sender, EventArgs e)
        {
            FileInfo selectedFile = new FileInfo(SelectedFileName);
            File.WriteAllText(selectedFile.FullName, txtFileContent.Value);
            SelectedFileName = string.Empty;
            TabularMultiView1.Views[1].Selectable = false;
            TabularMultiView1.SetActiveIndex(0);
            Notifier1.Message = ResourceManager.GetString("$file_write_success");
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedFileName = string.Empty;
            TabularMultiView1.Views[1].Selectable = false;
            TabularMultiView1.SetActiveIndex(0);
        }

        private void DataGridFiles_DataBind()
        {
            DataGridFiles.DataSource = fileSystemB.ListPhysicalFolder(Dir, Server.MapPath("~/" + TOP_LEVEL_DIR));
            DataGridFiles.DataBind();
        }

        protected void cmdUpload_Click(object sender, EventArgs e)
        {
            if (FolderRightsReader.IsWriteable(Dir))
            {
                string fileName = System.IO.Path.GetFileName(fileUpload.PostedFile.FileName);
                string extension = Path.GetExtension(fileUpload.PostedFile.FileName);

                if (GetEditableTypes(EditableTypes + ";jpg;gif;png").Contains(extension.Trim('.')))
                {
                    try
                    {
                        fileUpload.PostedFile.SaveAs(Path.Combine(Dir, fileName));
                        Notifier1.Message = ResourceManager.GetString("$file_uploaded");
                    }
                    catch (Exception ex)
                    {
                        Notifier1.ExceptionMessage = ex.Message;
                    }

                    DataGridFiles_DataBind();
                }
                else
                {
                    Notifier1.Message = ResourceManager.GetString("$file_type_not_allowed");
                }
            }
            else
            {
                Notifier1.Message = ResourceManager.GetString("$directory_not_writable");                
            }
        }
    }
}
