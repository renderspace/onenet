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
                return FormatTool.GetInteger(node.ChildNodes[0]["_pageID"]);
            if (node.NextSibling != null)
                return FormatTool.GetInteger(node.NextSibling["_pageID"]);
            if (node.ParentNode != null && node.ParentNode.NextSibling != null)
                return FormatTool.GetInteger(node.ParentNode.NextSibling["_pageID"]);
            return node.ParentNode != null ? FindNextNodeId(node.ParentNode, false) : null;
        }

        private int? FindPrevNodeId(SiteMapNode node)
        {
            if (node["_pageID"] == SiteMap.RootNode["_pageID"])
                return null;
            if (node.PreviousSibling != null)
            {
                return node.PreviousSibling.HasChildNodes
                           ? FindLastNodeId(node.PreviousSibling)
                           : FormatTool.GetInteger(node.PreviousSibling["_pageID"]);
            }
            return node.ParentNode != null ? FormatTool.GetInteger(node.ParentNode["_pageID"]) : (int?) null;
        }

        private int? FindLastNodeId(SiteMapNode node)
        {
            return node.HasChildNodes
                       ? FindLastNodeId(node.ChildNodes[node.ChildNodes.Count - 1])
                       : FormatTool.GetInteger(node["_pageID"]);
        }
    }
}