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
        public static TreeNode PopulateTreeViewControl(List<BOPage> pagesList, TreeNode parentNode, int selectedNodeId, Page callingPageObject, int expandToLevel)
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
                    
                    if (page.Level <= expandToLevel)
                    {
                        parentNode.Expanded = true;
                    }
                    else
                    {
                        parentNode.Expanded = false;
                    }

                    if (page.Id == selectedNodeId)
                    {
                        parentNode.Selected = true;
                        parentNode.Expanded = true;
                    }

                }

                if (page.ParentId.ToString() == parentNode.Value.ToString())
                {

                    string title = GetCssClass(page) + "<em>[" + page.MenuGroup + "]</em> " + page.Title + "</span>";
                    childNode = new TreeNode(title, page.Id.ToString());

                    if (page.Level <= expandToLevel)
                    {
                        childNode.Expanded = true;
                    }
                    else
                    {
                        childNode.Expanded = false;
                    }

                    if (page.Id == selectedNodeId)
                    {
                        childNode.Selected = true;
                        childNode.Expanded = true;
                    }

                    PopulateTreeViewControl(pagesList, childNode, selectedNodeId, callingPageObject, expandToLevel);
                    parentNode.ChildNodes.Add(childNode);
                }
            }

            return parentNode;
        }

        private static string GetCssClass(BOPage page)
        {
            string ret = "<span class=\"";
            if (page.MarkedForDeletion) 
            {
                ret += "pd";
            }
            else if (page.IsChanged)
            {
                ret += "ch";
            }
            else
            {
                ret += "pub";
            }
            ret += "\">";
            return ret;
        }

        public static string GetFileIcon(Page page, string location, string extension)
        {
            string ret = "";
            switch (extension)
            {
                case "ai":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof (OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "avi":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof (OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "cs":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "css":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + "default.icon.gif") + "\" /></a>";
                    break;
                case "dll":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "doc":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "exe":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "fla":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "gif":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "htm":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "html":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "jpg":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "js":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "mdb":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "mp3":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "pdf":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "ppt":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "rdp":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "swf":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "swt":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "txt":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "vsd":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "xls":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "xml":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                case "zip":
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + extension + ".gif") + "\" /></a>";
                    break;
                default:
                    ret += "<img src=\"" + page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), location + "default.icon.gif") + "\" /></a>";
                    break;
            }
            return ret;
        }
    }
}
