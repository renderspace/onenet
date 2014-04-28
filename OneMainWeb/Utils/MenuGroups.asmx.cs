using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.Web.UI;
using System.Web.Script.Services;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for MenuGroups
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [ScriptService]
    public class MenuGroups : WebService
    {
        //  Template Declarations
        private string _selectedUrl;
        private string _selectedParentUrl;
        private SiteMapNode _selectedNode;

        public int MaxDepth { get; set; }
        public int MinDepth { get; set; }
        public int ExpandToLevel { get; set; }
        public bool LocalExpand { get; set; }
        public string Group { get; set; }
        public int CurrentPageId { get; set; }
        public List<MenuGroupItem> MenuGroupItems { get; set; }

        protected void CreateNodes()
        {
            if (CurrentPageId > 0)
            {
                RecursiveFind(new SiteMapNodeCollection(SiteMap.RootNode), CurrentPageId, ref _selectedNode);
            }
            if (_selectedNode == null)
                _selectedNode = SiteMap.RootNode;

            if (_selectedNode != null)
            {
                if (string.IsNullOrEmpty(_selectedUrl))
                {
                    _selectedUrl = _selectedNode.Url;
                    if (_selectedNode.ParentNode != null)
                    {
                        _selectedParentUrl = _selectedNode.ParentNode.Url;
                    }
                }

                MenuGroupItems = new List<MenuGroupItem>();
                var coll = new SiteMapNodeCollection(SiteMap.RootNode);
                int currentItemId = 0;
                RecursiveCreateNodes(coll, ref currentItemId);
            }
        }

        private void RecursiveFind(IHierarchicalEnumerable dataItems, int pageId, ref SiteMapNode node)
        {
            foreach (SiteMapNode dataItem in dataItems)
            {
                if (dataItem["_pageID"] == pageId.ToString())
                    node = dataItem;
                if (dataItem.HasChildNodes)
                    RecursiveFind(dataItem.ChildNodes, pageId, ref node);
            }
        }

        private void RecursiveCreateNodes(IHierarchicalEnumerable dataItems, ref int currentItemId)
        {
            int? depth = null;
            bool levelSelected = false;
            bool levelDescendantSelected = false;
            bool levelParentSelected = false;
            bool levelAncestorSelected = false;
            bool firstAssigned = false;

            foreach (SiteMapNode dataItem in dataItems)
            {
                string menuGroup = dataItem["_menuGroup"];
                if (!depth.HasValue)
                    depth = Int32.Parse(dataItem["_absDepth"]);

                if (depth > MaxDepth)
                {
                    break;
                }
                if (menuGroup != Group && depth > this.MinDepth + 1)
                {
                    // do nothing
                }
                else
                {
                    string url = dataItem.Url;

                    string parentUrl = string.Empty;
                    if (dataItem.ParentNode != null)
                        parentUrl = dataItem.ParentNode.Url;

                    string title = dataItem.Title;
                    string pageId = dataItem["_pageID"];
                    bool dataItemHasChildren = dataItem.HasChildNodes;

                    bool dataItemSelected = false;
                    bool dataItemChildSelected = false;
                    bool dataItemDescendantSelected = false;

                    string cssClass = string.Empty;
                    string linkCssClass = string.Empty;

                    if (!firstAssigned)
                    {
                        cssClass = "first ";
                        linkCssClass = "first ";
                        firstAssigned = true;
                    }

                    var data = dataItems.GetHierarchyData(dataItem);

                    if (url == _selectedUrl)
                    {
                        dataItemSelected = true;
                        levelSelected = true;
                        cssClass += " sel";
                        linkCssClass += " asel";
                    }
                    else if (url == _selectedParentUrl)
                    {
                        dataItemChildSelected = true;
                        dataItemDescendantSelected = true;
                        levelDescendantSelected = true;
                        cssClass += " selc";
                        linkCssClass += " aselc";
                    }
                    else if (_selectedNode.IsDescendantOf(dataItem))
                    {
                        levelDescendantSelected = true;
                        dataItemDescendantSelected = true;
                        cssClass += " selc";
                        linkCssClass += " aselc";
                    }

                    // moved under css class decision making so that redirected urls don't affect css class decision
                    if (dataItem["_IsRedirected"] == "True")
                        url = dataItem["_redirectToUrl"].ToString();

                    if (parentUrl == _selectedUrl)
                        levelParentSelected = true;
                    else if (parentUrl == _selectedParentUrl)
                        levelSelected = true;
                    else if (_selectedNode.IsDescendantOf(dataItem.ParentNode))
                        levelDescendantSelected = true;

                    if (dataItem.IsDescendantOf(_selectedNode))
                        levelAncestorSelected = true;

                    if ((menuGroup != Group) || (depth < MinDepth))
                    {
                        if (dataItemHasChildren)
                        {
                            // original if ((dataItemChildSelected || dataItemDescendantSelected || dataItemSelected) || (this.ExpandToLevel > depth && !this._localExpand))
                            if ((dataItemChildSelected || dataItemDescendantSelected || dataItemSelected) || ((LocalExpand && ExpandToLevel > depth && levelAncestorSelected) || (!LocalExpand && ExpandToLevel > depth)))
                            {
                                //  Create any child items
                                RecursiveCreateNodes(data.GetChildren(), ref currentItemId);
                            }
                        }
                    }
                    else
                    {
                        bool showNode = (levelSelected || levelDescendantSelected ||
                                         levelParentSelected ||
                                         ((LocalExpand && ExpandToLevel >= depth && levelAncestorSelected) ||
                                          (!LocalExpand && ExpandToLevel >= depth)));

                        cssClass += " l" + depth.Value + " p" + pageId;

                        cssClass = cssClass.Trim();
                        linkCssClass = linkCssClass.Trim();

                        if (showNode)
                        {
                            currentItemId++; // this item id is incremented for each item that is added in the entire control

                            //  Create the data item
                            var item = new MenuGroupItem(cssClass, linkCssClass, title, url, depth.Value,
                                                         (dataItemSelected || dataItemChildSelected ||
                                                          dataItemDescendantSelected), currentItemId);

                            if (menuGroup == Group)
                                MenuGroupItems.Add(item);
                        }

                        if (depth.Value < MaxDepth && dataItemHasChildren)
                        {
                            if ((dataItemChildSelected || dataItemDescendantSelected || dataItemSelected) || ((LocalExpand && ExpandToLevel > depth && levelAncestorSelected) || (!LocalExpand && ExpandToLevel > depth)))
                            {
                                //  Create any child items
                                RecursiveCreateNodes(data.GetChildren(), ref currentItemId);
                            }
                        }
                    }
                }
            }
        }


        [WebMethod]
        [ScriptMethodAttribute(ResponseFormat = ResponseFormat.Json)]
        public string GetMenuGroupItems(int minDepth, int maxDepth, string menuGroup, int expandToLevel, bool localExpand, int currentPageId)
        {
            MinDepth = minDepth;
            MaxDepth = maxDepth;
            Group = menuGroup;
            ExpandToLevel = expandToLevel;
            LocalExpand = localExpand;
            CurrentPageId = currentPageId;

            CreateNodes();

            if (MenuGroupItems != null && MenuGroupItems.Count > 0)
                return JsonConvert.SerializeObject(MenuGroupItems);
            return "x";
        }
    }

    [Serializable]
    public class MenuGroupItem
    {
        public string CssClass { get; set; }
        public string LinkCssClass { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Depth { get; set; }
        public bool Selected { get; set; }
        public string ItemId { get; set; }

        public MenuGroupItem(string cssClass, string linkCssClass, string title, string url, int depth, bool selected, int itemId)
        {
            CssClass = cssClass;
            LinkCssClass = linkCssClass;
            Title = title;
            Url = url;
            Depth = depth;
            Selected = selected;
            ItemId = "Item" + itemId;
        }
    }
}
