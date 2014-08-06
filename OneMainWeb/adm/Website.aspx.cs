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
            return;
            var connString = "";
            if (CheckboxNewDatabase.Checked)
            {
                var weHaveDatabase = false;
                connString = DatabaseHelper.BuildConnectionString(TextBoxServer.Text, TextBoxDatabaseName.Text, TextBoxUsername.Text, TextBoxPassword.Text);
                var connectivityTest = DatabaseHelper.CheckDbConnectivity(connString);
                switch (connectivityTest)
                { 
                    case DatabaseHelper.DbConnectivityResult.CantConnect:
                        Notifier1.ExceptionName = "Can't connect to database. Please check connection details.";
                        weHaveDatabase = false;
                        break;
                    case DatabaseHelper.DbConnectivityResult.NotEmpty:
                        Notifier1.ExceptionName = "Database is not empty, please select another one.";
                        weHaveDatabase = false;
                        break;
                    case DatabaseHelper.DbConnectivityResult.Empty:
                        weHaveDatabase = true;
                        break;
                    case DatabaseHelper.DbConnectivityResult.DoesnExist:
                        weHaveDatabase = websiteB.CreateNewDatabase(connString);
                        if (!weHaveDatabase)
                            Notifier1.ExceptionName = "Can't create database.";
                        break;
                }
                if (!weHaveDatabase)
                {
                    return;
                }
            }

            var website = new BOWebSite();
            website.Title = InputTitle.Text;
            website.SubTitle = "";
            website.Teaser = "";
            website.Html = "";
            website.LanguageId = Int32.Parse(DropDownList1.SelectedValue);
            website.ContentId = null;
            website.PrincipalCreated = User.Identity.Name;
            website.PreviewUrl = TextBoxPreviewUrl.Text;
            var result = websiteB.AddWebSite(website, CheckboxNewDatabase.Checked, new DirectoryInfo(Server.MapPath("~")), connString);
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

        protected void GridViewWebsites_SelectedIndexChanged(object sender, EventArgs e)
        {
            var websiteId = int.Parse(GridViewWebsites.SelectedValue.ToString());
            LabelID.Text = websiteId.ToString();
            var website = websiteB.Get(websiteId);

            if (website == null)
            {
                return;
            }

            MultiView1.ActiveViewIndex = 2;
            LabelLanguage.Text = website.Culture.EnglishName;
            TextBoxTitle.Text = website.Title;
            TextBoxDescription.Text = website.Teaser;
            TextBoxOgImage.Text = website.SubTitle;
            LastChangeAndHistory1.Text = website.DisplayLastChanged;
            LastChangeAndHistory1.SelectedContentId = website.ContentId.Value;
            

            OneSettingsWebsite.ItemId = websiteId;
            OneSettingsWebsite.Mode = AdminControls.OneSettings.SettingMode.Website;
            OneSettingsWebsite.LoadSettingsControls(website.Settings);
            OneSettingsWebsite.LoadSettings();
            

        }

        protected void ButtonCancel_Click(object sender, EventArgs e)
        {
            LabelLanguage.Text = "";
            TextBoxTitle.Text = "";
            TextBoxDescription.Text = "";
            TextBoxOgImage.Text = "";
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            OneSettingsWebsite.ItemId = 0;
            MultiView1.ActiveViewIndex = 0;
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {

            var websiteId = int.Parse(LabelID.Text);
            var website = websiteB.Get(websiteId);
            if (website == null)
            {
                MultiView1.ActiveViewIndex = 0;
                Notifier1.ExceptionName = "Website doesn't exist.";
                return;
            }

            website.Title = TextBoxTitle.Text;        
            website.Teaser = TextBoxDescription.Text;
            website.SubTitle = TextBoxOgImage.Text;
            website.Html = "";
            OneSettingsWebsite.Save();
            websiteB.ChangeWebsite(website);
            MultiView1.ActiveViewIndex = 0;
            Notifier1.Title = "Saved.";

        }

    }
}