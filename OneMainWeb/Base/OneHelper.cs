using System;

using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Collections.Generic;

using One.Net.BLL;


namespace OneMainWeb
{
    public class OneHelper
    {
        public static TreeNode PopulateTreeViewControl(List<BOPage> pagesList, TreeNode parentNode, int selectedNodeId, int expandToLevel, ref TreeNode selectedNode)
        {
            TreeNode childNode = null;

            foreach (BOPage page in pagesList)
            {
                if (page.IsRoot && parentNode == null)
                {
                    page.ParentId = -1;
                    HttpContext.Current.Items["rootPageId"] = page.Id;

                    string title = GetCssClass(page) + "<em>[" + page.MenuGroup + "]</em> " + page.Title + "</span>";
                    parentNode = new TreeNode(title, page.Id.ToString());

                    parentNode.Expanded = (page.Level <= expandToLevel || page.Id == selectedNodeId);

                    if (page.Id == selectedNodeId)
                        selectedNode = parentNode;
                }

                if (page.ParentId.ToString() == parentNode.Value.ToString())
                {
                    string title = GetCssClass(page) + "<em>[" + page.MenuGroup + "]</em> " + page.Title + "</span>";
                    childNode = new TreeNode(title, page.Id.ToString());
                    childNode.Expanded = (page.Level <= expandToLevel || page.Id == selectedNodeId);

                    if (page.Id == selectedNodeId)
                        selectedNode = childNode;

                    PopulateTreeViewControl(pagesList, childNode, selectedNodeId, expandToLevel, ref selectedNode);
                    parentNode.ChildNodes.Add(childNode);
                }
            }

            return parentNode;
        }

        private static string GetCssClass(BOPage page)
        {
            string title = "";
            string ret = "<span class=\"";
            if (page.MarkedForDeletion) 
            {
                ret += "pd";
                title = "Marked for deletion";
            }
            else if (page.IsChanged)
            {
                ret += "ch";
                title = "Changes waiting for publish";
            }
            else
            {
                ret += "pub";
                title = "Published";
            }
            ret += "\" data-toggle=\"tooltip\" data-placement=\"bottom\" + title=\"" + title + "\" >";
            return ret;
        }
    }
}
