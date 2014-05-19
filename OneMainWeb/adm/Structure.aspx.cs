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


namespace OneMainWeb.adm
{
    public partial class Structure : OneBasePage
    {
        protected static BWebsite webSiteB = new BWebsite();
        protected static BContent contentB = new BContent();
        protected static BTextContent textContentB = new BTextContent();
        protected static BTextContent specialContentB = new BTextContent();

        private BOPage selectedPage;

        protected void Page_Load(object sender, EventArgs e)
        {
            LiteralLegend.Text = ResourceManager.GetString("$page_info");
            LiteralNoAccess.Text = ResourceManager.GetString("$no_rights");
            LiteralModulesOnPage.Text = ResourceManager.GetString("$modules_on_page");

            pageTree_DataBind();

            if (!IsPostBack)
            {
                Page.Title = ResourceManager.GetString("$module_structure");
                ddlModuleTypes_DataBind();
                InitializeControls();
            }
        }

        protected void pageTree_Unload(object sender, EventArgs e)
        {
            // save the state of all nodes.
            TreeViewState.SaveTreeView(pageTree, this.GetType().ToString());
        }

        private void pageTree_DataBind()
        {
            int currentExpandLevel = 10;

            if (pageTree.Nodes.Count > 0)
                pageTree.Nodes.Clear();

            TreeNode tree = OneHelper.PopulateTreeViewControl(webSiteB.GetSiteStructure(SelectedWebSiteId), null, SelectedPageId, Page, currentExpandLevel);

            if (tree != null)
            {
                pageTree.Nodes.Add(tree);

                // get the saved state of all nodes.
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(pageTree, this.GetType().ToString());

                pageTree.ExpandDepth = currentExpandLevel;
            }
        }

        protected void InitializeControls()
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

            // fix redirect problem when no pages are present in db
            if (TempWebSite != null && TempWebSite.Languages.Count == 0)
            {
                TempWebSite.Languages.Add(TempWebSite.PrimaryLanguageId);
            }

            if (TempWebSite != null && !TempWebSite.Languages.Contains(SelectedCultureId))
            {
                SelectedCultureId = TempWebSite.PrimaryLanguageId;
                SelectedCulture = (new CultureInfo(SelectedCultureId)).Name;
                log.Info("redirecting from TempWebSite.Languages.Contains");
                Response.Redirect(Request.Url.LocalPath);
            }

            RootNodeID = webSiteB.GetRootPageId(SelectedWebSiteId);

            if (TempWebSite == null)
            {
                // no website selected has no root
                MultiView1.ActiveViewIndex = 3;
                PanelAddSubPage.Visible = false;
                return;
            }
            else if (!RootNodeID.HasValue)
            {
                // selected website has no root
                MultiView1.ActiveViewIndex = 2;
                PanelAddSubPage.Visible = true;
                return;
            }
            else
            {
                MultiView1.ActiveViewIndex = 1;
                PanelAddSubPage.Visible = true;
            }

            if (SelectedPageId < 1)
            {
                SelectedPageId = RootNodeID.Value;
                pageTree_DataBind();
            }

            selectedPage = webSiteB.GetPage(SelectedPageId);

