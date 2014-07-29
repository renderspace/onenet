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


namespace OneMainWeb
{
    public abstract class OneBasePage : System.Web.UI.Page
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OneBasePage));

        WebProfile profile = null;
        protected bool EnableXHTMLValidator;
        private AuthorizationHelper authorizationHelper = null;
        public OneBasePage()
        {

            authorizationHelper = new AuthorizationHelper(Context);
            bool.TryParse(ConfigurationManager.AppSettings["EnableXHTMLValidator"], out EnableXHTMLValidator);        
        }

        protected int SelectedWebSiteId
        {
            get
            {
                return authorizationHelper.SelectedWebSiteId;
            }
            set
            {
                authorizationHelper.SelectedWebSiteId = value;
            }
        }

        protected string SelectedCulture
        {
            get
            {
                return authorizationHelper.SelectedCulture;
            }
        }

        protected int SelectedCultureId
        {
            get
            {
                return authorizationHelper.SelectedCultureId;
            }
            set
            {
                authorizationHelper.SelectedCultureId = value;
            }
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
            get { return profile.ShowUntranslated; }
            set
            {
                profile.ShowUntranslated = value;
                profile.Save();
            }
        }

        protected bool AutoPublish
        {
            get { return profile.AutoPublish; }
            set
            {
                profile.AutoPublish = value;
                profile.Save();
            }
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


       

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PreRenderComplete += AdminMetaPage2_PreRenderComplete;
        }

        private void AdminMetaPage2_PreRenderComplete(object sender, EventArgs e)
        {
            AdminHelper.TranslateControls(Controls, ResourceManager.DEFAULT_RESOURCE_FILE);
            if (Master != null)
                TranslateMasterControls(Master.Controls);
        }

        private static void TranslateMasterControls(ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is Menu)
                {
                    Menu menu = ctrl as Menu;
                    TranslateMenuItems(menu.Items);
                }

                TranslateMasterControls(ctrl.Controls);
            }
        }

        private static void TranslateMenuItems(MenuItemCollection items)
        {
            foreach (MenuItem item in items)
            {
                item.Text = ResourceManager.GetString(item.Text);
                TranslateMenuItems(item.ChildItems);
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
                ddl.Items.Insert(0, new ListItem(ResourceManager.GetString("$label_no_filter"), "-1"));
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