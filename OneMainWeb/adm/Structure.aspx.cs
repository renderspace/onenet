using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;
using System.Globalization;
using One.Net.BLL;
using System.Web.UI.HtmlControls;


namespace OneMainWeb.adm
{
    public partial class Structure : OneBasePage
    {
        protected static BWebsite webSiteB = new BWebsite();
        protected static BContent contentB = new BContent();
        protected static BTextContent textContentB = new BTextContent();
        protected static BTextContent specialContentB = new BTextContent();
        BOPage SelectedPage;

        protected void Page_Load(object sender, EventArgs e)
        {
            LiteralLegend.Text = "Selected page";
            LiteralModulesOnPage.Text = "Module instances on current page";


            SelectedWebsite_ValidateDataBind();

            TreeViewPages_DataBind();

            if (!IsPostBack)
            {
                Modules_DataBind();
                Templates_DataBind();
                InitializeControls();   
            }
        }

        protected void SelectedWebsite_ValidateDataBind()
        {
            BOWebSite TempWebSite = webSiteB.Get(SelectedWebSiteId);
            if (SelectedWebSiteId < 1 || TempWebSite == null)
            {
                // if session is lost, we need to select a default website and delete the possibly incorrect SelectedPageId
                SelectedWebSiteId = int.Parse(ConfigurationManager.AppSettings["WebSiteId"].ToString());
                SelectedPageId = -1;
                if (TempWebSite == null)
                {
                    TempWebSite = webSiteB.Get(SelectedWebSiteId);
                }
            }

            if (TempWebSite == null)
            { 
                ResetAllControlsToDefault("No website selected or no website in database");
                return;
            }

            Thread.CurrentThread.CurrentCulture = TempWebSite.Culture;

            RootNodeID = webSiteB.GetRootPageId(SelectedWebSiteId);

            if (!RootNodeID.HasValue)
            {
                ResetAllControlsToDefault("Website doesn't have a root page. Use form on the left to add it.");
                PanelAddSubPage.Visible = true;
                return;
            }

            if (SelectedPageId < 1)
                SelectedPageId = RootNodeID.Value;

            SelectedPage = webSiteB.GetPage(SelectedPageId);

            if (SelectedPage == null || SelectedPage.WebSiteId != SelectedWebSiteId || SelectedPage.LanguageId != Thread.CurrentThread.CurrentCulture.LCID)
            {
                ResetAllControlsToDefault("Please select a page on the left;");
                return;
            }

            if (!SelectedPage.IsEditabledByCurrentPrincipal)
            {
                ResetAllControlsToDefault("You do not have the rights to edit this page. Please select a page on the left.");
            }

            MultiView1.ActiveViewIndex = 0;
            PanelAddSubPage.Visible = true;

            if (RootNodeID.Value == SelectedPageId && !TempWebSite.HasGoogleAnalytics)
            {
                Notifier1.Warning = "Please consider enabling Google Analytics on this website.";
            }
        }

        protected void ResetAllControlsToDefault(string message)
        {
            TreeViewPages.Nodes.Clear();
            TreeViewPages.DataBind();
            RepeaterModuleInstances.DataSource = null;
            RepeaterModuleInstances.DataBind();
            TextBoxUri.Visible = false;
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            TextBoxTitle.Text = "";
            TextBoxSubtitle.Text = "";
            TextBoxDescription.Text = "";
            ButtonPublish.Visible = false;
            ButtonUnPublish.Visible = false;
            ButtonDelete.Visible = false;
            ButtonUndoDelete.Visible = false;
            PanelAddSubPage.Visible = false;
            ButtonPublish.Text = "";
            ImagePageStatus.Visible = false;
            ImagePageStatus.ImageUrl = "";
            LabelUriPart.Text = "";
            TextBoxUri.Text = "";
            TextBoxMenuGroup.Text = "";
            CheckBoxBreakPersitence.Checked = false;
            InputRedirectToUrl1.Text = "";
            cmdMovePageUp.Visible = true;
            cmdMovePageDown.Visible = true;
            OneSettingsPageSettings.ItemId = 0;
            OneSettingsPageSettings.LoadSettings();
            ddlPageTemplate.Items.Clear();

            LabelMessage.Text = message;
            MultiView1.ActiveViewIndex = 1;

            //LabelMessage
        }


