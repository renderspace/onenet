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
using MsSqlDBUtility;

namespace OneMainWeb.adm
{
    public partial class Website : OneBasePage
    {
        BWebsite websiteB = new BWebsite();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ButtonStartWizard.Visible = ButtonAdd.Visible = Authorization.IsInRole("admin");

                GridViewWebsitesLoad();
                var languages = websiteB.ListLanguages();
                foreach (var language in languages)
                    DropDownList1.Items.Add(new ListItem((new CultureInfo(language)).EnglishName, language.ToString()));
            }
        }

        

        private void GridViewWebsitesLoad()
        {
            var websites = Authorization.ListAllowedWebsites();
            GridViewWebsites.DataSource = websites;
            GridViewWebsites.DataBind();

            if (websites.Count() > 0)
            {
                PlaceHolderTemplates.Visible = true;
                var templates = BWebsite.ListTemplates("3");
                GridViewTemplates.DataSource = templates;
                GridViewTemplates.DataBind();
            }
            else
                PlaceHolderTemplates.Visible = false;

        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            var connString = "";
            if (CheckboxNewDatabase.Checked)
            {
                var weHaveDatabase = false;
                connString = SqlHelper.BuildConnectionString(TextBoxServer.Text, TextBoxDatabaseName.Text, TextBoxUsername.Text, TextBoxPassword.Text);
                var connectivityTest = SqlHelper.CheckDbConnectivity(connString);
                switch (connectivityTest)
                {
                    case SqlHelper.DbConnectivityResult.CantConnect:
                        Notifier1.ExceptionName = "Can't connect to database. Please check connection details.";
                        weHaveDatabase = false;
                        break;
                    case SqlHelper.DbConnectivityResult.NotEmpty:
                        Notifier1.Warning = "Database is not empty, please select another one.";
                        weHaveDatabase = true;
                        break;
                    case SqlHelper.DbConnectivityResult.Empty:
                        weHaveDatabase = true;
                        break;
                    case SqlHelper.DbConnectivityResult.DoesnExist:
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
            website.PreviewUrl = TextBoxPreviewUrl.Text;
            website.ProductionUrl = TextBoxProductionUrl.Text;
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
            LastChangeAndHistory1.SelectedLanguageId = website.LanguageId;
            

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

        protected void LinkButtonAddTemplate_Click(object sender, EventArgs e)
        {

            var templates = BWebsite.ListTemplates("");

            if (templates.Where(t => t.Name.ToLower() == TextBoxTemplate.Text.ToLower()).Count() > 0)
            {
                Notifier1.Warning = "Template with this name already exists";
            } 
            else
            {
                var template = new BOTemplate { Name = TextBoxTemplate.Text, Type = "3" };
                websiteB.ChangeTemplate(template);
                Notifier1.Title = "Template created";
            }
            GridViewWebsitesLoad();
        }

    }
}