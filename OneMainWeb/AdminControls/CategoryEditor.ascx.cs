using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Reflection;

using One.Net.BLL;


namespace OneMainWeb.AdminControls
{
    public partial class CategoryEditor : CategorizationUserControl
    {
        #region Exposed events

        public event EventHandler CategoryMoved;

        protected void OnCategoryMoved(EventArgs e)
        {
            if (CategoryMoved != null)
            {
                CategoryMoved(this, e);
            }
        }

        public event EventHandler CategoryFailedToMove;

        protected void OnCategoryFailedToMove(EventArgs e)
        {
            if ( CategoryFailedToMove != null)
            {
                CategoryFailedToMove(this, e);
            }
        }

        // Expose category deleted event
        public event EventHandler CategoryDeleted;

        protected void OnCategoryDeleted(EventArgs e)
        {
            if (CategoryDeleted != null)
            {
                CategoryDeleted(this, e);
            }
        }

        // Expose category update event
        public event EventHandler CategoryUpdated;

        protected void OnCategoryUpdated(EventArgs e)
        {
            if (CategoryUpdated != null)
            {
                CategoryUpdated(this, e);
            }
        }

        public void HideMovePanel()
        {
            moveCategoryPanel.Visible = false;              
        }

        protected void cmdUpdateNode_Click(object sender, EventArgs e)
        {
            BOCategory category = GetCategory(SelectedCategory.Id.Value, ShowUntranslated);
            category.Title = txtCategory.Title;
            category.SubTitle = txtCategory.SubTitle;
            category.Teaser = txtCategory.Teaser;
            category.Html = txtCategory.Html;
            category.IsSelectable = chkIsSelectable.Checked;
            category.IsPrivate = chkIsPrivate.Checked;
            category.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            ChangeCategory(category);
            OnCategoryUpdated(e);
        }

        protected void cmdDeleteNode_Click(object sender, EventArgs e)
        {
            DeleteCategory(SelectedCategory.Id.Value);
            OnCategoryDeleted(e);
        }

        #endregion Exposed events

        #region Public methods

        public void LoadControls()
        {
            if (SelectedCategory != null && SelectedCategory.Id.HasValue)
            {
                txtCategory.UseCkEditor = this.UseCkEditor;
                txtCategory.HtmlVisible = this.HtmlVisible;
                txtCategory.TitleVisible = this.TitleVisible;
                txtCategory.SubTitleVisible = this.SubTitleVisible;
                txtCategory.TeaserVisible = this.TeaserVisible;
                chkIsSelectable.Visible = this.ShowIsSelectable;
                chkIsPrivate.Visible = this.ShowIsPrivate;

                this.Visible = true;

                cmdDeleteNode.Visible = false;
                assignedGrid.Visible = ShowCategorizedItemsGrid;

                BOCategory category = GetCategory(SelectedCategory.Id.Value, ShowUntranslated);
                if (category != null)
                {
                    txtCategory.Title = category.Title;
                    txtCategory.Teaser = category.Teaser;
                    txtCategory.Html = category.Html;
                    txtCategory.SubTitle = category.SubTitle;
                    chkIsSelectable.Checked = category.IsSelectable;
                    chkIsPrivate.Checked = category.IsPrivate;
                    InfoLabelID.Value = category.Id.Value.ToString();

                    if (ListCategorizedItems != null)
                    {
                        List<ICategorizable> items = ListCategorizedItems(category.Id.Value);
                        cmdDeleteNode.Visible = category.ChildCount == 0 && items.Count == 0;

                        cmdDeleteNode.Enabled = EnableDelete;

                        if (ShowCategorizedItemsGrid)
                        {
                            assignedGrid.DataSource = ListCategorizedItems(category.Id.Value);
                            assignedGrid.DataBind();
                        }
                    }
                }

                TreeNodeCollection treeNodes = null;

                if (ListCategories != null)
                {
                    treeNodes = TreeCategorization.GenerateTreeNodes(ListCategories());
                }
                else if (ListCategoriesWithFilter != null)
                {
                    treeNodes = TreeCategorization.GenerateTreeNodes(ListCategoriesWithFilter(CategoryFilter, ShowUntranslated));
                }

                if (treeNodes != null && SelectedCategory.ParentId.HasValue)
                {
                    cmdMoveCategory.Visible = true;
                    moveCategoryPanel.Title = ResourceManager.GetString("$move_category");
                    lblMoveCategory.Value = SelectedCategory.Title;
                    moveCategoryTree.Nodes.Clear();
                    moveCategoryTree.Nodes.Add(treeNodes[0]);
                    moveCategoryTree.ExpandAll();
                }
            }
            else
            {
                this.Visible = false;
            }
        }

        public void moveCategoryTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            int newParentId = FormatTool.GetInteger(moveCategoryTree.SelectedValue);
            BOCategory newCategory = GetCategory(newParentId, ShowUntranslated);

            // We have to make sure we do not allow moving of parent category to child category
            // since this would create a loop
            TreeNode movingNode = moveCategoryTree.FindNode(BuildPath(SelectedCategory));
            TreeNode destinationNode = moveCategoryTree.FindNode(BuildPath(newCategory));

            // don't allow moving against itself, and dont allow moving of a node to it's descendent
            if (destinationNode.Value != movingNode.Value && !IsDescendantOf(destinationNode, movingNode))
            {
                this.MoveCategory(SelectedCategory, newCategory);
                OnCategoryMoved(e);
            }
            else
            {
                OnCategoryFailedToMove(e);
            }

            moveCategoryPanel.Visible = false;
        }

        protected void cmdMoveCategory_Click(object sender, EventArgs e)
        {
            moveCategoryPanel.Visible = true;            
        }

        protected void moveCategoryPanel_WindowClosed(object sender, EventArgs e)
        {
            moveCategoryPanel.Visible = false;            
        }

        #endregion Public methods
    }
}