        protected void TreeViewPages_Unload(object sender, EventArgs e)
        {
            // save the state of all nodes.
            TreeViewState.SaveTreeView(TreeViewPages, this.GetType().ToString());
        }

        private void TreeViewPages_DataBind()
        {
            int currentExpandLevel = 10;

            if (TreeViewPages.Nodes.Count > 0)
                TreeViewPages.Nodes.Clear();

            TreeNode tree = OneHelper.PopulateTreeViewControl(webSiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, Page, currentExpandLevel);

            if (tree != null)
            {
                TreeViewPages.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeViewPages, this.GetType().ToString());

                TreeViewPages.ExpandDepth = currentExpandLevel;
            }
        }

        

        protected void InitializeControls()
        {
            if (SelectedPage != null)
            {
                cmdMovePageDown.Visible = true;
                cmdMovePageUp.Visible = true;
                TextBoxUri.Visible = SelectedPageId != RootNodeID;
                LastChangeAndHistory1.Text = SelectedPage.DisplayLastChanged;
                LastChangeAndHistory1.SelectedContentId = SelectedPage.ContentId.Value;
                TextBoxTitle.Text = SelectedPage.Title;
                TextBoxSubtitle.Text = SelectedPage.SubTitle;
                TextBoxDescription.Text = SelectedPage.Teaser;
                ButtonPublish.Visible = SelectedPage.IsChanged;
                ButtonUnPublish.Visible = !SelectedPage.IsNew;
                ButtonDelete.Visible = !SelectedPage.MarkedForDeletion;
                ButtonUndoDelete.Visible = SelectedPage.MarkedForDeletion;
                PanelAddSubPage.Visible = PanelAddSubPage.Visible && !SelectedPage.MarkedForDeletion;
                ButtonPublish.Text = SelectedPage.MarkedForDeletion ? "Completely delete page" : "Publish page";
                ImagePageStatus.Visible = true;
                if (SelectedPage.MarkedForDeletion)
                {
                    ImagePageStatus.ImageUrl = "/Res/brisanje.gif";
                }
                else if (SelectedPage.IsChanged)
                {
                    ImagePageStatus.ImageUrl = "/Res/objava.gif";
                }
                else
                {
                    ImagePageStatus.ImageUrl = "/Res/objavljeno.gif";
                }
                LabelUriPart.Text = SelectedPage.ParentURI;
                TextBoxUri.Text = SelectedPage.ParLink;
                TextBoxMenuGroup.Text = SelectedPage.MenuGroup.ToString();
                ddlPageTemplate.ClearSelection();
                ListItem selItem = ddlPageTemplate.Items.FindByValue(SelectedPage.Template.Id.ToString());
                if (selItem != null)
                {
                    selItem.Selected = true;
                }
                CheckBoxBreakPersitence.Checked = SelectedPage.BreakPersistance;
                InputRedirectToUrl1.Text = SelectedPage.RedirectToUrl;
                if (SelectedPage.ParentId.HasValue)
                {
                    List<int> pages = webSiteB.ListChildrenIds(SelectedPage.ParentId.Value);
                    for (int i = 0; i < pages.Count; i++)
                    {
                        if (pages[i] == SelectedPage.Id)
                        {
                            if (i == 0)
                            {
                                cmdMovePageUp.Visible = false;
                            }
                            else if (i == pages.Count - 1)
                            {
                                cmdMovePageDown.Visible = false;
                            }
                        }
                    }
                }
                //                    OneSettingsPageSettings.Settings = SelectedPage.Settings;
                OneSettingsPageSettings.ItemId = SelectedPage.Id;
                OneSettingsPageSettings.Mode = AdminControls.OneSettings.SettingMode.Page;
                OneSettingsPageSettings.LoadSettingsControls(SelectedPage.Settings);
                OneSettingsPageSettings.LoadSettings();
                if (SelectedPageId == RootNodeID)
                {
                    cmdMovePageDown.Visible = false;
                    cmdMovePageUp.Visible = false;
                }
                RepeaterModuleInstances_DataBind();
            }
        }

