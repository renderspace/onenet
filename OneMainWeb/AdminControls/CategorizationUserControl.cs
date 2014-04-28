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
    public abstract class CategorizationUserControl : System.Web.UI.UserControl
    {
        #region Delegates

        public delegate void CategoryMover(BOCategory category, BOCategory newParent);
        public delegate void CategoryChanger(BOCategory cat);
        public delegate bool CategoryDeleter(int id);
        public delegate BOCategory CategoryGetter(int id, bool showUntranslated);
        public delegate List<BOCategory> CategoryLister();
        public delegate List<BOCategory> CategoryListerWithFilter(List<int> filter, bool showUntranslated);
        public delegate List<ICategorizable> CategorizedItemsLister(int categoryId);

        #endregion Delegates

        #region Variables

        private CategoryMover moveCategory;
        private CategoryChanger changeCategory;
        private CategoryLister listCategories;
        private CategoryGetter getCategory;
        private CategoryDeleter deleteCategory;
        private CategoryListerWithFilter listCategoriesWithFilter;
        private CategorizedItemsLister listCategorizedItems;

        private bool showUntranslated;
        private bool htmlVisible;
        private bool titleVisible;
        private bool subTitleVisible;
        private bool teaserVisible;
        private bool useCkEditor;
        private bool showIsSelectable;
        private bool showIsPrivate;
        private bool showCategorizedItemsGrid;

        private BOCategory selectedCategory;
        private string categoryType;
        private List<int> categoryFilter;
        private int nodeCount;

        private bool expandTree;
        private bool enableDelete;

        #endregion Variables

        #region Properties

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryMover MoveCategory
        {
            get { return moveCategory; }
            set { moveCategory = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryChanger ChangeCategory
        {
            get { return changeCategory; }
            set { changeCategory = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryLister ListCategories
        {
            get { return listCategories; }
            set { listCategories = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategorizedItemsLister ListCategorizedItems
        {
            get { return listCategorizedItems; }
            set { listCategorizedItems = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryListerWithFilter ListCategoriesWithFilter
        {
            get { return listCategoriesWithFilter; }
            set { listCategoriesWithFilter = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryGetter GetCategory
        {
            get { return getCategory; }
            set { getCategory = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public CategoryDeleter DeleteCategory
        {
            get { return deleteCategory; }
            set { deleteCategory = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool UseCkEditor
        {
            get { return useCkEditor; }
            set { useCkEditor = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool ShowUntranslated
        {
            get { return showUntranslated; }
            set { showUntranslated = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool ShowIsSelectable
        {
            get { return showIsSelectable; }
            set { showIsSelectable = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool ShowIsPrivate
        {
            get { return showIsPrivate; }
            set { showIsPrivate = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool ShowCategorizedItemsGrid
        {
            get { return showCategorizedItemsGrid; }
            set { showCategorizedItemsGrid = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool HtmlVisible
        {
            get { return htmlVisible; }
            set { htmlVisible = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool TitleVisible
        {
            get { return titleVisible; }
            set { titleVisible = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool SubTitleVisible
        {
            get { return subTitleVisible; }
            set { subTitleVisible = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public bool TeaserVisible
        {
            get { return teaserVisible; }
            set { teaserVisible = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public List<int> CategoryFilter
        {
            get { return categoryFilter; }
            set { categoryFilter = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public string CategoryType
        {
            get { return categoryType; }
            set { categoryType = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public BOCategory SelectedCategory
        {
            get { return selectedCategory; }
            set { selectedCategory = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        public int NodeCount
        {
            get { return nodeCount; }
            set { nodeCount = value; }
        }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool EnableDelete
        {
            get { return enableDelete; }
            set { enableDelete = value; }
        }        

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public bool ExpandTree
        {
            get { return expandTree; }
            set { expandTree = value; }
        }

        #endregion Properties

        #region Methods

        public bool IsDescendantOf(TreeNode testNode, TreeNode against)
        {
            // if test node is root, then it is not a descendant of anything
            if (testNode.Parent == null)
                return false;
            // else if against is root, then everything is its descendent
            // or if testNode is a direct descendent of against
            else if (against.Parent == null || testNode.Parent.Value == against.Value)
                return true;

            return CountFound(testNode, against.ChildNodes) > 0;
        }

        private static int CountFound(TreeNode testNode, TreeNodeCollection coll)
        {
            int count = 0;

            foreach (TreeNode node in coll)
            {
                if (node.Value == testNode.Value)
                {
                    return 1;
                }
                else if ( node.ChildNodes != null && node.ChildNodes.Count > 0)
                    count += CountFound(testNode, node.ChildNodes);
            }
            return count;
        }

        public string BuildPath(BOCategory category)
        {
            string valuePath = category.Id.Value.ToString();

            if (category.ParentId.HasValue)
            {
                BOCategory parent = GetCategory(category.ParentId.Value, showUntranslated);
                valuePath = BuildPath(parent) + "/" + valuePath;
            }

            return valuePath;
        }

        #endregion Methods

        #region State methods

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object[] cSThis = new object[23];
            object cSBase = base.SaveControlState();

            cSThis[0] = cSBase;
            cSThis[1] = changeCategory;
            cSThis[2] = listCategories;
            cSThis[3] = getCategory;
            cSThis[4] = deleteCategory;
            cSThis[5] = listCategoriesWithFilter;
            cSThis[6] = htmlVisible;
            cSThis[7] = titleVisible;
            cSThis[8] = subTitleVisible;
            cSThis[9] = teaserVisible;
            cSThis[10] = useCkEditor;
            cSThis[11] = selectedCategory;
            cSThis[12] = categoryType;
            cSThis[13] = categoryFilter;
            cSThis[14] = nodeCount;
            cSThis[15] = listCategorizedItems;
            cSThis[16] = showIsSelectable;
            cSThis[17] = showCategorizedItemsGrid;
            cSThis[18] = moveCategory;
            cSThis[19] = showUntranslated;
            cSThis[20] = expandTree;
            cSThis[21] = enableDelete;
            cSThis[22] = showIsPrivate;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];

            changeCategory = (CategoryChanger)cSThis[1];
            listCategories = (CategoryLister)cSThis[2];
            getCategory = (CategoryGetter)cSThis[3];
            deleteCategory = (CategoryDeleter)cSThis[4];
            listCategoriesWithFilter = (CategoryListerWithFilter)cSThis[5];
            htmlVisible = (bool)cSThis[6];
            titleVisible = (bool)cSThis[7];
            subTitleVisible = (bool)cSThis[8];
            teaserVisible = (bool)cSThis[9];
            useCkEditor = (bool)cSThis[10];
            selectedCategory = (BOCategory)cSThis[11];
            categoryType = (string)cSThis[12];
            categoryFilter = (List<int>)cSThis[13];
            nodeCount = (int)cSThis[14];
            listCategorizedItems = (CategorizedItemsLister)cSThis[15];
            showIsSelectable = (bool)cSThis[16];
            showCategorizedItemsGrid = (bool)cSThis[17];
            moveCategory = (CategoryMover) cSThis[18];
            showUntranslated = (bool) cSThis[19];
            expandTree = (bool)cSThis[20];
            enableDelete = (bool)cSThis[21];
            showIsPrivate = (bool)cSThis[22];

            base.LoadControlState(cSBase);
        }

        #endregion State methods
    }
}
