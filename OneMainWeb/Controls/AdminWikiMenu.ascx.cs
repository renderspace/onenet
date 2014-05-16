using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Web;
using System.Configuration;
using One.Net.BLL;

namespace OneMainWeb.Controls
{
    public partial class AdminWikiMenu : MModule
    {
        BWebsite webSiteB = new BWebsite();

        protected void Page_Load(object sender, EventArgs e)
        {
            Version version = Page.GetType().BaseType.Assembly.GetName().Version;
            string szHtml = string.Format(@"<span class=""adminPreview"">{0}</span>
<span class=""adminOneNet"">One.NET v{1}</span>
<span class=""adminVersion"">{2} {3}</span>", "Preview", version, Request.Browser.Browser, Request.Browser.Version);
            LiteralInfo.Text = szHtml;

            if (!IsPostBack)
            {
                var RepeaterModules = LoginView1.FindControl("RepeaterModules") as Repeater;
                if (RepeaterModules != null)
                {
                    RepeaterModules.DataSource = BWebsite.ListModules();
                    RepeaterModules.DataBind();
                }
            }
            /*
             * Literal lit = new Literal();
			
			
			lit.Text = szHtml;
             * */

        }

        protected void LinkButtonClearCache_Click(object sender, EventArgs e)
        {
            OCache.Clear();
            OneSiteMapProvider.ReloadSiteMap();
        }



        protected void LinkButtonDelete_Click(object sender, EventArgs e)
        {
            var result = webSiteB.DeletePageById(CurrentPageId);
            switch (result)
            {
                case BWebsite.DeletePageByIdResult.DeletedRoot:
                    // well, what can we do...
                    OCache.Clear();
                    OneSiteMapProvider.ReloadSiteMap();
                    break;
                case BWebsite.DeletePageByIdResult.Deleted:
                    OCache.Clear();
                    OneSiteMapProvider.ReloadSiteMap();
                    Response.Redirect(ParentPageUrl);
                    break;
                case BWebsite.DeletePageByIdResult.HasChildren:
                    Notifier1.Warning = ResourceManager.GetString("$has_children_delete_not_possible");
                    break;
                case BWebsite.DeletePageByIdResult.Error:
                    Notifier1.Warning = ResourceManager.GetString("$delete_page_error");
                    break;
            }
        }

        protected void LinkButtonLogout_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            Session.Contents.RemoveAll();
            System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("/");
        }

        protected void LinkButtonNewPage_Click(object sender, EventArgs e)
        {
            if (CurrentPageId == 0)
                throw new Exception("Couldn't find current page ID");

            var TextBoxPageTitle = LoginView1.FindControl("TextBoxPageTitle") as TextBox;

            var result = webSiteB.AddSubPage(TextBoxPageTitle.Text, CurrentWebSiteId, CurrentPageId);
            switch (result)
            {
                case BWebsite.AddSubPageResult.Ok:
                    TextBoxPageTitle.Text = "";
                    break;
                case BWebsite.AddSubPageResult.NoTemplates:
                    Notifier1.Warning = "$unsucessfull_add_page";
                    Notifier1.Message = "There are no availible templates in the database!";
                    break;
                case BWebsite.AddSubPageResult.OkRootPage:
                    Notifier1.Message = "$label_insert_root_par_link_as_blank";
                    TextBoxPageTitle.Text = "";
                    break;
                case BWebsite.AddSubPageResult.PartialLinkExistsOnThisLevel:
                    Notifier1.Warning = "$unsucessfull_add_page";
                    Notifier1.Message = "This child page cannot be added because the page URL already exists at this level under this parent page!";
                    break;
                case BWebsite.AddSubPageResult.PartialLinkNotValid:
                    Notifier1.Warning = "$unsucessfull_add_page";
                    Notifier1.Message = "This URL cannot be updated because it contains invalid characters! Valid characters are a to z, A to Z and 0 to 9";
                    break;
                case BWebsite.AddSubPageResult.TriedToAddRootPageToNonEmptySite:
                    Notifier1.Warning = "$trying_to_add_root_page_to_nonempty_website";
                    break;
            }
            /*
            pageTree_DataBind();
            InitializeControls();
            MultiView1.Visible = (pageTree.Nodes.Count != 0); */
        }
    }
}