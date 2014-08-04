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
using System.IO;

namespace OneMainWeb.adm
{
    public partial class Website : OneBasePage
    {
        BWebsite websiteB = new BWebsite();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GridViewWebsitesLoad();
                var languages = websiteB.ListLanguages();
                foreach (var language in languages)
                    DropDownList1.Items.Add(new ListItem((new CultureInfo(language)).EnglishName, language.ToString()));
            }
        }

        

        private void GridViewWebsitesLoad()
        {
            GridViewWebsites.DataSource = websiteB.List();
            GridViewWebsites.DataBind();
        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            var website = new BOWebSite();
            website.Title = InputTitle.Text;
            website.SubTitle = "";
            website.Teaser = "";
            website.Html = "";
            website.LanguageId = Int32.Parse(DropDownList1.SelectedValue);
            website.ContentId = null;
            website.PrincipalCreated = User.Identity.Name;
            website.PreviewUrl = TextBoxPreviewUrl.Text;

            var testConnString = "server=tartar.netinet.si;uid=zrsz;pwd=zrsz;database=test2;Pooling=true;";

            var result = websiteB.AddWebSite(website, CheckboxNewDatabase.Checked, new DirectoryInfo(Server.MapPath("~")), testConnString);
            if (result == BWebsite.AddWebSiteResult.Success)
            {
                Notifier1.Title = "Created";
                MultiView1.ActiveViewIndex = 0;
            }
            else
            {
                Notifier1.ExceptionName = "Failed";
                Notifier1.ExceptionMessage = result.ToString();
            }
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
                    GridViewWebsitesLoad();
                    break;
                case 1:
                    

                    break;
            }
        }

    }
}