            if (selectedPage != null)
            {
                cmdMovePageDown.Visible = true;
                cmdMovePageUp.Visible = true;

                MultiView1.ActiveViewIndex = 1;
                if (!selectedPage.IsEditabledByCurrentPrincipal)
                {
                    MultiView1.ActiveViewIndex = 0;
                    PanelAddSubPage.Visible = false;
                }
                else if (selectedPage.HasTranslationInCurrentLanguage)
                {
                    BOWebSite webSite = webSiteB.Get(SelectedWebSiteId);
                    TextBoxUri.Visible = SelectedPageId != RootNodeID;

                    LabelChanged.Text = selectedPage.LastChanged + ", " + selectedPage.LastChangedBy;
                    TwoInputTitle.Value = selectedPage.Title;
                    ButtonPublish.Visible = selectedPage.IsChanged;
                    ButtonUnPublish.Visible = !selectedPage.IsNew;
                    ButtonDelete.Visible = !selectedPage.MarkedForDeletion;
                    ButtonUndoDelete.Visible = selectedPage.MarkedForDeletion;

                    PanelAddSubPage.Visible = PanelAddSubPage.Visible && !selectedPage.MarkedForDeletion;

                    ButtonPublish.Text = selectedPage.MarkedForDeletion ? "$publish_delete" : "$publish";
                    ButtonUnPublish.Text = "$unpublish_page";
                    ImagePageStatus.Visible = true;
                    if (selectedPage.MarkedForDeletion)
                    {
                        ImagePageStatus.ImageUrl = "/Res/brisanje.gif";
                    }
                    else if (selectedPage.IsChanged)
                    {
                        ImagePageStatus.ImageUrl = "/Res/objava.gif";
                    }
                    else
                    {
                        ImagePageStatus.ImageUrl = "/Res/objavljeno.gif";
                    }

                    LabelUriPart.Text = selectedPage.ParentURI;
                    TextBoxUri.Text = selectedPage.ParLink;

                    DropDownListMenuGroups.Items.Clear();
                    PanelMenuGroups.Visible = false;
                    TwoInputMenuGroup.Visible = true;

                    TwoInputMenuGroup.Value = selectedPage.MenuGroup.ToString();
                    BOSetting MenuGroupListSetting = null;
                    if (webSite.Settings.TryGetValue("MenuGroupList", out MenuGroupListSetting))
                    {
                        List<int> menugroupIds = StringTool.SplitStringToIntegers(MenuGroupListSetting.Value);
                        if (menugroupIds.Count > 0)
                        {
                            PanelMenuGroups.Visible = true;
                            TwoInputMenuGroup.Visible = false;
                            DropDownListMenuGroups.DataSource = menugroupIds;
                            DropDownListMenuGroups.DataBind();

                            if (menugroupIds.Contains(selectedPage.MenuGroup))
                                DropDownListMenuGroups.SelectedValue = selectedPage.MenuGroup.ToString();
                        }
                    }
                    ddlPageTemplate_DataBind();

                    ListItem selItem = ddlPageTemplate.Items.FindByValue(selectedPage.Template.Id.ToString());
                    if (selItem != null)
                    {
                        selItem.Selected = true;
                    }
                    LabeledCheckBoxBreakPersistance.Checked = selectedPage.BreakPersistance;

                    InputRedirectToUrl.Value = selectedPage.RedirectToUrl;

                    if (selectedPage.ParentId.HasValue)
                    {
                        List<int> pages = webSiteB.ListChildrenIds(selectedPage.ParentId.Value);
                        for (int i = 0; i < pages.Count; i++)
                        {
                            if (pages[i] == selectedPage.Id)
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

                    //                    OneSettingsPageSettings.Settings = selectedPage.Settings;
                    OneSettingsPageSettings.ItemId = selectedPage.Id;
                    OneSettingsPageSettings.Mode = AdminControls.OneSettings.SettingMode.Page;
                    OneSettingsPageSettings.LoadSettingsControls(selectedPage.Settings);
                    OneSettingsPageSettings.LoadSettings();

                    if (SelectedPageId == RootNodeID)
                    {
                        cmdMovePageDown.Visible = false;
                        cmdMovePageUp.Visible = false;
                    }

                    RepeaterModuleInstances_DataBind();
                }
            }
        }

        private void ddlModuleTypes_DataBind()
        {
            ddlModuleTypes.Items.Clear();
            List<BOModule> modules = BWebsite.ListModules();
            foreach (BOModule module in modules)
            {
                ddlModuleTypes.Items.Add(new ListItem(ResourceManager.GetString("$" + module.Name), module.Id.ToString()));
            }
        }

        private void ddlPageTemplate_DataBind()
        {
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
            Control editButton = e.Item.FindControl("cmdEdit");
            Control deleteButton = e.Item.FindControl("cmdDeleteInstance");
            Control undeleteButton = e.Item.FindControl("cmdUndeleteInstance");
            Control cmdMoveUp = e.Item.FindControl("cmdMoveUp");
            Control cmdMoveDown = e.Item.FindControl("cmdMoveDown");
            Label lblPlaceHolder = (Label)(e.Item.FindControl("lblPlaceHolder"));
            Label LabelModuleDistinctName = (Label)(e.Item.FindControl("LabelModuleDistinctName"));
            BOModuleInstance moduleInstance = e.Item.DataItem as BOModuleInstance;

            if (moduleInstance != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                BOPlaceHolder currentPlaceHolder = null;
                if (selectedPage.PlaceHolders.ContainsKey(moduleInstance.PlaceHolderId))
                    currentPlaceHolder = selectedPage.PlaceHolders[moduleInstance.PlaceHolderId];
                else
                    return;

                cmdMoveUp.Visible = !moduleInstance.IsInherited;
                cmdMoveDown.Visible = !moduleInstance.IsInherited;

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

                deleteButton.Visible = !moduleInstance.IsInherited && !moduleInstance.PendingDelete;
                undeleteButton.Visible = moduleInstance.PendingDelete && !moduleInstance.IsInherited;
                editButton.Visible = (moduleInstance.Name == "TextContent" || moduleInstance.Name == "SpecialContent") ? (!moduleInstance.IsInherited && !moduleInstance.PendingDelete) : false;
                if (!moduleInstance.IsInherited && moduleInstance.Name == "TextContent")
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

                AdminControls.OneSettings moduleSettings = e.Item.FindControl("moduleSettings") as AdminControls.OneSettings;

                if (moduleSettings != null)
                {
                    moduleSettings.Visible = !moduleInstance.IsInherited;
                    //                    moduleSettings.Settings = moduleInstance.Settings;
                    moduleSettings.Mode = AdminControls.OneSettings.SettingMode.Module;
                    moduleSettings.ItemId = moduleInstance.Id;
                    moduleSettings.LoadSettingsControls(moduleInstance.Settings);
                    moduleSettings.LoadSettings();
                }

                lblPlaceHolder.Text = "$" + currentPlaceHolder.Name;
                DropDownList ddlPlaceHolder = e.Item.FindControl("ddlPlaceHolder") as DropDownList;
                DropDownList ddlPersistentFromDGrid = (DropDownList)e.Item.FindControl("ddlPersistentFromDGrid");
                DropDownList ddlPersistentToDGrid = (DropDownList)e.Item.FindControl("ddlPersistentToDGrid");
                Label lblPersistentFromDGrid = (Label)e.Item.FindControl("lblPersistentFromDGrid");
                Label lblPersistentToDGrid = (Label)e.Item.FindControl("lblPersistentToDGrid");

                if (ddlPlaceHolder != null)
                {
                    ddlPlaceHolder.DataSource = webSiteB.ListPlaceHolders();
                    ddlPlaceHolder.DataTextField = "name";
                    ddlPlaceHolder.DataValueField = "id";
                    ddlPlaceHolder.SelectedValue = moduleInstance.PlaceHolderId.ToString();
                    ddlPlaceHolder.DataBind();

                    int maxLevel = 6;
                    ddlPersistentToDGrid.Items.Clear();
                    for (int i = selectedPage.Level; i <= maxLevel; i++)
                    {
                        ListItem item = new ListItem(i.ToString(), i.ToString());
                        ddlPersistentToDGrid.Items.Add(item);
                        ddlPersistentFromDGrid.Items.Add(item);
                    }
                    ddlPersistentFromDGrid.SelectedValue = lblPersistentFromDGrid.Text = moduleInstance.PersistFrom.ToString();
                    ddlPersistentToDGrid.SelectedValue = lblPersistentToDGrid.Text = moduleInstance.PersistTo.ToString();
                }
            }
        }

        protected void moduleSettings_SettingsSaved(object sender, EventArgs e)
        {
            pageTree_DataBind();
            InitializeControls();
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
                                    pageTree_DataBind();
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
                                    pageTree_DataBind();
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

                            pageTree_DataBind();
                            break;
                        }
                }
                InitializeControls();
            }
        }



        protected string RenderModuleName(object _Changed, object _PendingDelete, object name, object id)
        {
            string strExtension = string.Empty;
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

            return ResourceManager.GetString("$" + name.ToString()) + " " + strExtension + " [" + id.ToString() + "]";
        }

        protected string RenderPageStatus()
        {
            string strReturn = string.Empty;

            BOPage page = webSiteB.GetPage(SelectedPageId);
            if (page != null)
            {
                if (page.MarkedForDeletion)
                {
                    strReturn = "/Res/brisanje.gif";
                }
                else if (page.IsChanged)
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


        protected void cmdAddChild_Click(object sender, EventArgs e)
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

            pageTree_DataBind();
            InitializeControls();
            MultiView1.Visible = (pageTree.Nodes.Count != 0);
        }

        public void pageTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedPageId = FormatTool.GetInteger(pageTree.SelectedNode.Value);
            pageTree.CollapseAll();
            ExpandLoop(pageTree.SelectedNode);
            InitializeControls();
        }

        private static void ExpandLoop(TreeNode node)
        {
            node.Expand();
            if (node.Parent != null)
                ExpandLoop(node.Parent);
        }

        protected void cmdUnDelete_Click(object sender, EventArgs e)
        {
            BOPage currentPageModel = webSiteB.GetPage(SelectedPageId);

            if (currentPageModel != null && currentPageModel.MarkedForDeletion)
            {
                webSiteB.UndeletePage(SelectedPageId);
                pageTree_DataBind();
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
                    pageTree_DataBind();
                    InitializeControls();
                    break;
                case BWebsite.DeletePageByIdResult.Deleted:
                    pageTree_DataBind();
                    InitializeControls();
                    break;
                case BWebsite.DeletePageByIdResult.HasChildren:
                    Notifier1.Warning = ResourceManager.GetString("$has_children_delete_not_possible");
                    break;
                case BWebsite.DeletePageByIdResult.Error:
                    Notifier1.Warning = ResourceManager.GetString("$delete_page_error");
                    break;
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            BOPage page = webSiteB.GetPage(SelectedPageId);

            if (page != null)
            {
                bool breakPersistance = LabeledCheckBoxBreakPersistance.Checked;
                string redirectToUrl = InputRedirectToUrl.Value;
                int menuGroupID = 0;

                if (PanelMenuGroups.Visible && DropDownListMenuGroups.Items.Count > 0)
                    menuGroupID = FormatTool.GetInteger(DropDownListMenuGroups.SelectedValue);
                else
                    menuGroupID = FormatTool.GetInteger(TwoInputMenuGroup.Value);
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
                        page.Title = TwoInputTitle.Value;
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
                    page.Title = TwoInputTitle.Value;
                    page.ParLink = ""; // page.IsRoot
                    page.BreakPersistance = breakPersistance;
                    page.RedirectToUrl = redirectToUrl;
                    webSiteB.ChangePage(page);
                }
                OneSettingsPageSettings.Save();
                InitializeControls();
                pageTree_DataBind();
            }
            else
            {
                Notifier1.Title = "Selected Pages doesn't exist, or no page is selected";
            }
        }

        protected void CmdMovePage_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            selectedPage = webSiteB.GetPage(SelectedPageId);
            if (selectedPage != null && selectedPage.ParentId.HasValue)
            {
                List<int> pages = webSiteB.ListChildrenIds(selectedPage.ParentId.Value);
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
                pageTree_DataBind();
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
                pageTree_DataBind();
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
                        Notifier1.Warning = "$publish_parent_first";
                        return;
                    }
                }

                if (webSiteB.PublishPage(SelectedPageId))
                {
                    if (publishingPage.MarkedForDeletion && !publishingPage.IsRoot)
                    {
                        SelectedPageId = publishingPage.ParentId.Value;
                        Notifier1.Message = "$page_delete_publish_sucessfull";
                    }
                    else if (publishingPage.MarkedForDeletion)
                    {
                        SelectedPageId = -1;
                        Notifier1.Message = "$root_page_delete_publish_sucessfull";
                    }
                    else
                    {
                        Notifier1.Message = "$publish_sucessfull";
                    }
                }
                else
                {
                    Notifier1.Warning = "$publish_unsucessfull";
                }
            }
            else
            {
                Notifier1.ExceptionName = "$trying_to_publish_nonexisting_page";
            }
            InitializeControls();
            pageTree_DataBind();
        }

        protected void ButtonUnPublish_Click(object sender, EventArgs e)
        {
            BOPage publishingPage = webSiteB.GetPage(SelectedPageId);

            if (!publishingPage.IsNew)
            {
                bool hasChildren = webSiteB.ListChildrenIds(SelectedPageId).Count > 0;
                if (hasChildren)
                {
                    Notifier1.Warning = "$unpublish_unsuccessfull_because_page_has_children";
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
            pageTree_DataBind();
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
