using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using One.Net.BLL.Model.Web;
using WC = System.Web.UI.WebControls;

using System.Web.Configuration;
using System.Web.Profile;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using One.Net.BLL;
using Microsoft.AspNet.Identity;
using OneMainWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using OneMainWeb.adm;
using NLog;


namespace OneMainWeb
{
    public abstract class OneBasePage : System.Web.UI.Page
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        protected bool EnableXHTMLValidator;
        private AuthorizationHelper authorizationHelper = null;

        public int GridViewPageSize
        {
            get { return 15; }
        }

        protected string GridViewSortExpression
        {
            get
            {
                if (ViewState["sortField"] == null)
                    ViewState["sortField"] = "";
                return (string)ViewState["sortField"];
            }
            set { ViewState["sortField"] = value; }
        }

        protected void GridViewSorting(GridViewSortEventArgs e)
        {
            if (GridViewSortExpression == e.SortExpression)
            {
                GridViewSortDirection = SortDir.Ascending == GridViewSortDirection
                                            ? SortDir.Descending
                                            : SortDir.Ascending;
            }
            else
            {
                GridViewSortExpression = e.SortExpression;
                GridViewSortDirection = SortDir.Ascending;
            } 
        }

        public SortDir GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDir.Ascending;
                return (SortDir)ViewState["sortDirection"];
            }
            set { ViewState["sortDirection"] = value; }
        }

        public AuthorizationHelper Authorization
        {
            get { return authorizationHelper; }
            set { authorizationHelper = value; }
        }
        public OneBasePage()
        {

            authorizationHelper = new AuthorizationHelper(Context);
            bool.TryParse(ConfigurationManager.AppSettings["EnableXHTMLValidator"], out EnableXHTMLValidator);
            log.Debug("OneBasePage() contructor (end)");
        }

        protected BOWebSite SelectedWebsite
        {
            get
            {
                return authorizationHelper.SelectedWebSite;
            }
        }

        protected int SelectedWebSiteId
        {
            get
            {
                return authorizationHelper.SelectedWebSiteId;
            }
            /*set
            {
                authorizationHelper.SelectedWebSiteId = value;
            }*/
        }

        protected int SelectedPageId
        {
            get
            {
                return authorizationHelper.SelectedPageId;
            }
            set
            {
                authorizationHelper.SelectedPageId = value;
            }
        }

        protected bool ExpandTree
        {
            get { return false; }
        }

        protected bool ShowUntranslated
        {
            get { return false; }
        }

        protected bool AutoPublish
        {
            get { return false; }
        }

        protected int? RootNodeID
        {
            get
            {
                int? returnValue = null;

                if (Session["RootNodeId"] != null)
                {
                    returnValue = Int32.Parse(Session["RootNodeId"].ToString());
                }
                return returnValue > 0 ? returnValue : null;
            }
            set 
            {
                
                if (value.HasValue)
                {
                    Session["RootNodeId"] = value;
                }
                else
                {
                    Session["RootNodeId"] = -1;
                }
            }
        }


        protected string RenderStatusIcons(object objMarkedForDeletion, object objIsChanged)
        {
            string strReturn = "";
            if (objIsChanged != null && objMarkedForDeletion != null)
            {
                if (bool.Parse(objMarkedForDeletion.ToString()))
                {
                    strReturn = "/Res/brisanje.gif";
                }
                else if (bool.Parse(objIsChanged.ToString()))
                {
                    strReturn = "/Res/objava.gif";
                }
                else
                {
                    strReturn = "/Res/objavljeno.gif";
                }
            }

            return strReturn;
        }

        protected static void AddEmptyItem(System.Web.UI.WebControls.DropDownList ddl)
        {
            if (ddl.Items.FindByValue("-1") == null)
                ddl.Items.Insert(0, new ListItem("please select filter...", "-1"));
        }

        

        protected List<BOCategory> ArrangeTree(List<BOCategory> list)
        {
            List<BOCategory> tree = new List<BOCategory>();

            BOCategory root = null;

            foreach (BOCategory cat in list)
            {
                if (!cat.ParentId.HasValue)
                {
                    BOCategory newCat = (BOCategory)cat.Clone();
                    tree.Add(newCat);
                    root = newCat;
                    break;
                }
            }

            if (root != null && root.Id.HasValue)
            {
                Hashtable processed = new Hashtable();
                processed[root.Id] = 1;
                PopulateChildren(root, tree, list, processed, 1);
            }
            return tree;
        }

        protected static void PopulateChildren(BOCategory node, List<BOCategory> tree, List<BOCategory> list, Hashtable processed, int increment)
        {
            foreach (BOCategory child in list)
            {
                BOCategory newChild = (BOCategory) child.Clone();
                if (newChild.ParentId.HasValue && newChild.ParentId == node.Id && processed[newChild.Id] == null)
                {
                    string preTag = "";
                    for (int i = 0; i < increment; i++)
                    {
                        preTag += "->";
                    }
                    newChild.Title = preTag + newChild.Title.Replace("->", "");
                    processed[newChild.Id] = 1;
                    tree.Add(newChild);
                    PopulateChildren(newChild, tree, list, processed, increment + 1);
                }
            }
        }
    }
}