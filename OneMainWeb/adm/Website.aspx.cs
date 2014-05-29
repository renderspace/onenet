using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using OneMainWeb.AdminControls;


using One.Net.BLL.Utility;

namespace OneMainWeb.adm
{
    public partial class Website : OneBasePage
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
                MultiView1.ActiveViewIndex = 0;
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
            InfoLabelPageId.Value = SelectedPageId.ToString();
            InfoLabelPageName.Value = TreeView2.SelectedNode.Text;
            TreeView2.CollapseAll();
            ExpandLoop(TreeView2.SelectedNode);
        }

        private void TreeView2_DataBind()
        {
            int currentExpandLevel = 0;
            if (ExpandTree)
                currentExpandLevel = 10;

            if (TreeView2.Nodes.Count > 0)
                TreeView2.Nodes.Clear();

            TreeNode tree = OneHelper.PopulateTreeViewControl(websiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, Page, currentExpandLevel);
            if (tree != null)
            {
                TreeView2.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeView2, this.GetType().ToString());

                if (SelectedPageId == -1)
                {
                    tree.Selected = true;
                    SelectedPageId = Int32.Parse(tree.Value);
                    InfoLabelPageId.Value = tree.Value;
                    InfoLabelPageName.Value = tree.Text;
                }
            }
        }

        protected void TreeView1_Unload(object sender, EventArgs e)
        {
            // save the state of all nodes.
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

            TreeNode tree = OneHelper.PopulateTreeViewControl(websiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, Page, currentExpandLevel);
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

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        protected void TabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            MultiView multiView = sender as MultiView;
            if (multiView.ActiveViewIndex == 0)
            {
                GridViewWebsitesLoad(true);
                if (DropDownList1.Items.Count == 0)
                {
                    var languages = websiteB.ListLanguages();
                    foreach (var language in languages)
                        DropDownList1.Items.Add(new ListItem(language.ToString(), language.ToString()));
                }
            }
            else if (multiView.ActiveViewIndex == 1)
            {
                LoadCopyControls();
            }
            else if (multiView.ActiveViewIndex == 2)
            {
                TreeView2_DataBind();
            }
        }

        private void LoadCopyControls()
        {
            TreeView1_DataBind();

            Websites = websiteB.List();
            var emptyList = new List<BOWebSite>();
            foreach (var website in Websites)
            {
                if (!website.RootPageId.HasValue)
                    emptyList.Add(website);
            }
            CheckBoxListEmptyWebSites.DataSource = emptyList;
            CheckBoxListEmptyWebSites.DataTextField = "Title";
            CheckBoxListEmptyWebSites.DataValueField = "Id";
            CheckBoxListEmptyWebSites.DataBind();
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            var mcount = 0;
            var dcount = 0;
            websiteB.DeleteTree(SelectedPageId, ref mcount, ref dcount);
            notifier.Message = mcount + ResourceManager.GetString("$pages_marked_for_deletion");
            notifier.Message += "<br />" + dcount + ResourceManager.GetString("$pages_permanently_deleted");
            InfoLabelPageId.Value = "";
            InfoLabelPageName.Value = "";
            TreeView2_DataBind();
        }

        protected void ButtonDelete_PreRender(object sender, EventArgs e)
        {
            ButtonDelete.OnClientClick = "return confirm('" + ResourceManager.GetString("$confirm") + "')";
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

            if (SelectedPageId > 0 && siteIds.Count > 0 )
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

        private void GridViewWebsitesLoad(bool reloadData)
        {
            if (Websites == null || reloadData)
                Websites = websiteB.List();
            GridViewWebsites.DataSource = Websites;
            GridViewWebsites.DataBind();
        }

        protected void GridViewWebsites_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridViewWebsites.EditIndex = e.NewEditIndex;
            GridViewWebsitesLoad(false);
        }

        protected void GridViewWebsites_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridViewWebsites.EditIndex = -1;
            GridViewWebsitesLoad(false);
        }

        protected void GridViewWebsites_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = GridViewWebsites.Rows[e.RowIndex];
            BOWebSite website = null;
            if (Websites != null)
                website = Websites[row.DataItemIndex];

            var titleBox = row.Cells[1].Controls[0] as TextBox;

            if (website != null && titleBox != null)
            {
                var title = titleBox.Text;
                var id = website.Id;

                website = websiteB.Get(id);
                if (website != null)
                {
                    website.Title = title;
                    websiteB.ChangeWebsite(website);
                    GridViewWebsites.EditIndex = -1;
                    GridViewWebsitesLoad(true);
                }
            }
        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            var title = InputTitle.Value;
            var lcid = Int32.Parse(DropDownList1.SelectedValue);

            var website = new BOWebSite();
            website.Title = title;
            website.SubTitle = "";
            website.Teaser = "";
            website.Html = "";
            website.LanguageId = lcid;
            website.ContentId = null;
            website.PrincipalCreated = User.Identity.Name;
            websiteB.ChangeWebsite(website);
            GridViewWebsitesLoad(true);
        }

        protected void LinkButtonWebsites_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        protected void LinkButtonCopy_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 1;
        }

        protected void LinkButtonRecursive_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 2;
        }
    }
}