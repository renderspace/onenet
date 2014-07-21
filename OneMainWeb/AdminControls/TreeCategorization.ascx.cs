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
    public partial class TreeCategorization : CategorizationUserControl
    {
        #region Exposed events

        public event EventHandler AdaptedSelectedNodeChanged;

        public void OnAdaptedSelectedNodeChanged(EventArgs e)
        {
            if (AdaptedSelectedNodeChanged != null)
            {
                AdaptedSelectedNodeChanged(this, e);
            }
        }

        public event EventHandler SelectedNodeChanged;

        public void OnSelectedNodeChanged(EventArgs e)
        {
            if (SelectedNodeChanged != null)
            {
                SelectedNodeChanged(this, e);
            }
        }

        public void categoriesTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedCategory = GetCategory(Int32.Parse(categoriesTree.SelectedNode.Value.ToString()), ShowUntranslated);
            OnSelectedNodeChanged(e);

        }

        public event EventHandler NodeAdded;

        protected void OnNodeAdded(EventArgs e)
        {
            if (NodeAdded != null)
            {
                NodeAdded(this, e);
            }
        }

        protected void CmdAddTreeNode_Click(object sender, EventArgs e)
        {
            if (InputWithButtonAddTreeNode.Value.Length > 0)
            {
                BOCategory folder = new BOCategory();

                folder.Type = CategoryType;
                folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                folder.Title = InputWithButtonAddTreeNode.Value;
                folder.Teaser = folder.SubTitle = folder.Html = "";
                folder.IsSelectable = true;
                folder.IsPrivate = false;
                folder.ChildCount = 0;

                if (SelectedCategory == null || !SelectedCategory.Id.HasValue)
                {
                    folder.ParentId = null;
                }
                else
                {
                    folder.ParentId = SelectedCategory.Id.Value;
                }

                ChangeCategory(folder);
                InputWithButtonAddTreeNode.Value = "";

                LoadControls();

                OnNodeAdded(e);
            }
        }

        #endregion Exposed events

        #region Public methods

        public TreeNode FindNode(BOCategory category)
        {
            return categoriesTree.FindNode(BuildPath(category));
        }

        public void SelectNode(BOCategory category)
        {
            TreeNode node = FindNode(category);

            if (node != null)
            {
                node.Select();
            }
            else if (categoriesTree.Nodes.Count > 0)
            {
                categoriesTree.Nodes[0].Select();
            }

            if (categoriesTree.SelectedNode != null)
            {
                SelectedCategory = GetCategory(Int32.Parse(categoriesTree.SelectedNode.Value.ToString()), ShowUntranslated);
            }
            else
            {
                SelectedCategory = null;
            }
        }

        public void SelectParent()
        {
            BOCategory parentCategory = null;
            if (this.SelectedCategory != null && this.SelectedCategory.ParentId.HasValue)
            {
                parentCategory = GetCategory(this.SelectedCategory.ParentId.Value, ShowUntranslated);
            }

            TreeNode parentNode = null;
            if (parentCategory != null)
            {
                parentNode = FindNode(parentCategory);
            }

            if (parentNode != null)
            {
                parentNode.Select();
            }
            else if (categoriesTree.Nodes.Count > 0)
            {
                categoriesTree.Nodes[0].Select();
            }

            if (categoriesTree.SelectedNode != null)
            {
                SelectedCategory = GetCategory(Int32.Parse(categoriesTree.SelectedNode.Value.ToString()), ShowUntranslated);
            }
            else
            {
                SelectedCategory = null;
            }
        }

        public void TreeDataBind()
        {
            categoriesTree.DataBind();
        }

        public void LoadControls()
        {
            TreeNodeCollection treeNodes = null;

            if (ListCategories != null)
            {
                treeNodes = GenerateTreeNodes(ListCategories());
            }
            else
            {
                treeNodes = GenerateTreeNodes(ListCategoriesWithFilter(CategoryFilter, ShowUntranslated));
            }

            categoriesTree.Nodes.Clear();
            NodeCount = treeNodes.Count;

            if (treeNodes.Count > 0)
            {
                categoriesTree.Nodes.Add(treeNodes[0]);

                int currentExpandLevel = 1;
                if (ExpandTree)
                {
                    currentExpandLevel = 10;
                }
                categoriesTree.ExpandDepth = currentExpandLevel;
                //categoriesTree.ExpandAll();

                if (categoriesTree.SelectedNode == null)
                {
                    TreeNode selectedNode = null;
                    BOCategory selectedCategory = null;

                    if (SelectedCategory != null && SelectedCategory.Id.HasValue)
                    {
                        selectedCategory = GetCategory(SelectedCategory.Id.Value, ShowUntranslated);
                    }

                    if (selectedCategory != null)
                    {
                        selectedNode = FindNode(selectedCategory);
                    }

                    if (selectedNode != null)
                    {
                        selectedNode.Select();
                    }
                    else
                    {
                        treeNodes[0].Select();
                    }
                }
                else
                {
                    categoriesTree.SelectedNode.Select();
                }
            }

            SelectedCategory = (categoriesTree.SelectedNode == null ? null : GetCategory(Int32.Parse(categoriesTree.SelectedNode.Value.ToString()), ShowUntranslated));
        }

        #endregion Public methods

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
                if ( category.ParentId.HasValue && category.ParentId.Value.ToString() == node.Value )
                {
                    node.ChildNodes.Add(new TreeNode(category.Title, category.Id.ToString()));
                }
            }
            if (node != null)
            {
                foreach ( TreeNode childNode in node.ChildNodes)
                {
                    AddChildren(childNode, categories);
                }
            }
        }

        #endregion Helper methods
    }
}