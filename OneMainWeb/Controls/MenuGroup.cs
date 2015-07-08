using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;

using System.Diagnostics;
using NLog;
using One.Net.BLL;

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
        protected static Logger log = LogManager.GetCurrentClassLogger();

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

        public bool ShowDescription { get; set; }

        public int LeadImageTemplateId { get; set; }

        protected BOImageTemplate LeadImageTemplate 
        { 
            get 
            {
                if (LeadImageTemplateId < 1)
                    return null;
                object o = BWebsite.GetTemplate(LeadImageTemplateId);
                if (o is BOImageTemplate)
                    return (BOImageTemplate)o;
                else
                    return null;
            } 
        }

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

            if (_selectedNode == null && Page is PresentBasePage && string.IsNullOrEmpty(_selectedUrl))
            {
                _selectedParentUrl = ((PresentBasePage)Page).CurrentPage.ParentURI;
                if (_selectedParentUrl.Length > 1)
                    _selectedParentUrl = _selectedParentUrl.TrimEnd('/');
                _selectedUrl = ((PresentBasePage)Page).CurrentPage.URI;

                // let's cheat and get the first subpage node of ParentNode
                var coll = SiteMap.RootNode.GetAllNodes();
                var e = coll.GetEnumerator();
                while ((e.MoveNext()) && (e.Current != null) && _selectedNode == null)
                {
                    if (e.Current is SiteMapNode)
                    {
                        var smn = (SiteMapNode) e.Current;
                        if (smn.Url == _selectedParentUrl)
                        {
                            if (smn.ChildNodes.Count == 1)
                            {
                                _selectedNode = smn.ChildNodes[0];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            } 
            if (_selectedNode != null && string.IsNullOrEmpty(_selectedUrl))
            {
                _selectedUrl = _selectedNode.Url;
                if (_selectedNode.ParentNode != null)
                {
                    _selectedParentUrl = _selectedNode.ParentNode.Url;
                }
            }

            if (!string.IsNullOrEmpty(_selectedUrl))
            {
                int currentItemId = 0;
                int preControlsCount = this.Controls.Count;
                RecursiveCreateChildControls(new SiteMapNodeCollection(SiteMap.RootNode), ref currentItemId);
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

                    string parentUrl = "";
                    if (dataItem.ParentNode != null)
                    {
                        parentUrl = dataItem.ParentNode.Url;
                    }

                    string pageId = dataItem["_pageID"];
                    bool dataItemHasChildren = dataItem.HasChildNodes;

                    bool dataItemSelected = false;
                    bool dataItemChildSelected = false;
                    bool dataItemDescendantSelected = false;

                    string cssClass = "";
                    string linkCssClass = "";

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
                        cssClass += " sel active ";
                        linkCssClass += " asel ";
                    }
                    else if (url == _selectedParentUrl)
                    {
                        dataItemChildSelected = true;
                        dataItemDescendantSelected = true;
                        levelDescendantSelected = true;
                        cssClass += " selc ";
                        linkCssClass += " aselc ";
                    }
                    else if (_selectedNode.IsDescendantOf(dataItem))
                    {
                        levelDescendantSelected = true;
                        dataItemDescendantSelected = true;
                        cssClass += " selc ";
                        linkCssClass += " aselc ";
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
                        if (depth == MinDepth && Controls.Count > 0)
                        {
                            // Samo and I consulted and it was decided that if page at MinDepth does not match
                            // the Group we want to display, we do not recursively display anything that is a child 
                            // of that page.
                            // I added Controls.Count > 0 in order to allow for root page to not match if MinDepth is 0.
                        } 
                        else if (dataItemHasChildren)
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

                            var item = RenderItem(linkCssClass, dataItem.Title, url, dataItem.Description, dataItem["_ogImage"], currentItemId);

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

        protected Literal RenderItem(string linkCssClass, string title, string url, string description, string leadImageUri, int currentItemId)
        {
            var item = new Literal();

            var leadImageTag = "";
            if (!string.IsNullOrWhiteSpace(leadImageUri) && LeadImageTemplate != null)
            {
                leadImageTag = LeadImageTemplate.RenderHtml(description, leadImageUri, "");
            }
            item.Text = "<a href=\"" + url + "\" class=\"" + linkCssClass + "\">" + leadImageTag + "<span>" + title + "</span>" + (ShowDescription && !string.IsNullOrWhiteSpace(description) ? "<span class=\"d\">" + description + "</span>" : "") + "</a>";
            item.ID = "Item" + currentItemId;
            return item;
        }

        private static string CreateIndentTabs(int indentLevel)
        {
            return new string('\t', indentLevel);
        }
    }
}
