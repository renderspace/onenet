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
        /*

          
          
          
            TextSpecialContent1.IsSpecialContent = false;*/

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
                    int instanceId = FormatTool.GetInteger(Request["instanceId"]);
                    BOModuleInstance requestInstance = webSiteB.GetModuleInstance(instanceId, false);

                    if (requestInstance != null && requestInstance.Name.Contains("SpecialContent"))
                    {
                        SelectedModuleInstanceId = instanceId;
                        TextContentEditor.UseCkEditor = false;
                    }
                    else if (requestInstance != null && requestInstance.Name.Contains("TextContent"))
                    {
                        SelectedModuleInstanceId = instanceId;
                        TextContentEditor.UseCkEditor = true;
                    }

                }
                DropDownListModuleInstances_DataBind();
                InitializeControls();
            }
        }

        protected void SelectedWebsite_ValidateDataBind()
        {
            BOWebSite TempWebSite = webSiteB.Get(SelectedWebSiteId);
            if (SelectedWebSiteId < 1 || TempWebSite == null)
            {
                // if session is lost, we need to select a default website and delete the possibly incorrect SelectedPageId
                SelectedWebSiteId = int.Parse(ConfigurationManager.AppSettings["WebSiteId"].ToString());
                SelectedPageId = -1;
                if (TempWebSite == null)
                {
                    TempWebSite = webSiteB.Get(SelectedWebSiteId);
                }
            }

            if (TempWebSite == null)
            {
                ResetAllControlsToDefault("No website selected or no website in database");
                return;
            }

            Thread.CurrentThread.CurrentCulture = TempWebSite.Culture;

            var RootNodeID = webSiteB.GetRootPageId(SelectedWebSiteId);

            if (!RootNodeID.HasValue)
            {
                ResetAllControlsToDefault("Website doesn't have a root page. Use structure menu.");
                return;
            }

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
                ResetAllControlsToDefault("You do not have the rights to edit this page. Please select a page on the left.");
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

        private void InitializeControls()
        {
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            PanelEditor.Visible = SelectedModuleInstanceId.HasValue;
            // MainTextBox.TextBoxCssClass = chkUseFck.Checked ? "ckeditor" : "";

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

                        if (moduleInstanceModel.Name.Contains("SpecialContent"))
                        {
                            TextContentEditor.UseCkEditor = false;
                        }
                        else if (moduleInstanceModel.Name.Contains("TextContent"))
                        {
                            TextContentEditor.UseCkEditor = true;
                        }
                        

                        TextContentEditor.Title = textContentModel.Title;
                        TextContentEditor.SubTitle = textContentModel.SubTitle;
                        TextContentEditor.Teaser = textContentModel.Teaser;
                        TextContentEditor.Html = textContentModel.Html;
                        LastChangeAndHistory1.Text = textContentModel.DisplayLastChanged;
                        LastChangeAndHistory1.SelectedContentId = textContentModel.ContentId.Value;
                        LastChangeAndHistory1.SelectedLanguageId = textContentModel.LanguageId;
                    }
                    else
                    {
                        TextContentEditor.Html = TextContentEditor.Title = TextContentEditor.SubTitle = TextContentEditor.Teaser = "";
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
                if (instance.Name == "TextContent" && !instance.IsInherited)
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
                        Notifier1.Warning += "<h3>" + "$errors" + "</h3><ul>";

                    foreach (var validatorError in errors)
                    {
                        Notifier1.Warning += "<li>" + "$" + validatorError.Error;
                        if (!string.IsNullOrEmpty(validatorError.Tag))
                            Notifier1.Warning += "<span>" + validatorError.Tag + "</span>";
                        Notifier1.Warning += "</li>";
                    }

                    if (hasErrors)
                        Notifier1.Warning += "</ul>";

                    if (hasAmpersands)
                    {
                        Notifier1.Warning += "<h3>" + "$ampersands" + "</h3><ul>";
                        foreach (int i in ampersands)
                        {
                            Notifier1.Warning += "<li>" + "$position" + "<span>" + i + "</span></li>";
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
                Notifier1.ExceptionName = "$because_of_inactivity_selected_module_was_not_saved";
            }

            TreeView1_DataBind();
            if (string.IsNullOrEmpty(error))
                InitializeControls();
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
                    InitializeControls();
                    Notifier1.Message = "Save sucessfull.";
                }
            }
        }
    }
}
