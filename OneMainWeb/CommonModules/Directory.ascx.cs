using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using log4net;
using One.Net.BLL;

using One.Net.BLL.Web;
using OneMainWeb.Controls;

namespace OneMainWeb.CommonModules
{
    public partial class Directory : MModule
    {
        public int MinLevel { get { return GetIntegerSetting("MinLevel"); } }
        public int MaxLevel { get { return GetIntegerSetting("MaxLevel"); } }
        public bool ShowCurrentPage { get { return GetBooleanSetting("ShowCurrentPage"); } }
        public List<int> ExcludeMenuGroupsList { get { return GetIntegerListSetting("ExcludeMenuGroupsList"); } }

        private readonly ILog log = LogManager.GetLogger("OneMainWeb.CommonModules.Directory");

        protected override void CreateChildControls()
        {
            SiteMapNodeCollection coll = new SiteMapNodeCollection(SiteMap.RootNode);
            RecursiveCreateChildControls(coll);
        }

        private void RecursiveCreateChildControls(IHierarchicalEnumerable dataItems)
        {
            bool headerRendered = false;
            int? menuGroup = null;
            int? depth = null;
            foreach (SiteMapNode dataItem in dataItems)
            {
                if (!depth.HasValue)
                {
                    depth = Int32.Parse(dataItem["_absDepth"]);
                }

                menuGroup = Int32.Parse(dataItem["_menuGroup"]);

                var pageId = int.Parse(dataItem["_pageID"]);

                if (this.MaxLevel < depth)
                {
                    break;
                }
                else
                {
                    string url = dataItem.Url;
                    string title = dataItem.Title;
                    bool dataItemHasChildren = dataItem.HasChildNodes;

                    string cssClass = "mg" + menuGroup.Value.ToString() + " ";
                    string linkCssClass = "";

                    IHierarchyData data = dataItems.GetHierarchyData(dataItem);

                    if (url == SiteMap.CurrentNode.Url)
                    {
                        cssClass += " sel";
                        linkCssClass = " asel";
                    }

                    if (depth < this.MinLevel)
                    {
                        if (dataItemHasChildren)
                        {
                            //  Create any child items
                            RecursiveCreateChildControls(data.GetChildren());
                        }
                    }
                    else
                    {
                        cssClass = cssClass.Trim();
                        linkCssClass = linkCssClass.Trim();

                        if (!headerRendered)
                        {
                            //  Render the header only once <ul>
                            Literal header = new Literal();
                            header.Text = "<ul>";
                            Controls.Add(header);
                            headerRendered = true;
                        }

                        //  Create an item header <li>
                        Literal itemHeader = new Literal();
                        itemHeader.Text = "<li class=\"" + cssClass + " dp" + pageId + "\">";
                        

                        //  Create the data item
                        Literal item = new Literal();
                        item.Text = "<a href=\"" + url + "\" class=\"" + linkCssClass + "\">" + title + "</a>";

                        if (!ExcludeMenuGroupsList.Contains(menuGroup.Value))
                        {
                            Controls.Add(itemHeader);
                            Controls.Add(item);
                            if (depth.Value < this.MaxLevel && dataItemHasChildren)
                            {
                                //  Create any child items
                                RecursiveCreateChildControls(data.GetChildren());
                            }
                            Literal itemFooter = new Literal();
                            itemFooter.Text = "</li>";
                            Controls.Add(itemFooter);
                        }                        
                    }
                }
            }

            //  If we had a header, then render out the footer </ul>
            if (headerRendered)
            {
                Literal footer = new Literal();
                footer.Text = "</ul>";
                Controls.Add(footer);
            }
        }
    }
}