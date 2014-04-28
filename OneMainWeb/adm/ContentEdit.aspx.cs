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
using TwoControlsLibrary;

using One.Net.BLL.Utility;

namespace OneMainWeb.adm
{
    public partial class ContentEdit : OneBasePage
    {
        private static readonly BWebsite webSiteB = new BWebsite();
        private static readonly BTextContent textContentB = new BTextContent();

        protected int? SelectedModuleInstanceId
        {
            get { return (Session["SelectedModuleInstanceId"] != null ? (int?)Int32.Parse(Session["SelectedModuleInstanceId"].ToString()) : (int?)null); }
            set { Session["SelectedModuleInstanceId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            TreeView1_DataBind();

            if (!IsPostBack)
            {
                chkUseFck.Checked = UseCkEditor;
                TextContentEditor.UseCkEditor = this.UseCkEditor;

                if (Request["instanceId"] != null)
                {
                    int instanceId = FormatTool.GetInteger(Request["instanceId"]);
                    BOModuleInstance requestInstance = webSiteB.GetModuleInstance(instanceId, false);

                    if (requestInstance != null && requestInstance.Name == "TextContent")
                    {
                        SelectedModuleInstanceId = instanceId;
                    }
                }

                HistoryControl.GetContent = textContentB.GetTextContent;
                HistoryControl.Img1Src = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.extend-down.gif");
                HistoryControl.Img2Src = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.extend-up.gif");

                if ( SelectedModuleInstanceId.HasValue )
                    HistoryControl.SelectedItemId = SelectedModuleInstanceId.Value;

                DropDownListModuleInstances_DataBind();
                InitializeControls();
            }
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

            TreeNode tree = OneHelper.PopulateTreeViewControl(webSiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, Page, currentExpandLevel);
            if (tree != null)
            {
                TreeView1.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeView1, this.GetType().ToString());

                if (SelectedPageId == -1)
                {
                    tree.Selected = true;
                    SelectedPageId = Int32.Parse(tree.Value);
                }
            }
        }

        protected void HistoryControl_RevertToAudit(object sender, TypedEventArg<BOInternalContent> e)
        {
            TextContentEditor.Title = e.Value.Title;
            TextContentEditor.SubTitle = e.Value.SubTitle;
            TextContentEditor.Teaser = e.Value.Teaser;
            TextContentEditor.Html = e.Value.Html;
        }

        private void InitializeControls()
        {
            rightSettings.Visible = SelectedModuleInstanceId.HasValue;
            // MainTextBox.TextBoxCssClass = chkUseFck.Checked ? "ckeditor" : "";

            HistoryControl.Visible = false;

            if (SelectedModuleInstanceId.HasValue)
            {
                BOModuleInstance moduleInstanceModel = webSiteB.GetModuleInstance(SelectedModuleInstanceId.Value);

                if (moduleInstanceModel != null)
                {
                    BOInternalContent textContentModel = textContentB.GetTextContent(SelectedModuleInstanceId.Value);

                    if (textContentModel != null && 
                        textContentModel.ContentId.HasValue && 
                        textContentModel.ContentId.Value > 0)
                    {
                        TextContentEditor.Title = textContentModel.Title;
                        TextContentEditor.SubTitle = textContentModel.SubTitle;
                        TextContentEditor.Teaser = textContentModel.Teaser;
                        TextContentEditor.Html = textContentModel.Html;
                        InfoLabelLastChange.Value = textContentModel.LastChangedBy;

                        /*
                        if (textContentModel.IsRated)
                        {
                            InfoLabelVotes.Visible = InfoLabelScore.Visible = true;
                            InfoLabelScore.Value = textContentModel.Score.Value.ToString();
                            InfoLabelVotes.Value = textContentModel.Votes.ToString();
                        }
                        else
                            InfoLabelVotes.Visible = InfoLabelScore.Visible = false;
                        */
                        HistoryControl.Visible = true;
                        if (SelectedModuleInstanceId.HasValue)
                            HistoryControl.SelectedItemId = SelectedModuleInstanceId.Value;
                        HistoryControl.LoadHistory();
                    }
                    else
                    {
                        InfoLabelLastChange.Text = TextContentEditor.Html = TextContentEditor.Title = TextContentEditor.SubTitle = TextContentEditor.Teaser = string.Empty;
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

        protected void chkUseFck_CheckedChanged(object sender, EventArgs e)
        {
            UseCkEditor = chkUseFck.Checked;
        }

        protected override void OnPreRender(EventArgs e)
        {
            TextContentEditor.TextBoxCssClass = UseCkEditor ? "ckeditor" : "";
            base.OnPreRender(e);
        }

        protected void DropDownListModuleInstances_DataBind()
        {
            List<BOModuleInstance> moduleInstances = webSiteB.ListModuleInstances(SelectedPageId);
            List<BOModuleInstance> filteredModuleInstances = new List<BOModuleInstance>();

            foreach (BOModuleInstance instance in moduleInstances)
            {
                if (instance.Name == "TextContent" && !instance.IsInherited)
                    filteredModuleInstances.Add(instance);
            }

            ButtonChangeModuleInstance.Visible = DropDownListModuleInstances.Visible = filteredModuleInstances.Count > 1;

            LabelModuleInstanceName.Text = string.Empty;
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

            if (filteredModuleInstances.Count > 0 && !SelectedModuleInstanceId.HasValue )
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
            InitializeControls();
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
            InitializeControls();
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
                        Notifier1.Warning += "<h3>" + ResourceManager.GetString("$errors") + "</h3><ul>";

                    foreach (var validatorError in errors)
                    {
                        Notifier1.Warning += "<li>" + ResourceManager.GetString("$" + validatorError.Error);
                        if (!string.IsNullOrEmpty(validatorError.Tag))
                            Notifier1.Warning += "<span>" + validatorError.Tag + "</span>";
                        Notifier1.Warning += "</li>";
                    }

                    if (hasErrors)
                        Notifier1.Warning += "</ul>";

                    if (hasAmpersands)
                    {
                        Notifier1.Warning += "<h3>" + ResourceManager.GetString("$ampersands") + "</h3><ul>";
                        foreach (int i in ampersands)
                        {
                            Notifier1.Warning += "<li>" + ResourceManager.GetString("$position") + "<span>" + i + "</span></li>";
                        }
                        Notifier1.Warning += "</ul>";
                    }
                }
                else
                {
                    textContentB.ChangeTextContent(SelectedModuleInstanceId.Value, TextContentEditor.Title, TextContentEditor.SubTitle, TextContentEditor.Teaser, html);
                    Notifier1.Message = ResourceManager.GetString("$save_sucessfull");
                }
            }
            else
            {
                Notifier1.ExceptionName = ResourceManager.GetString("$because_of_inactivity_selected_module_was_not_saved");
            }
            
            TreeView1_DataBind();
            if (string.IsNullOrEmpty(error))
                InitializeControls();
        }

        /*
        protected void InputWithButtonVote_Click(object sender, EventArgs e)
        {
            if (SelectedModuleInstanceId.HasValue)
            {
                int score = FormatTool.GetInteger(InputWithButtonVote.Value);
                textContentB.Vote(score, SelectedModuleInstanceId.Value);
            }
        }
        */

        protected void cmdRevertToPublished_Click(object sender, EventArgs e)
        {
            if (SelectedModuleInstanceId.HasValue)
            {
                BOInternalContent onlineContent = textContentB.GetTextContent(SelectedModuleInstanceId.Value, true);

                if (onlineContent != null )
                {
                    // TODO: after content is reverted to published version, the offline content should no longer be marked as changed. 
                    textContentB.ChangeTextContent(SelectedModuleInstanceId.Value, onlineContent.Title, onlineContent.SubTitle, onlineContent.Teaser, onlineContent.Html);
                    TreeView1_DataBind();
                    InitializeControls();
                    Notifier1.Message = ResourceManager.GetString("$save_sucessfull");
                }
            }
        }
    }
}