        private void Modules_DataBind()
        {
            ddlModuleTypes.Items.Clear();
            List<BOModule> modules = BWebsite.ListModules();
            foreach (BOModule module in modules)
            {
                ddlModuleTypes.Items.Add(new ListItem(module.Name, module.Id.ToString()));
            }
        }

        private void Templates_DataBind()
        {
            ddlPageTemplate.Items.Clear();
            ddlPageTemplate.DataSource = BWebsite.ListTemplates("3");
            ddlPageTemplate.DataTextField = "Name";
            ddlPageTemplate.DataValueField = "Id";
            ddlPageTemplate.DataBind();
        }

        protected void RepeaterModuleInstances_DataBind()
        {
            RepeaterModuleInstances.DataSource = webSiteB.ListModuleInstances(SelectedPageId);
            RepeaterModuleInstances.DataBind();
        }

        protected void RepaterModuleInstances_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            
            // for position and inheritance settings
            Control updateButton = e.Item.FindControl("cmdUpdateDetails");
            // for textcontentedit
            var ButtonEdit = e.Item.FindControl("ButtonEdit") as LinkButton;
            Control deleteButton = e.Item.FindControl("cmdDeleteInstance");
            Control undeleteButton = e.Item.FindControl("cmdUndeleteInstance");
            Control cmdMoveUp = e.Item.FindControl("cmdMoveUp");
            Control cmdMoveDown = e.Item.FindControl("cmdMoveDown");
            Label lblPlaceHolder = (Label)(e.Item.FindControl("lblPlaceHolder"));
            Label LabelModuleDistinctName = (Label)(e.Item.FindControl("LabelModuleDistinctName"));
            var PlaceHolderNotInherited1 = (PlaceHolder)(e.Item.FindControl("PlaceHolderNotInherited1"));
            var PlaceHolderNotInherited2 = (PlaceHolder)(e.Item.FindControl("PlaceHolderNotInherited2"));
            
            

            BOModuleInstance moduleInstance = e.Item.DataItem as BOModuleInstance;

