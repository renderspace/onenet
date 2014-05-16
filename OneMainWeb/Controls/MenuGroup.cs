using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;

using System.Diagnostics;
using log4net;

namespace OneMainWeb.Controls
{
    /// <summary>
    /// ****** IMPORTANT NOTE!!! ******
    /// What is INamingContainer?
    /// INamingContainer is a marker interface, meaning it has no methods to implement. 
    /// A control "implements" this interface to let the framework know that it plans on giving it's child controls really specific IDs. 
    /// It's important to the framework, because if two instances of the same control are on the same page, 
    /// and the control gives its child controls some specific ID, there'd end up being multiple controls with the same ID on the page, 
    /// which is going to cause problems. 
    /// So when a Control is a naming container, the UniqueID for all controls within it will have the parent's ID as a prefix. 
    /// This scopes it to that control. So while a child control might have ID "foo", its UniqueID will be "parentID$foo" (where parentID = the ID of the parent). 
    /// Now even if this control exists twice on the page, everyone will still have a UniqueID.
    /// INamingContainer also has the property that any controls within it that do not have a specific ID will have its ID automatically determined based on a counter that is scoped to it. 
    /// So if there were two naming containers, foo and bar, they might have child controls with UniqueIDs foo$ctl01 and bar$ctl01. 
    /// Each naming container gets its own little counter.
    /// ****** END IMPORTANT NOTE ******
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:MenuGroup runat=server></{0}:MenuGroup>")]
    public class MenuGroup : WebControl, INamingContainer // HierarchicalDataBoundControl
    {
        private readonly ILog log = LogManager.GetLogger("OneMainWeb.Controls.MenuGroup");

        private bool controlsAdded = false;

        //  Template Declarations
        private string _selectedUrl;
        private string _selectedParentUrl;
        private SiteMapNode _selectedNode;
        private SiteMapNode _rootNode;

        public int MaxDepth { get; set; }

        public int MinDepth { get; set; }

        public int ExpandToLevel { get; set; }

        public bool LocalExpand { get; set; }

        public string Group { get; set; }

        public string FirstUlClass { get; set; }

        public MenuGroup()
        {
            base.EnableViewState = false;
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            if (controlsAdded)
            {
                writer.Write("\n");
                writer.Indent = 0;
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                writer.RenderBeginTag("nav");
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (controlsAdded)
            {
                base.RenderEndTag(writer);
            }
        }

        protected override void CreateChildControls()
        {

            //SiteMap.Providers["AspNetXmlSiteMapProvider"]
            _selectedNode = SiteMap.CurrentNode;
            _rootNode = SiteMap.RootNode;

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
                SiteMapNodeCollection coll = new SiteMapNodeCollection(SiteMap.RootNode);
                int currentItemId = 0;
                int preControlsCount = this.Controls.Count;
                RecursiveCreateChildControls(coll, ref currentItemId);
                int postControlsCount = this.Controls.Count;

                if (postControlsCount > preControlsCount)
                {
                    controlsAdded = true;
                }
            }
            Enabled = Visible = controlsAdded;
        }

