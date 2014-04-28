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

namespace OneMainWeb.adm
{
    public partial class SpecialContentEdit : OneBasePage
    {
        private static readonly BWebsite webSiteB = new BWebsite();
        private static readonly BTextContent specialContentB = new BTextContent();

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
                if (Request["instanceId"] != null)
                {
                    int instanceId = FormatTool.GetInteger(Request["instanceId"]);
                    BOModuleInstance requestInstance = webSiteB.GetModuleInstance(instanceId, false);

                    if (requestInstance != null && requestInstance.Name == "SpecialContent")
                        SelectedModuleInstanceId = instanceId;
                }

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

        private void InitializeControls()
        {
            rightSettings.Visible = SelectedModuleInstanceId.HasValue;

            if (SelectedModuleInstanceId.HasValue)
            {
                BOModuleInstance moduleInstanceModel = webSiteB.GetModuleInstance(SelectedModuleInstanceId.Value);

                if (moduleInstanceModel != null)
                {
                    BOInternalContent specialContent = specialContentB.GetTextContent(SelectedModuleInstanceId.Value);

                    if (specialContent != null &&
                        specialContent.ContentId.HasValue &&
                        specialContent.ContentId.Value > 0)
                    {
                        MainTextBox.Value = specialContent.Html;
                        InfoLabelLastChange.Value = specialContent.LastChangedBy;
                    }
                    else
                    {
                        InfoLabelLastChange.Text = MainTextBox.Value = string.Empty;
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
                if (instance.Name == "SpecialContent" && !instance.IsInherited)
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
                    DropDownListModuleInstances.SelectedValue = SelectedModuleInstanceId.Value.ToString();
            }
            else if (filteredModuleInstances.Count == 1)
                LabelModuleInstanceName.Text = filteredModuleInstances[0].ExpandedName;

            if (filteredModuleInstances.Count > 0 && !SelectedModuleInstanceId.HasValue)
                SelectedModuleInstanceId = filteredModuleInstances[0].Id;
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
            if (SelectedModuleInstanceId.HasValue)
            {
                string html = MainTextBox.Value;
                specialContentB.ChangeTextContent(SelectedModuleInstanceId.Value, string.Empty, string.Empty, string.Empty, html);
                Notifier1.Message = ResourceManager.GetString("$save_sucessfull");
            }
            else
                Notifier1.ExceptionName = ResourceManager.GetString("$because_of_inactivity_selected_module_was_not_saved");

            TreeView1_DataBind();
            InitializeControls();
        }

        protected void cmdRevertToPublished_Click(object sender, EventArgs e)
        {
            if (SelectedModuleInstanceId.HasValue)
            {
                BOInternalContent onlineContent = specialContentB.GetTextContent(SelectedModuleInstanceId.Value, true);

                if (onlineContent != null)
                {
                    // TODO: after content is reverted to published version, the offline content should no longer be marked as changed. 
                    specialContentB.ChangeTextContent(SelectedModuleInstanceId.Value, string.Empty, string.Empty, string.Empty, onlineContent.Html);
                    TreeView1_DataBind();
                    InitializeControls();
                    Notifier1.Message = ResourceManager.GetString("$save_sucessfull");
                }
            }
        }
    }
}
