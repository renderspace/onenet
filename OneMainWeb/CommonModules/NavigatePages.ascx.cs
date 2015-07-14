using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class NavigatePages : MModule
    {
        private readonly BWebsite _websiteB = new BWebsite();
        private bool IsPrevNavigation { get { return GetBooleanSetting("IsPrevNavigation"); } }

        private int? PrevPageId { get; set; }
        private int? NextPageId { get; set; }

        private string NavigateUri
        {
            get { return (IsPrevNavigation) ? PrevPageUri : NextPageUri; }
        }

        private string PrevPageUri
        {
            get { return (PrevPageId.HasValue) ? _websiteB.GetPageUri(PrevPageId.Value) : ""; }
        }

        private string NextPageUri
        {
            get { return (NextPageId.HasValue) ? _websiteB.GetPageUri(NextPageId.Value) : ""; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PrevPageId = FindPrevNodeId(SiteMap.CurrentNode);
                NextPageId = FindNextNodeId(SiteMap.CurrentNode, true);

                if (!string.IsNullOrEmpty(NavigateUri))
                {
                    HyperLinkNavigate.Visible = true;
                    HyperLinkNavigate.NavigateUrl = NavigateUri;
                    HyperLinkNavigate.Text = IsPrevNavigation ? Translate("navigate_pages_prev") : Translate("navigate_pages_next");
                }
            }
        }

        private int? FindNextNodeId(SiteMapNode node, bool checkChildren)
        {
            if (checkChildren && node.HasChildNodes)
            {
                //_redirectToUrl
                var firstChildNode = node.ChildNodes[0];

                // if the next node is a redirect, don't return it, seek further, otherwise you get a loop
                if (string.IsNullOrEmpty(firstChildNode["_redirectToUrl"]))
                    return FormatTool.GetInteger(firstChildNode["_pageID"]);
                else
                    return FindNextNodeId(firstChildNode, true);
            }
            if (node.NextSibling != null)
            {
                var nextSiblingNode = node.NextSibling;

                // if the next node is a redirect, don't return it, seek further, otherwise you get a loop
                if (string.IsNullOrEmpty(nextSiblingNode["_redirectToUrl"]))
                    return FormatTool.GetInteger(nextSiblingNode["_pageID"]);
                else
                    return FindNextNodeId(nextSiblingNode, true);
            }
            if (node.ParentNode != null && node.ParentNode.NextSibling != null)
            {
                var nextParentSibling = node.ParentNode.NextSibling;

                // if the next node is a redirect, don't return it, seek further, otherwise you get a loop
                if (string.IsNullOrEmpty(nextParentSibling["_redirectToUrl"]))
                    return FormatTool.GetInteger(nextParentSibling["_pageID"]);
                else
                    return FindNextNodeId(nextParentSibling, true);
            }
            return node.ParentNode != null ? FindNextNodeId(node.ParentNode, false) : (int?) null;
        }

        private int? FindPrevNodeId(SiteMapNode node)
        {
            if (node["_pageID"] == SiteMap.RootNode["_pageID"])
                return null;
            if (node.PreviousSibling != null)
            {
                var previousSiblingNode = node.PreviousSibling;

                if (previousSiblingNode.HasChildNodes)
                    return FindLastNodeId(previousSiblingNode);
                else
                {
                    // if the prev node is a redirect, don't return it, seek further, otherwise you get a loop
                    if (string.IsNullOrEmpty(previousSiblingNode["_redirectToUrl"]))
                        return FormatTool.GetInteger(previousSiblingNode["_pageID"]);
                    else
                        return FindPrevNodeId(previousSiblingNode);
                }
            }

            var parentNode = node.ParentNode;

            if (parentNode != null)
            {
                if (string.IsNullOrEmpty(parentNode["_redirectToUrl"]))
                    return FormatTool.GetInteger(parentNode["_pageID"]);
                else
                    return FindPrevNodeId(parentNode);
            }

            return (int?)null;
        }

        private int? FindLastNodeId(SiteMapNode node)
        {
            if (node.HasChildNodes)
                return FindLastNodeId(node.ChildNodes[node.ChildNodes.Count - 1]);

            if (string.IsNullOrEmpty(node["_redirectToUrl"]))
                return FormatTool.GetInteger(node["_pageID"]);
            else
                return FindPrevNodeId(node);
        }
    }
}