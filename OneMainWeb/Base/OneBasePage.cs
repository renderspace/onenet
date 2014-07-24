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


namespace OneMainWeb
{
    public abstract class OneBasePage : System.Web.UI.Page
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OneBasePage));

        WebProfile profile = null;

        protected bool EnableXHTMLValidator;
        
        public OneBasePage()
        {
            profile = new WebProfile(ProfileBase.Create(this.User.Identity.Name, this.User.Identity.IsAuthenticated));

            bool.TryParse(ConfigurationManager.AppSettings["EnableXHTMLValidator"], out EnableXHTMLValidator);        
        }

        protected bool ExpandTree
        {
            get { return profile.ExpandTree; }
            set
            {
                profile.ExpandTree = value;
                profile.Save();
            }
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
                else if (profile.RootNodeId > 0)
                {
                    Session["RootNodeId"] = profile.RootNodeId;
                    returnValue = profile.RootNodeId;
                }
                
                return returnValue > 0 ? returnValue : null;
            }
            set 
            {
                
                if (value.HasValue)
                {
                    Session["RootNodeId"] = value;
                    profile.RootNodeId = value.Value;
                }
                else
                {
                    Session["RootNodeId"] = -1;
                    profile.RootNodeId = -1;
                }
                profile.Save();
            }
        }


        protected string SelectedCulture
        {
            get
            {
                if (Session["SelectedCulture"] != null)
                {
                    return Session["SelectedCulture"].ToString();
                }
                else
                {
                    Session["SelectedCulture"] = profile.SelectedCulture;
                    return profile.SelectedCulture;
                }
            }
            set
            {
                Session["SelectedCulture"] = value;
                profile.SelectedCulture = value;
                profile.Save();
            }
        }

        protected int SelectedUICultureId
        {
            get
            {
                if (Session["SelectedUICultureId"] != null)
                {
                    return Int32.Parse(Session["SelectedUICultureId"].ToString());
                }
                else
                {
                    Session["SelectedUICultureId"] = profile.SelectedUICultureId;
                    return profile.SelectedUICultureId;
                }
            }
            set
            {
                Session["SelectedUICultureId"] = value;
                profile.SelectedUICultureId = value;
                profile.Save();
            }
        }

        protected int SelectedCultureId
        {
            get
            {
                if (Session["SelectedCultureId"] != null)
                {
                    return Int32.Parse(Session["SelectedCultureId"].ToString());
                }
                else
                {
                    Session["SelectedCultureId"] = profile.SelectedCultureId;
                    return profile.SelectedCultureId;
                }
            }
            set
            {
                Session["SelectedCultureId"] = value;
                profile.SelectedCultureId = value;
                profile.Save();
            }
        }

        protected int SelectedPageId
        {
            get
            {
                if (Session["SelectedPageId"] != null)
                {
                    return Int32.Parse(Session["SelectedPageId"].ToString());
                }
                else
                {
                    Session["SelectedPageId"] = profile.SelectedPageId;
                    return profile.SelectedPageId;
                }
            }
            set 
            {
                Session["SelectedPageId"] = value;
                profile.SelectedPageId = value;
                profile.Save();
            }
        }

        protected int SelectedWebSiteId
        {
            get 
            {
                if (Session["SelectedWebSiteId"] != null)
                {
                    return Int32.Parse(Session["SelectedWebSiteId"].ToString());
                }
                else
                {
                    Session["SelectedWebSiteId"] = profile.SelectedWebSiteId;
                    return profile.SelectedWebSiteId;
                }
            }
            set
            {
                Session["SelectedWebSiteId"] = value;
                profile.SelectedWebSiteId = value;
                profile.Save();
            }
        }

        protected override void InitializeCulture()
        {
            if (string.IsNullOrEmpty(SelectedCulture) && SelectedCultureId == 0)
            {
                SelectedCulture = ConfigurationManager.AppSettings["PreferredCulture"].ToString();
            }

            if (SelectedCultureId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedCultureId);
                SelectedCulture = Thread.CurrentThread.CurrentCulture.Name;
            }
            else if (!string.IsNullOrEmpty(SelectedCulture) && SelectedCulture != "Auto")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(SelectedCulture);
                SelectedCultureId = Thread.CurrentThread.CurrentCulture.LCID;
            }

            if ( SelectedWebSiteId == 0)
            {
                SelectedWebSiteId = Int32.Parse(ConfigurationManager.AppSettings["WebSiteId"].ToString());
                CheckWebSiteRole();
            }
        }

        private void CheckWebSiteRole()
        {
            var webSiteB = new BWebsite();
            // the following code is used to fix specific issue where web.config specified website is 
            // loaded on first load to users that may not have permissions to view this website.
            var website = webSiteB.Get(SelectedWebSiteId);
            if (website != null)
            {
                AuthenticationSection authenticationSection = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
                AuthenticationMode currentMode = AuthenticationMode.Windows;
                if (authenticationSection != null)
                    currentMode = authenticationSection.Mode;

                bool adminRole = Roles.IsUserInRole("admin");
                if (!(adminRole || currentMode == AuthenticationMode.Windows || Roles.IsUserInRole("website_" + website.Title.ToLower())))
                {
                    // website with id SelectedWebSiteId is not accessible to user - so use another website id.
                    List<BOWebSite> webSiteList = webSiteB.List();
                    foreach (BOWebSite webSite2 in webSiteList)
                    {
                        if (Roles.IsUserInRole("website_" + webSite2.Title.ToLower()))
                        {
                            SelectedWebSiteId = webSite2.Id;
                            break;
                        }
                    }
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