using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.adm
{
    public partial class StructureDelete : OneBasePage
    {
        BWebsite websiteB = new BWebsite();

        private List<BOWebSite> Websites
        {
            get { return ViewState["Websites"] != null ? ViewState["Websites"] as List<BOWebSite> : null; }
            set { ViewState["Websites"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TreeView2_DataBind();
            }
        }

        protected void TreeView2_Unload(object sender, EventArgs e)
        {
            // save the state of all nodes.
            TreeViewState.SaveTreeView(TreeView2, this.GetType().ToString());
        }

        public void TreeView2_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedPageId = FormatTool.GetInteger(TreeView2.SelectedNode.Value);
            LabelSelectedPageId.Text = SelectedPageId.ToString();
            LabelSelectedPageName.Text = TreeView2.SelectedNode.Text;
            TreeView2.CollapseAll();
            ExpandLoop(TreeView2.SelectedNode);
        }

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        private void TreeView2_DataBind()
        {
            int currentExpandLevel = 0;
            if (ExpandTree)
                currentExpandLevel = 10;

            if (TreeView2.Nodes.Count > 0)
                TreeView2.Nodes.Clear();

            TreeNode selectedNode = null;
            TreeNode tree = OneHelper.PopulateTreeViewControl(websiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, currentExpandLevel, ref selectedNode);
            if (tree != null)
            {
                TreeView2.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeView2, this.GetType().ToString());

                if (selectedNode != null)
                {
                    TreeView2.FindNode(selectedNode.ValuePath).Selected = true;
                    SelectedPageId = Int32.Parse(selectedNode.Value);
                    LabelSelectedPageId.Text = selectedNode.Value;
                    LabelSelectedPageName.Text = selectedNode.Text;
                }
                else
                {
                    tree.Selected = true;
                    SelectedPageId = Int32.Parse(tree.Value);
                    LabelSelectedPageId.Text = tree.Value;
                    LabelSelectedPageName.Text = tree.Text;
                }
            }
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            var mcount = 0;
            var dcount = 0;
            websiteB.DeleteTree(SelectedPageId, ref mcount, ref dcount);
            notifier.Message = mcount + "$pages_marked_for_deletion";
            notifier.Message += "<br />" + dcount + "$pages_permanently_deleted";
            LabelSelectedPageId.Text = "";
            LabelSelectedPageName.Text = "";
            TreeView2_DataBind();
        }

        protected void ButtonDelete_PreRender(object sender, EventArgs e)
        {
            ButtonDelete.OnClientClick = "return confirm('" + "$confirm" + "')";
        }
    }
}