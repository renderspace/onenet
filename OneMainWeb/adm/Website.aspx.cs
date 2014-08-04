using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using OneMainWeb.AdminControls;


using One.Net.BLL.Utility;
using System.Globalization;

namespace OneMainWeb.adm
{
    public partial class Website : OneBasePage
    {
        BWebsite websiteB = new BWebsite();

        private List<BOWebSite> Websites
        {
            get { return ViewState["Websites"] != null ? ViewState["Websites"] as List<BOWebSite> : null; }
            set { ViewState["Websites"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var languages = websiteB.ListLanguages();
                foreach (var language in languages)
                    DropDownList1.Items.Add(new ListItem((new CultureInfo(language)).EnglishName, language.ToString()));
            }
        }

        

        private void GridViewWebsitesLoad(bool reloadData)
        {
            if (Websites == null || reloadData)
                Websites = websiteB.List();
            GridViewWebsites.DataSource = Websites;
            GridViewWebsites.DataBind();
        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            var title = InputTitle.Text;
            var lcid = Int32.Parse(DropDownList1.SelectedValue);

            var website = new BOWebSite();
            website.Title = title;
            website.SubTitle = "";
            website.Teaser = "";
            website.Html = "";
            website.LanguageId = lcid;
            website.ContentId = null;
            website.PrincipalCreated = User.Identity.Name;
            websiteB.ChangeWebsite(website);
            MultiView1.ActiveViewIndex = 0;
        }

        protected void ButtonStartWizard_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 1;
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {
            switch (MultiView1.ActiveViewIndex)
            { 
                case 0:
                    GridViewWebsitesLoad(true);
                    
                    break;
                case 1:
                    

                    break;
            }
        }

    }
}