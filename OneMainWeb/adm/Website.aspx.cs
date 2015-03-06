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
                MultiView1.ActiveViewIndex = 0;
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

                var placeholders = BWebsite.ListPlaceHolders();
                GridViewPlaceholders.DataSource = placeholders;
                GridViewPlaceholders.DataBind();   
            }
            else
            {
                // no websites, go straight to add feature
                if (ButtonAdd.Visible)
                {
                    MultiView1.ActiveViewIndex = 1;
                }
                PlaceHolderTemplates.Visible = false;
            }
        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            var connString = "";
            if (!PanelEmptyDatabase.Visible) // we have an empty database
            { 
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

            BWebsite.AddWebSiteResult result = BWebsite.AddWebSiteResult.Error;
            if (PanelEmptyDatabase.Visible || CheckboxManualCopy.Checked) // we have an empty database
            {
                websiteB.ChangeWebsite(website);
                result = BWebsite.AddWebSiteResult.Success;
            }
            else 
            { 
                result = websiteB.AddWebSite(website, CheckboxNewDatabase.Checked, new DirectoryInfo(Server.MapPath("~")), connString);
            }

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
                    PlaceholderNewDatabase.Visible = true;
                    PanelEmptyDatabase.Visible = false;
                    InputTitle.Text = "";
                    TextBoxPreviewUrl.Text = "";
                    TextBoxProductionUrl.Text = "";
                    if (Authorization.ListAllowedWebsites().Count() == 0)
                    {
                        PlaceholderNewDatabase.Visible = false;
                        PanelEmptyDatabase.Visible = true;
                    }
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
            LabelPreviewUrl.Text = website.PreviewUrl;
            LabelProductionUrl.Text = website.ProductionUrl;
            LastChangeAndHistory1.Text = website.DisplayLastChanged;
            LastChangeAndHistory1.SelectedContentId = website.ContentId.Value;
            LastChangeAndHistory1.SelectedLanguageId = website.LanguageId;
            

            OneSettingsWebsite.ItemId = websiteId;
            OneSettingsWebsite.Mode = AdminControls.OneSettings.SettingMode.Website;
            OneSettingsWebsite.LoadSettingsControls(website.Settings);
            OneSettingsWebsite.Databind();
        }

        protected void GridViewTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            var templateId = int.Parse(GridViewTemplates.SelectedValue.ToString());
            LabelTemplateId.Text = templateId.ToString();
            var template = BWebsite.GetTemplate(templateId);

            if (template == null)
            {
                MultiView1.ActiveViewIndex = 0;
                Notifier1.ExceptionName = "Template doesn't exist.";
                return;
            }

            LabelTemplateId.Text = template.Id.Value.ToString();
            TextBoxTemplateName.Text = template.Name;
            TextBoxTemplateType.Text = template.Type;
            TextBoxTemplateContent.Text = template.TemplateContent;

            MultiView1.ActiveViewIndex = 3;
        }

        protected void ButtonSaveTemplate_Click(object sender, EventArgs e)
        {
            var templateId = int.Parse(LabelTemplateId.Text);

            var template = BWebsite.GetTemplate(templateId);

            if (template == null)
            {
                MultiView1.ActiveViewIndex = 0;
                Notifier1.ExceptionName = "Template doesn't exist.";
                return;
            }

            template.Name = TextBoxTemplateName.Text;
            template.Type = TextBoxTemplateType.Text;
            template.TemplateContent = TextBoxTemplateContent.Text;

            websiteB.ChangeTemplate(template);

            MultiView1.ActiveViewIndex = 0;
            Notifier1.Title = "Saved.";
        }

        protected void GridViewTemplates_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null)
            {
                var cmdDelete = grid.Rows[e.RowIndex].FindControl("cmdDelete") as LinkButton;
                if (cmdDelete != null) {
                    var templateId = Int32.Parse(cmdDelete.CommandArgument.ToString());
                    var template = BWebsite.GetTemplate(templateId);

                    if (template == null)
                    {
                        MultiView1.ActiveViewIndex = 0;
                        Notifier1.ExceptionName = "Template doesn't exist.";
                        return;
                    }

                    websiteB.DeleteTemplate(templateId);

                    MultiView1.ActiveViewIndex = 0;
                    Notifier1.ExceptionName = "Template successfully deleted.";
                }
                
            }
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
            websiteB.ChangeWebsite(website);
            OneSettingsWebsite.Save();
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

        protected void LinkButtonAddPlaceholder_Click(object sender, EventArgs e)
        {
            var placeholders = BWebsite.ListPlaceHolders();

            if (placeholders.Where(t => t.Name.ToLower() == TextBoxPlaceholder.Text.ToLower()).Count() > 0)
            {
                Notifier1.Warning = "Placeholder with this name already exists";
            }
            else
            {
                var placeholder = new BOPlaceHolder { Name = TextBoxPlaceholder.Text  };
                websiteB.ChangePlaceHolder(placeholder);
                Notifier1.Title = "Placeholder created";
            }
            GridViewWebsitesLoad();
        }
    }
}