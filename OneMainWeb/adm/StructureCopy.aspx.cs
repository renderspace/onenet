using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.adm
{
    public partial class StructureCopy : OneBasePage
    {
        BWebsite websiteB = new BWebsite();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCopyControls();
            }
        }

        protected void TreeView1_Unload(object sender, EventArgs e)
        {
            TreeViewState.SaveTreeView(TreeView1, this.GetType().ToString());
        }

        public void TreeView1_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedPageId = FormatTool.GetInteger(TreeView1.SelectedNode.Value);
            TreeView1.CollapseAll();
            ExpandLoop(TreeView1.SelectedNode);
            LoadCopyControls();
        }

        private void TreeView1_DataBind()
        {
            int currentExpandLevel = 0;
            if (ExpandTree)
                currentExpandLevel = 10;

            if (TreeView1.Nodes.Count > 0)
                TreeView1.Nodes.Clear();

            TreeNode selectedNode = null;
            TreeNode tree = OneHelper.PopulateTreeViewControl(websiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, currentExpandLevel, ref selectedNode);
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

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        private void LoadCopyControls()
        {
            TreeView1_DataBind();

            var websites = Authorization.ListAllowedWebsites();
            var emptyList = new List<BOWebSite>();
            foreach (var website in websites)
            {
                if (!website.RootPageId.HasValue)
                    emptyList.Add(website);
            }
            CheckBoxListEmptyWebSites.DataSource = emptyList;
            CheckBoxListEmptyWebSites.DataTextField = "DisplayName";
            CheckBoxListEmptyWebSites.DataValueField = "Id";
            CheckBoxListEmptyWebSites.DataBind();
        }

        protected void ButtonCopy_Click(object sender, EventArgs e)
        {
            var siteIds = new List<int>();

            foreach (ListItem item in CheckBoxListEmptyWebSites.Items)
            {
                if (item.Selected)
                {
                    var siteId = Int32.Parse(item.Value);
                    if (siteId > 0)
                        siteIds.Add(siteId);
                }
            }

            if (SelectedPageId > 0 && siteIds.Count > 0)
            {
                foreach (var siteId in siteIds)
                {
                    var site = websiteB.Get(siteId);
                    if (!site.RootPageId.HasValue)
                    {
                        websiteB.CopyToWebsite(siteId, SelectedPageId, SelectedWebSiteId);
                    }
                }
                LoadCopyControls();

                notifier.Message = "$copy_completed";
            }
        }
    }
}