            if (moduleInstance != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var HtmlDetails = e.Item.FindControl("HtmlDetails") as HtmlGenericControl;
                var ddlPlaceHolder = e.Item.FindControl("ddlPlaceHolder") as DropDownList;
                var ddlPersistentFromDGrid = (DropDownList)e.Item.FindControl("ddlPersistentFromDGrid");
                var ddlPersistentToDGrid = (DropDownList)e.Item.FindControl("ddlPersistentToDGrid");
                var LiteralInstanceSummary = e.Item.FindControl("LiteralInstanceSummary") as Literal;

                BOPlaceHolder currentPlaceHolder = null;
                if (SelectedPage.PlaceHolders.ContainsKey(moduleInstance.PlaceHolderId))
                    currentPlaceHolder = SelectedPage.PlaceHolders[moduleInstance.PlaceHolderId];
                else
                    return;
                // hide cmdMoveUp/cmdMoveDown, if there is no module instance above/below current instance respectively.
                for (int i = 0; i < currentPlaceHolder.ModuleInstances.Count; i++)
                {
                    if (currentPlaceHolder.ModuleInstances[i].Id == moduleInstance.Id)
                    {
                        if (i == 0)
                        {
                            cmdMoveUp.Visible = false;
                        }
                        if (i == (currentPlaceHolder.ModuleInstances.Count - 1))
                        {
                            cmdMoveDown.Visible = false;
                        }
                    }
                }

                deleteButton.Visible = !moduleInstance.PendingDelete;
                undeleteButton.Visible = moduleInstance.PendingDelete;
                ButtonEdit.Visible = (moduleInstance.Name == "TextContent" || moduleInstance.Name == "SpecialContent") ? (!moduleInstance.IsInherited && !moduleInstance.PendingDelete) : false;
                if (moduleInstance.Name == "TextContent")
                {
                    BOInternalContent textContentModel = textContentB.GetTextContent(moduleInstance.Id);
                    if (textContentModel != null && textContentModel.IsComplete)
                    {
                        LabelModuleDistinctName.Visible = true;

                        var distinctName = StringTool.StripHtmlTags(textContentModel.Title);
                        string postfix = (distinctName.Length >= 20) ? "..." : "";
                        LabelModuleDistinctName.Text = distinctName.Substring(0, distinctName.Length < 20 ? distinctName.Length : 20) + postfix;
                    }
                    else
                    {
                        LabelModuleDistinctName.Visible = true;
                        LabelModuleDistinctName.Text = "$new_tc";
                    }
                }

                var moduleSettings = e.Item.FindControl("moduleSettings") as AdminControls.OneSettings;
                moduleSettings.Visible = !moduleInstance.IsInherited;
                //                    moduleSettings.Settings = moduleInstance.Settings;
                moduleSettings.Mode = AdminControls.OneSettings.SettingMode.Module;
                moduleSettings.ItemId = moduleInstance.Id;
                moduleSettings.LoadSettingsControls(moduleInstance.Settings);
                moduleSettings.LoadSettings();
                PlaceHolderNotInherited1.Visible = PlaceHolderNotInherited2.Visible = !moduleInstance.IsInherited;

                moduleSettings.Visible = moduleSettings.Visible && !moduleInstance.IsInherited;

                if (moduleInstance.IsInherited)
                {
                    LiteralInstanceSummary.Text = "This is inherited module instance.";
                    HtmlDetails.Attributes.Add("class", "nolink");
                }
                else if (moduleInstance.PersistFrom == moduleInstance.PersistTo)
                {
                    LiteralInstanceSummary.Text = "<span>Template position: </span><strong>" + currentPlaceHolder.Name + "</strong>";
                }
                else
                {
                    LiteralInstanceSummary.Text = "<span>Inherited from depth: </span><strong>" + moduleInstance.PersistFrom.ToString() + "</strong> to depth: <strong>" +
                        moduleInstance.PersistTo.ToString() + "</strong>; <span>Template position: </span>" + currentPlaceHolder.Name;
                }

                if (ddlPlaceHolder != null)
                {
                    ddlPlaceHolder.Items.Clear();
                    ddlPlaceHolder.DataSource = webSiteB.ListPlaceHolders();
                    ddlPlaceHolder.DataTextField = "name";
                    ddlPlaceHolder.DataValueField = "id";
                    ddlPlaceHolder.SelectedValue = moduleInstance.PlaceHolderId.ToString();
                    ddlPlaceHolder.DataBind();

                    int maxLevel = 6;
                    ddlPersistentToDGrid.Items.Clear();
                    for (int i = SelectedPage.Level; i <= maxLevel; i++)
                    {
                        ListItem item = new ListItem(i.ToString(), i.ToString());
                        ddlPersistentToDGrid.Items.Add(item);
                        ddlPersistentFromDGrid.Items.Add(item);
                    }
                    ddlPersistentFromDGrid.SelectedValue =  moduleInstance.PersistFrom.ToString();
                    ddlPersistentToDGrid.SelectedValue = moduleInstance.PersistTo.ToString();
                }
            }
        }

        protected void moduleSettings_SettingsSaved(object sender, EventArgs e)
        {
            InitializeControls();
            TreeViewPages_DataBind();
        }

        protected void RepeaterModuleInstances_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int moduleInstanceID = FormatTool.GetInteger(e.CommandArgument);
            if (moduleInstanceID > -1)
            {
                BOModuleInstance moduleInstance = webSiteB.GetModuleInstance(moduleInstanceID);
                BOPage containingPage = webSiteB.GetPage(moduleInstance.PageId);
                BOPlaceHolder placeHolder = null;
                if (containingPage.PlaceHolders.ContainsKey(moduleInstance.PlaceHolderId))
                {
                    placeHolder = containingPage.PlaceHolders[moduleInstance.PlaceHolderId];
                }

                switch (e.CommandName)
                {
                    case "COMMAND_MOVE_UP":
                        {
                            if (placeHolder != null)
                            {
                                BOModuleInstance adjacentInstance = null;
                                for (int i = 0; i < placeHolder.ModuleInstances.Count; i++)
                                {
                                    if (placeHolder.ModuleInstances[i].Id == moduleInstance.Id)
                                    {
                                        if (i > 0 && placeHolder.ModuleInstances[i - 1] != null)
                                        {
                                            adjacentInstance = placeHolder.ModuleInstances[i - 1];
                                            break;
                                        }
                                    }
                                }

                                if (adjacentInstance != null)
                                {
                                    webSiteB.SwapModuleInstances(moduleInstance, adjacentInstance);
                                    InitializeControls();
                                    TreeViewPages_DataBind();
                                }
                            }
                            break;
                        }
                    case "COMMAND_MOVE_DOWN":
                        {
                            if (placeHolder != null)
                            {
                                BOModuleInstance adjacentInstance = null;
                                for (int i = 0; i < placeHolder.ModuleInstances.Count; i++)
                                {
                                    if (placeHolder.ModuleInstances[i].Id == moduleInstance.Id)
                                    {
                                        if (i < (placeHolder.ModuleInstances.Count - 1) && placeHolder.ModuleInstances[i + 1] != null)
                                        {
                                            adjacentInstance = placeHolder.ModuleInstances[i + 1];
                                            break;
                                        }
                                    }
                                }

                                if (adjacentInstance != null)
                                {
                                    webSiteB.SwapModuleInstances(moduleInstance, adjacentInstance);
                                    InitializeControls();
                                    TreeViewPages_DataBind();
                                }
                            }
                            break;
                        }
                    case "COMMAND_EDIT_INSTANCE":
                        {
                            if (moduleInstance.Name == "TextContent")
                            {
                                string redirectTo = Request.ApplicationPath + "adm/ContentEdit.aspx?instanceId=" + moduleInstanceID;
                                Response.Redirect(redirectTo);
                            }
                            else if (moduleInstance.Name == "SpecialContent")
                            {
                                string redirectTo = Request.ApplicationPath + "adm/SpecialContentEdit.aspx?instanceId=" + moduleInstanceID;
                                Response.Redirect(redirectTo);
                            }
                            else
                            {
                                throw new InvalidOperationException("this button shouldn't be visible");
                            }
                            break;
                        }
                    case "COMMAND_UNDELETE":
                        {
                            webSiteB.UnDeleteModuleInstance(moduleInstanceID);
                            InitializeControls();
                            break;
                        }
                    case "COMMAND_DELETE":
                        {
                            webSiteB.DeleteModuleInstance(moduleInstanceID);
                            InitializeControls();
                            break;
                        }
                    case "COMMAND_SAVE_INSTANCE":
                        {
                            DropDownList ddlPlaceHolder = e.Item.FindControl("ddlPlaceHolder") as DropDownList;
                            DropDownList ddlPersistentFromDGrid = (DropDownList)e.Item.FindControl("ddlPersistentFromDGrid");
                            DropDownList ddlPersistentToDGrid = (DropDownList)e.Item.FindControl("ddlPersistentToDGrid");

                            moduleInstance.PersistFrom = FormatTool.GetInteger(ddlPersistentFromDGrid.SelectedValue);
                            moduleInstance.PersistTo = FormatTool.GetInteger(ddlPersistentToDGrid.SelectedValue);
                            moduleInstance.PlaceHolderId = FormatTool.GetInteger(ddlPlaceHolder.SelectedValue);

                            if (moduleInstance.PageId == SelectedPageId && moduleInstance.PlaceHolderId > 0)
                                webSiteB.ChangeModuleInstance(moduleInstance);

                            TreeViewPages_DataBind();
                            break;
                        }
                }
                InitializeControls();
            }
        }

        protected string RenderModuleName(object _Changed, object _PendingDelete, object name, object id)
        {
            string strExtension = "";
            if (FormatTool.GetBoolean(_PendingDelete))
            {
                strExtension = "/Res/brisanje.gif";
            }
            else if (FormatTool.GetBoolean(_Changed))
            {
                strExtension = "/Res/objava.gif";
            }
            else
            {
                strExtension = "/Res/objavljeno.gif";
            }
            strExtension = "<img src='" + strExtension + "' alt='' />";

            return name.ToString() + " " + strExtension + " [" + id.ToString() + "]";
        }

        protected string RenderPageStatus()
        {
            string strReturn = "";
            if (SelectedPage != null)
            {
                if (SelectedPage.MarkedForDeletion)
                {
                    strReturn = "/Res/brisanje.gif";
                }
                else if (SelectedPage.IsChanged)
                {
                    strReturn = "/Res/objava.gif";
                }
                else
                {
                    strReturn = "/Res/objavljeno.gif"; ;
                }
            }
            return strReturn;
        }

        protected void ButtonAddPage_Click(object sender, EventArgs e)
        {
            var result = webSiteB.AddSubPage(TextBoxSubPage.Text, SelectedWebSiteId, SelectedPageId);
            switch (result)
            {
                case BWebsite.AddSubPageResult.Ok:
                    TextBoxSubPage.Text = "";
                    break;
                case BWebsite.AddSubPageResult.NoTemplates:
                    Notifier1.Warning = "$unsucessfull_add_page";
                    Notifier1.Message = "There are no availible templates in the database!";
                    break;
                case BWebsite.AddSubPageResult.OkRootPage:
                    Notifier1.Message = "$label_insert_root_par_link_as_blank";
                    TextBoxSubPage.Text = "";
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

            TreeViewPages_DataBind();
            InitializeControls();
            MultiView1.Visible = (TreeViewPages.Nodes.Count != 0);
        }

        public void TreeViewPages_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedPageId = FormatTool.GetInteger(TreeViewPages.SelectedNode.Value);
            TreeViewPages.CollapseAll();
            ExpandLoop(TreeViewPages.SelectedNode);
            SelectedWebsite_ValidateDataBind();
            InitializeControls();
        }

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        protected void ButtonUndelete_Click(object sender, EventArgs e)
        {
            BOPage currentPageModel = webSiteB.GetPage(SelectedPageId);

            if (currentPageModel != null && currentPageModel.MarkedForDeletion)
            {
                webSiteB.UndeletePage(SelectedPageId);
                TreeViewPages_DataBind();
                InitializeControls();
            }
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            var result = webSiteB.DeletePageById(SelectedPageId);
            switch (result)
            {
                case BWebsite.DeletePageByIdResult.DeletedRoot:
                    SelectedPageId = -1;
                    RootNodeID = null;
                    TreeViewPages_DataBind();
                    InitializeControls();
                    break;
                case BWebsite.DeletePageByIdResult.Deleted:
                    TreeViewPages_DataBind();
                    InitializeControls();
                    break;
                case BWebsite.DeletePageByIdResult.HasChildren:
                    Notifier1.Warning = "$has_children_delete_not_possible";
                    break;
                case BWebsite.DeletePageByIdResult.Error:
                    Notifier1.Warning = "$delete_page_error";
                    break;
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            BOPage page = webSiteB.GetPage(SelectedPageId);

            if (page != null)
            {
                bool breakPersistance = CheckBoxBreakPersitence.Checked;
                string redirectToUrl = InputRedirectToUrl1.Text;
                int menuGroupID = FormatTool.GetInteger(TextBoxMenuGroup.Text);
                int selectedTemplateID = FormatTool.GetInteger(ddlPageTemplate.SelectedValue);

                if (!page.IsRoot)
                {
                    int parentPageID = page.ParentId.Value;
                    string newParLink = BWebsite.CleanStringForUrl(TextBoxUri.Text);

                    // check whether new par_link already exists in system
                    bool validParLink = webSiteB.ValidateParLinkAgainstDB(parentPageID, SelectedPageId, newParLink, SelectedWebSiteId);

                    if (validParLink || page.ParLink == newParLink || SelectedPageId == parentPageID)
                    {
                        page.MenuGroup = menuGroupID;
                        page.Template = new BOTemplate { Id = selectedTemplateID };
                        page.Title = TextBoxTitle.Text;
                        page.SubTitle = TextBoxSubtitle.Text;
                        page.Teaser = TextBoxDescription.Text;
                        page.ParLink = newParLink;
                        page.BreakPersistance = breakPersistance;
                        page.RedirectToUrl = redirectToUrl;
                        webSiteB.ChangePage(page);
                    }
                    else if (!validParLink)
                    {
                        Notifier1.Warning = "This URL cannot be updated because the URL already exists at this level for this parent page!";
                    }
                }
                else  // page.IsRoot
                {
                    var site = webSiteB.Get(SelectedWebSiteId);

                    page.MenuGroup = menuGroupID;
                    page.Template = new BOTemplate { Id = selectedTemplateID };
                    page.Title = TextBoxTitle.Text;
                    page.SubTitle = TextBoxSubtitle.Text;
                    page.Teaser = TextBoxDescription.Text;
                    page.ParLink = ""; // page.IsRoot
                    page.BreakPersistance = breakPersistance;
                    page.RedirectToUrl = redirectToUrl;
                    webSiteB.ChangePage(page);
                }
                OneSettingsPageSettings.Save();
                InitializeControls();
                TreeViewPages_DataBind();
            }
            else
            {
                Notifier1.Title = "Selected Pages doesn't exist, or no page is selected";
            }
        }

        protected void CmdMovePage_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            SelectedPage = webSiteB.GetPage(SelectedPageId);
            if (SelectedPage != null && SelectedPage.ParentId.HasValue)
            {
                List<int> pages = webSiteB.ListChildrenIds(SelectedPage.ParentId.Value);
                if (button.CommandName == "Up")
                {
                    for (int i = 0; i < pages.Count; i++)
                    {
                        if (i > 0 && pages[i] == SelectedPageId)
                        {
                            webSiteB.SwapOrderOfPages(pages[i - 1], pages[i]);
                            break;
                        }
                    }
                }
                else if (button.CommandName == "Down")
                {
                    for (int i = 0; i < pages.Count; i++)
                    {
                        if (i + 1 < pages.Count && pages[i] == SelectedPageId)
                        {
                            webSiteB.SwapOrderOfPages(pages[i + 1], pages[i]);
                            break;
                        }
                    }
                }
                TreeViewPages_DataBind();
                InitializeControls();
            }
        }

        protected void cmdAddInstance_Click(object sender, EventArgs e)
        {
            int selectedModuleID = FormatTool.GetInteger(ddlModuleTypes.SelectedItem.Value);
            List<BOPlaceHolder> placeHolderData = webSiteB.ListPlaceHolders();
            if (placeHolderData.Count < 1)
                return;

            int selectedPlaceHolder = placeHolderData[0].Id.Value;
            var result = webSiteB.AddModulesInstance(SelectedPageId, selectedPlaceHolder, selectedModuleID);
            if (result)
            {
                InitializeControls();
                TreeViewPages_DataBind();
            }
        }

        protected void ButtonPublish_Click(object sender, EventArgs e)
        {
            BOPage publishingPage = webSiteB.GetPage(SelectedPageId);

            if (publishingPage != null)
            {
                if (!publishingPage.IsRoot)
                {
                    BOPage parentPage = webSiteB.GetPage(publishingPage.ParentId.Value);
                    if (parentPage.IsNew)
                    {
                        Notifier1.Warning = "Please publish parent page before trying to publish this one.";
                        Notifier1.Message = "Page was not published, try again after you published the page above";
                        return;
                    }
                }

                if (webSiteB.PublishPage(SelectedPageId))
                {
                    if (publishingPage.MarkedForDeletion && !publishingPage.IsRoot)
                    {
                        SelectedPageId = publishingPage.ParentId.Value;
                        Notifier1.Message = "Page was successfully completely deleted.";
                    }
                    else if (publishingPage.MarkedForDeletion)
                    {
                        SelectedPageId = -1;
                        Notifier1.Message = "Root page was successfully completely deleted.";
                    }
                    else
                    {
                        Notifier1.Message = "Publish sucessfull.";
                    }
                }
                else
                {
                    Notifier1.Warning = "Unknown error while publishing.";
                }
            }
            else
            {
                Notifier1.ExceptionName = "Trying to publish nonexisting page.";
            }
            InitializeControls();
            TreeViewPages_DataBind();
        }

        protected void ButtonUnPublish_Click(object sender, EventArgs e)
        {
            BOPage publishingPage = webSiteB.GetPage(SelectedPageId);

            if (!publishingPage.IsNew)
            {
                bool hasChildren = webSiteB.ListChildrenIds(SelectedPageId).Count > 0;
                if (hasChildren)
                {
                    Notifier1.Warning = "Unpublish was not successful because the page has children.";
                    Notifier1.Message = "Unpublish children pages first.";
                    return;
                }


                if (webSiteB.UnPublishPage(SelectedPageId))
                {
                    Notifier1.Message = "$unpublish_successfull";
                }
                else
                {
                    Notifier1.Warning = "$unpublish_unsuccessfull";
                }
            }
            else
            {
                Notifier1.ExceptionName = "$page_never_published";
            }
            InitializeControls();
            TreeViewPages_DataBind();
        }
    }

    public class TreeViewState
    {
        public static void SaveTreeView(TreeView treeView, string key)
        {
            List<bool?> list = new List<bool?>();
            SaveTreeViewExpandedState(treeView.Nodes, list);
            HttpContext.Current.Session[key + treeView.ID] = list;
        }

        private static void SaveTreeViewExpandedState(TreeNodeCollection nodes, List<bool?> list)
        {
            foreach (TreeNode node in nodes)
            {
                list.Add(node.Expanded);
                if (node.ChildNodes.Count > 0)
                    SaveTreeViewExpandedState(node.ChildNodes, list);
            }
        }

        private int RestoreTreeViewIndex;

        public void RestoreTreeView(TreeView treeView, string key)
        {
            RestoreTreeViewIndex = 0;
            RestoreTreeViewExpandedState(treeView.Nodes,
                (List<bool?>)HttpContext.Current.Session[key + treeView.ID] ?? new List<bool?>());
        }

        private void RestoreTreeViewExpandedState(TreeNodeCollection nodes, List<bool?> list)
        {
            foreach (TreeNode node in nodes)
            {
                if (RestoreTreeViewIndex >= list.Count) return;

                node.Expanded = list[RestoreTreeViewIndex++];

                if (node.ChildNodes.Count > 0)
                    RestoreTreeViewExpandedState(node.ChildNodes, list);
            }
        }

        public static void ExpandTreeView(TreeView treeView, bool expand)
        {
            ExpandTreeViewState(treeView.Nodes, expand);
        }

        private static void ExpandTreeViewState(TreeNodeCollection nodes, bool expand)
        {
            foreach (TreeNode node in nodes)
            {
                node.Expanded = expand;
                if (node.ChildNodes.Count > 0)
                    ExpandTreeViewState(node.ChildNodes, expand);
            }
        }
    }
}
