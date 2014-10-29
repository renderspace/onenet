using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Collections.Generic;
using System.Threading;
using One.Net.BLL;
using OneMainWeb.AdminControls;


using One.Net.BLL.Utility;

namespace OneMainWeb.adm
{
    public partial class ContentEdit : OneBasePage
    {
        private static readonly BWebsite webSiteB = new BWebsite();
        private static readonly BTextContent textContentB = new BTextContent();


        public BOPage SelectedPage { get; set; }

        protected int? SelectedModuleInstanceId
        {
            get { return (Session["SelectedModuleInstanceId"] != null ? (int?)Int32.Parse(Session["SelectedModuleInstanceId"].ToString()) : (int?)null); }
            set { Session["SelectedModuleInstanceId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SelectedWebsite_ValidateDataBind();
            TreeView1_DataBind();
            
            if (!IsPostBack)
            {
                if (Request["instanceId"] != null)
                {
                    SelectedModuleInstanceId = FormatTool.GetInteger(Request["instanceId"]);
                }
                DropDownListModuleInstances_DataBind();
                TextContent_DataBind();
            }
        }

        protected void SelectedWebsite_ValidateDataBind()
        {
            // WEBSITE
            if (SelectedWebsite == null)
            {
                ResetAllControlsToDefault("You don't have permissions for any site or there are no websites defined in database.");
                return;
            }
            // ROOT PAGE
            RootNodeID = webSiteB.GetRootPageId(SelectedWebSiteId);
            if (!RootNodeID.HasValue)
            {
                ResetAllControlsToDefault("Website doesn't have a root page. ");
                Notifier1.Warning = "Website doesn't have a root page. ";
                LabelMessage.Text = "Website doesn't have a root page. ";
                MultiView1.ActiveViewIndex = 1;
                return;
            }
            // SELECTED PAGE
            if (SelectedPageId < 1)
                SelectedPageId = RootNodeID.Value;

            SelectedPage = webSiteB.GetPage(SelectedPageId);
            if (SelectedPage == null || SelectedPage.WebSiteId != SelectedWebSiteId || SelectedPage.LanguageId != Thread.CurrentThread.CurrentCulture.LCID)
            {
                ResetAllControlsToDefault("Please select a page on the left;");
                return;
            }
            if (!SelectedPage.IsEditabledByCurrentPrincipal)
            {
                ResetAllControlsToDefault("You do not have the rights to edit this page. Please select another page on the left.");
                return;
            }

            MultiView1.ActiveViewIndex = 0;
        }

        protected void ResetAllControlsToDefault(string message)
        {
            TreeView1.Nodes.Clear();
            TreeView1.DataBind();
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            MultiView1.ActiveViewIndex = 1;

            //LabelMessage
        }

        protected void TreeView1_Unload(object sender, EventArgs e)
        {
            // save the state of all nodes.
            TreeViewState.SaveTreeView(TreeView1, this.GetType().ToString());
        }

        private void TreeView1_DataBind()
        {
            int currentExpandLevel = 0;
            if (ExpandTree)
                currentExpandLevel = 10;

            if (TreeView1.Nodes.Count > 0)
                TreeView1.Nodes.Clear();

            TreeNode selectedNode = null;
            TreeNode tree = OneHelper.PopulateTreeViewControl(webSiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, currentExpandLevel, ref selectedNode);
            if (tree != null)
            {
                TreeView1.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeView1, this.GetType().ToString());

                if (selectedNode != null)
                {
                    TreeView1.FindNode(selectedNode.ValuePath).Selected = true;
                    SelectedPageId = Int32.Parse(selectedNode.Value);
                }
                else
                {
                    tree.Selected = true;
                    SelectedPageId = Int32.Parse(tree.Value);
                }
            }
        }

        private void TextContent_DataBind()
        {
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            PanelEditor.Visible = SelectedModuleInstanceId.HasValue;
            LiteralScript.Text = "";
            TextContentEditor.TeaserVisible = true;
            TextContentEditor.TitleVisible = true;
            TextContentEditor.SubTitleVisible = true;
            TextContentEditor.Html = TextContentEditor.Title = TextContentEditor.SubTitle = TextContentEditor.Teaser = "";
            TextContentEditor.Visible = false;
            LastChangeAndHistory1.Visible = false;

            if (SelectedModuleInstanceId.HasValue)
            {
                BOModuleInstance moduleInstanceModel = webSiteB.GetModuleInstance(SelectedModuleInstanceId.Value);

                if (moduleInstanceModel != null)
                {
                    BOInternalContent textContentModel = textContentB.GetTextContent(SelectedModuleInstanceId.Value);

                    if (textContentModel != null)
                    {
                        if (moduleInstanceModel.Name.Contains("SpecialContent"))
                        {
                            LiteralScript.Text = "<script>$(function () { var myCodeMirror = CodeMirror.fromTextArea(document.getElementById(\"TextBoxHtml\"), { lineNumbers: true, mode: \"htmlembedded\"  }); });</script>";
                            TextContentEditor.UseCkEditor = false;
                            TextContentEditor.TeaserVisible = false;
                            TextContentEditor.TitleVisible = false;
                            TextContentEditor.SubTitleVisible = false;
                        }
                        else if (moduleInstanceModel.Name.Contains("TextContent"))
                        {
                            TextContentEditor.UseCkEditor = true;
                        }
                        TextContentEditor.Visible = true;
                        TextContentEditor.Title = textContentModel.Title;
                        TextContentEditor.SubTitle = textContentModel.SubTitle;
                        TextContentEditor.Teaser = textContentModel.Teaser;
                        TextContentEditor.Html = textContentModel.Html;
                        LastChangeAndHistory1.Text = textContentModel.DisplayLastChanged;
                        if (textContentModel.ContentId.HasValue)
                        {
                            LastChangeAndHistory1.SelectedContentId = textContentModel.ContentId.Value;
                            LastChangeAndHistory1.SelectedLanguageId = textContentModel.LanguageId;
                            LastChangeAndHistory1.Visible = true;
                        }
                    }
                }

                cmdRevertToPublished.Visible = false;
                BOModuleInstance onlineInstance = webSiteB.GetModuleInstance(SelectedModuleInstanceId.Value, true);
                if (onlineInstance != null && moduleInstanceModel.Changed)
                {
                    cmdRevertToPublished.Visible = true;
                }
            }
        }

        protected void DropDownListModuleInstances_DataBind()
        {
            List<BOModuleInstance> moduleInstances = webSiteB.ListModuleInstances(SelectedPageId);
            List<BOModuleInstance> filteredModuleInstances = new List<BOModuleInstance>();

            foreach (BOModuleInstance instance in moduleInstances)
            {
                if ((instance.Name == "TextContent" || instance.Name == "SpecialContent") && !instance.IsInherited)
                    filteredModuleInstances.Add(instance);
            }

            ButtonChangeModuleInstance.Visible = DropDownListModuleInstances.Visible = filteredModuleInstances.Count > 1;

            LabelModuleInstanceName.Text = "";
            if (filteredModuleInstances.Count > 1)
            {
                DropDownListModuleInstances.DataSource = filteredModuleInstances;
                DropDownListModuleInstances.DataValueField = "Id";
                DropDownListModuleInstances.DataTextField = "ExpandedName";
                DropDownListModuleInstances.DataBind();
                if (SelectedModuleInstanceId.HasValue)
                {
                    DropDownListModuleInstances.SelectedValue = SelectedModuleInstanceId.Value.ToString();
                }
            }
            else if (filteredModuleInstances.Count == 1)
            {
                LabelModuleInstanceName.Text = filteredModuleInstances[0].ExpandedName;
            }

            if (filteredModuleInstances.Count > 0 && !SelectedModuleInstanceId.HasValue)
            {
                SelectedModuleInstanceId = filteredModuleInstances[0].Id;
            }
        }

        public void TreeView1_SelectedNodeChanged(object sender, EventArgs e)
        {

            SelectedPageId = FormatTool.GetInteger(TreeView1.SelectedNode.Value);
            TreeView1.CollapseAll();
            ExpandLoop(TreeView1.SelectedNode);
            SelectedModuleInstanceId = null;
            DropDownListModuleInstances_DataBind();
            SelectedWebsite_ValidateDataBind();
            TextContent_DataBind();
        }

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        protected void ButtonChangeModuleInstance_Click(object sender, EventArgs e)
        {
            SelectedModuleInstanceId = FormatTool.GetInteger(DropDownListModuleInstances.SelectedValue);
            TextContent_DataBind();
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            string error = "";
            if (SelectedModuleInstanceId.HasValue)
            {
                string html = TextContentEditor.Html;

                var ampersands = Validator.CheckForAmpersand(html);

                var errors = new List<ValidatorError>();

                Validator.CheckHtml(html, ref errors);

                var hasErrors = errors.Count > 0;
                var hasAmpersands = ampersands.Count > 0;

                if (EnableXHTMLValidator && (hasErrors || hasAmpersands))
                {
                    if (hasErrors)
                        Notifier1.Warning += "<h3>" + "Errors" + "</h3><ul>";

                    foreach (var validatorError in errors)
                    {
                        Notifier1.Warning += "<li>" + validatorError.Error;
                        if (!string.IsNullOrEmpty(validatorError.Tag))
                            Notifier1.Warning += "<span>" + validatorError.Tag + "</span>";
                        Notifier1.Warning += "</li>";
                    }

                    if (hasErrors)
                        Notifier1.Warning += "</ul>";

                    if (hasAmpersands)
                    {
                        Notifier1.Warning += "<h3>" + "Ampersands" + "</h3><ul>";
                        foreach (int i in ampersands)
                        {
                            Notifier1.Warning += "<li>" + "Position" + "<span>" + i + "</span></li>";
                        }
                        Notifier1.Warning += "</ul>";
                    }
                }
                else
                {
                    textContentB.ChangeTextContent(SelectedModuleInstanceId.Value, TextContentEditor.Title, TextContentEditor.SubTitle, TextContentEditor.Teaser, html);
                    Notifier1.Message = "Save sucessfull.";
                }
            }
            else
            {
                Notifier1.ExceptionName = "For some reasone we're lost and we didn't save your work.";
            }

            TreeView1_DataBind();
            if (string.IsNullOrEmpty(error))
                TextContent_DataBind();
        }


        protected void cmdRevertToPublished_Click(object sender, EventArgs e)
        {
            if (SelectedModuleInstanceId.HasValue)
            {
                BOInternalContent onlineContent = textContentB.GetTextContent(SelectedModuleInstanceId.Value, true);

                if (onlineContent != null)
                {
                    // TODO: after content is reverted to published version, the offline content should no longer be marked as changed. 
                    textContentB.ChangeTextContent(SelectedModuleInstanceId.Value, onlineContent.Title, onlineContent.SubTitle, onlineContent.Teaser, onlineContent.Html);
                    TreeView1_DataBind();
                    TextContent_DataBind();
                    Notifier1.Message = "Save sucessfull.";
                }
            }
        }
    }
}