        private void RecursiveCreateChildControls(IHierarchicalEnumerable dataItems, ref int currentItemId)
        {
            bool headerRendered = false;
            int? depth = null;
            bool levelSelected = false;
            bool levelDescendantSelected = false;
            bool levelParentSelected = false;
            bool levelAncestorSelected = false;

            bool firstAssigned = false;

            foreach (SiteMapNode dataItem in dataItems)
            {
                string _menuGroup = dataItem["_menuGroup"];

                if (!depth.HasValue)
                {
                    depth = int.Parse(dataItem["_absDepth"]);
                }

                if (depth > MaxDepth)
                {
                    break;
                }
                else if (_menuGroup != Group && depth > MinDepth + 1)
                {
                    // do nothing
                }
                /*else if (group != this._group && ExpandToLevel > depth)
                {
                    // do nothing
                }*/
                else
                {
                    string url = dataItem.Url;

                    string parentUrl = string.Empty;
                    if (dataItem.ParentNode != null)
                    {
                        parentUrl = dataItem.ParentNode.Url;
                    }

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

                    IHierarchyData data = dataItems.GetHierarchyData(dataItem);

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
                    if (dataItem["_IsRedirected"] == "True")
                    {
                        url = dataItem["_redirectToUrl"].ToString();
                    }

                    if (parentUrl == _selectedUrl)
                        levelParentSelected = true;
                    else if (parentUrl == _selectedParentUrl)
                        levelSelected = true;
                    else if (_selectedNode.IsDescendantOf(dataItem.ParentNode))
                        levelDescendantSelected = true;

                    if (dataItem.IsDescendantOf(_selectedNode))
                        levelAncestorSelected = true;

                    if ((_menuGroup != Group) || (depth < MinDepth))
                    {
                        if (dataItemHasChildren)
                        {
                            if ((dataItemChildSelected || dataItemDescendantSelected || dataItemSelected) || ((this.LocalExpand && this.ExpandToLevel > depth && levelAncestorSelected) || (!LocalExpand && ExpandToLevel > depth)))
                            {
                                RecursiveCreateChildControls(data.GetChildren(), ref currentItemId);
                            }
                        }
                    }
                    else
                    {
                        bool showNode = (levelSelected || levelDescendantSelected ||
                                         levelParentSelected ||
                                         ((this.LocalExpand && this.ExpandToLevel >= depth && levelAncestorSelected) ||
                                          (!LocalExpand && ExpandToLevel >= depth)));

                        if (pageId != null)
                        {
                            cssClass += (" l" + depth.Value + " p" + pageId).Trim();
                        }
                        linkCssClass = linkCssClass.Trim();
                        if (showNode)
                        {
                            if (!headerRendered)
                            {
                                //  Render the header only once <ul>
                                Literal header = new Literal();
                                header.Text = "\n" + CreateIndentTabs(depth.Value) + "<ul class=\"" + FirstUlClass + "\">";
                                Controls.Add(header);
                                headerRendered = true;
                            }

                            currentItemId++; // this item id is incremented for each item that is added in the entire control

                            //  Create an item header <li>
                            Literal itemHeader = new Literal();
                            itemHeader.Text = CreateIndentTabs(depth.Value + 1) + "<li class=\"" + cssClass + "\">";
                            itemHeader.ID = "ItemHeader" + currentItemId;
                            Controls.Add(itemHeader);

                            //  Create the data item
                            Literal item = RenderItem(linkCssClass, title, url);
                            item.ID = "Item" + currentItemId;

                            if (_menuGroup == Group)
                                Controls.Add(item);
                            else 
                                showNode = false;

                        }

                        if (depth.Value < MaxDepth && dataItemHasChildren)
                        {
                            if ((dataItemChildSelected || dataItemDescendantSelected || dataItemSelected) || ((this.LocalExpand && this.ExpandToLevel > depth && levelAncestorSelected) || (!LocalExpand && ExpandToLevel > depth)))
                            {
                                //  Create any child items
                                RecursiveCreateChildControls(data.GetChildren(), ref currentItemId);
                            }
                        }

                        if (showNode)
                        {
                            //  Create the item footer </li>
                            Literal itemFooter = new Literal();
                            itemFooter.Text = "</li>\n";
                            Controls.Add(itemFooter);
                        }
                    }
                }
            }

            //  If we had a header, then render out the footer </ul>
            if (headerRendered)
            {
                // Before rendering footer, change the last item class to contain the class "last".
                // Also, change the last item header class to contain the class "last"
                Literal tmpHeader = this.FindControl("ItemHeader" + (currentItemId).ToString()) as Literal;
                Literal tmpItem = this.FindControl("Item" + (currentItemId).ToString()) as Literal;

                if (tmpHeader != null && tmpItem != null)
                {
                    if (!tmpHeader.Text.Contains("class=\"first"))
                    {
                        tmpHeader.Text = tmpHeader.Text.Replace("class=\"", "class=\"last ");
                    }

                    if (!tmpItem.Text.Contains("class=\"first"))
                    {
                        tmpItem.Text = tmpItem.Text.Replace("class=\"", "class=\"last ");
                    }
                }

                Literal footer = new Literal();
                footer.Text = CreateIndentTabs(depth.Value) + "</ul>\n";
                Controls.Add(footer);
            }
        }

        protected virtual Literal RenderItem(string linkCssClass, string title, string url)
        {
            Literal item = new Literal();
            item.Text = "<a href=\"" + url + "\" class=\"" + linkCssClass + "\">" + title + "</a>";
            return item;
        }

        private static string CreateIndentTabs(int indentLevel)
        {
            string s = new string('\t', indentLevel);
            return s;
        }
    }
}
