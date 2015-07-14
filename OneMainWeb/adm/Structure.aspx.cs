using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
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
        BOPage SelectedPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SelectedWebsite_ValidateDataBind();
            TreeViewPages_DataBind();
            if (!IsPostBack)
            {
                PanelPublishAll.Visible = PublishAllRights;

                Modules_DataBind();
                Templates_DataBind();
                SelectedPage_DataBind();   
            }
        }

        protected void SelectedWebsite_ValidateDataBind()
        {
            // WEBSITE
            if (SelectedWebsite == null)
            {
                ResetAllControlsToDefault("You don't have permissions for any site or there are no websites defined in database.");
                return;
            }
            // ROOT PAGE
            RootNodeID = webSiteB.GetRootPageId(SelectedWebSiteId);
            if (!RootNodeID.HasValue)
            {
                ResetAllControlsToDefault("Website doesn't have a root page. Use form on the left to add it.");
                PanelAddSubPage.Visible = true;
                return;
            }
            // SELECTED PAGE
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
                ResetAllControlsToDefault("You do not have the rights to edit this page. Please select another page on the left.");
                return;
            }

            MultiView1.ActiveViewIndex = 0;
            PanelAddSubPage.Visible = true;

            if (RootNodeID.Value == SelectedPageId && !SelectedWebsite.HasGoogleAnalytics)
            {
                Notifier1.Warning = "Please consider enabling Google Analytics on this website.";
            }
        }

        protected void ResetAllControlsToDefault(string message)
        {
            SelectedPageId = 0;
            PanelFbDebug.Visible = false;
            PanelMove.Visible = false;
            LinkButtonPublishAll.Visible = false;
            TreeViewPages.Nodes.Clear();
            TreeViewPages.DataBind();
            RepeaterModuleInstances.Visible = false;
            RepeaterModuleInstances.DataSource = null;
            RepeaterModuleInstances.DataBind();
            TextBoxUri.Visible = false;
            LastChangeAndHistory1.Text = "";
            LastChangeAndHistory1.SelectedContentId = 0;
            TextBoxTitle.Text = "";
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
            SetPreviewHyperLink("");
            TextBoxUri.Text = "";
            TextBoxMenuGroup.Text = "";
            CheckBoxBreakPersitence.Checked = false;
            InputRedirectToUrl1.Text = "";
            CheckBoxSubPageRouting.Checked = false;
            ButtonMovePageUp.Visible = true;
            ButtonMovePageDown.Visible = true;
            OneSettingsPageSettings.ItemId = 0;
            OneSettingsPageSettings.Databind();
            ddlPageTemplate.ClearSelection();

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

            TreeNode selectedNode = null;
            var siteStructure = webSiteB.GetSiteStructure(SelectedWebSiteId);
            LinkButtonPublishAll.Visible = siteStructure.Count > 0 && PublishAllRights;
            TreeNode tree = OneHelper.PopulateTreeViewControl(siteStructure, null, SelectedPageId, currentExpandLevel, ref selectedNode);

            if (tree != null)
            {
                TreeViewPages.Nodes.Add(tree);
                new OneMainWeb.adm.TreeViewState().RestoreTreeView(TreeViewPages, this.GetType().ToString());

                if (selectedNode != null)
                {
                    TreeViewPages.FindNode(selectedNode.ValuePath).Selected = true;
                    SelectedPageId = Int32.Parse(selectedNode.Value);
                    SelectedPage = webSiteB.GetPage(SelectedPageId);
                }
                else
                {
                    tree.Selected = true;
                    SelectedPageId = Int32.Parse(tree.Value);
                    SelectedPage = webSiteB.GetPage(SelectedPageId);
                }

                MultiView1.Visible = (TreeViewPages.Nodes.Count != 0);
                MultiView1.ActiveViewIndex = 0;

                TreeViewPages.ExpandDepth = currentExpandLevel;
            }
        }

        protected string RenderModuleName(object _Changed, object _PendingDelete, object name)
        {
            return name.ToString() + " " + RenderStatusIcons(_PendingDelete, _Changed);
        }

        protected void SelectedPage_DataBind()
        {
            if (SelectedPage != null)
            {
                PanelMove.Visible = true;
                ButtonMovePageDown.Visible = true;
                ButtonMovePageUp.Visible = true;
                TextBoxUri.Visible = SelectedPageId != RootNodeID;
                LastChangeAndHistory1.Text = SelectedPage.DisplayLastChanged;
                LastChangeAndHistory1.SelectedContentId = SelectedPage.ContentId.Value;
                LastChangeAndHistory1.SelectedLanguageId = SelectedPage.LanguageId;

                LabelCurrentPageId.Text = "[" + SelectedPage.Id.ToString() + "]";
                TextBoxTitle.Text = SelectedPage.Title;
                TextBoxSubtitle.Text = SelectedPage.SubTitle;
                TextBoxDescription.Text = SelectedPage.Teaser;
                TextBoxSeoTitle.Text = SelectedPage.Html;
                ButtonPublish.Visible = SelectedPage.IsChanged && PublishRights;
                ButtonUnPublish.Visible = !SelectedPage.IsNew && PublishRights;
                ButtonDelete.Visible = !SelectedPage.MarkedForDeletion && DeletePageRights;
                ButtonUndoDelete.Visible = SelectedPage.MarkedForDeletion;
                PanelAddSubPage.Visible = PanelAddSubPage.Visible && !SelectedPage.MarkedForDeletion;
                ButtonPublish.Text = SelectedPage.MarkedForDeletion ? "Completely delete page" : "Publish page";
                ImagePageStatus.Visible = true;

                ImagePageStatus.Attributes.Add("data-toggle", "tooltip");
                ImagePageStatus.Attributes.Add("data-placement", "right");
                if (SelectedPage.MarkedForDeletion)
                {
                    ImagePageStatus.ImageUrl = "/Res/brisanje.png";
                    ImagePageStatus.Attributes.Add("title", "Marked for deletion");
                }
                else if (SelectedPage.IsChanged)
                {
                    ImagePageStatus.ImageUrl = "/Res/objava.png";
                    ImagePageStatus.Attributes.Add("title", "Changes waiting for publish");
                }
                else
                {
                    ImagePageStatus.Attributes.Add("title", "Published");
                    ImagePageStatus.ImageUrl = "/Res/objavljeno.png";
                }

                LabelUriPart.Text = SelectedPage.ParentURI;
                SetPreviewHyperLink(SelectedPage.URI);
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
                CheckBoxSubPageRouting.Checked = SelectedPage.HasSubPageRouting;
                if (SelectedPage.ParentId.HasValue)
                {
                    List<int> pages = webSiteB.ListChildrenIds(SelectedPage.ParentId.Value);
                    for (int i = 0; i < pages.Count; i++)
                    {
                        if (pages[i] == SelectedPage.Id)
                        {
                            if (i == 0)
                            {
                                ButtonMovePageUp.Visible = false;
                            }
                            else if (i == pages.Count - 1)
                            {
                                ButtonMovePageDown.Visible = false;
                            }
                        }
                    }
                }

                //                    OneSettingsPageSettings.Settings = SelectedPage.Settings;
                OneSettingsPageSettings.ItemId = SelectedPage.Id;
                OneSettingsPageSettings.Mode = AdminControls.OneSettings.SettingMode.Page;
                OneSettingsPageSettings.LoadSettingsControls(SelectedPage.Settings);
                OneSettingsPageSettings.Databind();

                if (SelectedPageId == RootNodeID)
                {
                    ButtonMovePageDown.Visible = false;
                    ButtonMovePageUp.Visible = false;
                }

                PanelMove.Visible = ButtonMovePageDown.Visible || ButtonMovePageUp.Visible;

                RepeaterModuleInstances_DataBind();

                if (SelectedPage.IsPublished && SelectedWebsite.ProductionUrl.StartsWith("http"))
                {
                    PanelFbDebug.Visible = true;
                    var url = SelectedWebsite.ProductionUrl.TrimEnd('/') + SelectedPage.URI;
                    HyperLinkFBDebug.NavigateUrl = "https://developers.facebook.com/tools/debug/og/object?q=" + url;
                }
            }
            else
            {
                MultiView1.ActiveViewIndex = 1;
                LabelMessage.Text = "No page selected. Please refresh the page.";
            }
        }

        private void Modules_DataBind()
        {
            DropDownListModules.Items.Clear();
            List<BOModule> modules = BWebsite.ListModules();
            DropDownListModules.Items.Add(new ListItem("Please select a module to add..", "0"));
            foreach (BOModule module in modules)
            {
                DropDownListModules.Items.Add(new ListItem(module.Name, module.Id.ToString()));
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
            var instances = webSiteB.ListModuleInstances(SelectedPage.Id);
            PanelNoModuleInstances.Visible = instances.Count == 0;
            RepeaterModuleInstances.Visible = instances.Count != 0;
            RepeaterModuleInstances.DataSource = instances;
            RepeaterModuleInstances.DataBind();
        }

        protected void RepaterModuleInstances_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            
            // for position and inheritance settings
            Control updateButton = e.Item.FindControl("cmdUpdateDetails");
            var ButtonModalEdit = e.Item.FindControl("ButtonModalEdit") as WebControl;
            var ButtonContentTemplateModalEdit = e.Item.FindControl("ButtonContentTemplateModalEdit") as WebControl;
            Control deleteButton = e.Item.FindControl("cmdDeleteInstance");
            Control undeleteButton = e.Item.FindControl("cmdUndeleteInstance");
            Control cmdMoveUp = e.Item.FindControl("cmdMoveUp");
            Control cmdMoveDown = e.Item.FindControl("cmdMoveDown");
            Label lblPlaceHolder = (Label)(e.Item.FindControl("lblPlaceHolder"));
            var LabelModuleDistinctName = e.Item.FindControl("LabelModuleDistinctName") as Label;
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

                deleteButton.Visible = !moduleInstance.PendingDelete && !moduleInstance.IsInherited;
                undeleteButton.Visible = moduleInstance.PendingDelete && !moduleInstance.IsInherited;
                ButtonModalEdit.Visible = (moduleInstance.ModuleName == "TextContent" || moduleInstance.ModuleName == "SpecialContent") ? (!moduleInstance.IsInherited && !moduleInstance.PendingDelete) : false;
                ButtonContentTemplateModalEdit.Visible = (moduleInstance.ModuleName == "TemplateContent") ? (!moduleInstance.IsInherited && !moduleInstance.PendingDelete) : false;

                BOInternalContent textContentModel = null;
                if (moduleInstance.ModuleName == "TextContent" || moduleInstance.ModuleName == "SpecialContent")
                {
                    textContentModel = textContentB.GetTextContent(moduleInstance.Id);
                    if (textContentModel.IsComplete)
                    {
                        ButtonModalEdit.Attributes.Add("data-content-id", textContentModel.ContentId.Value.ToString());
                        ButtonModalEdit.Attributes.Add("data-ck", moduleInstance.ModuleName == "TextContent" ? "true" : "false");
                    }
                }
                if (moduleInstance.ModuleName == "TextContent" || moduleInstance.ModuleName == "SpecialContent")
                {
                    if (textContentModel != null && textContentModel.IsComplete)
                    {
                        LabelModuleDistinctName.Visible = true;
                        var distinctName = "";
                        if (moduleInstance.ModuleName == "SpecialContent")
                        {
                            distinctName = StringTool.StripHtmlTags(textContentModel.Html);
                        }
                        else
                        {
                            distinctName = StringTool.StripHtmlTags(textContentModel.Title);
                        }
                        string postfix = (distinctName.Length >= 35) ? "..." : "";
                        LabelModuleDistinctName.Text = distinctName.Substring(0, distinctName.Length < 35 ? distinctName.Length : 35) + postfix;
                    }
                    else
                    {
                        LabelModuleDistinctName.Visible = true;
                        LabelModuleDistinctName.Text = "[Empty]";
                    }   
                }

                if (moduleInstance.Settings.ContainsKey("InstanceComment") && !string.IsNullOrWhiteSpace(moduleInstance.Settings["InstanceComment"].Value))
                {
                    LabelModuleDistinctName.Visible = true;
                    LabelModuleDistinctName.Text = "[" + moduleInstance.Settings["InstanceComment"].Value + "]";
                }

                var moduleSettings = e.Item.FindControl("moduleSettings") as AdminControls.OneSettings;
                moduleSettings.Visible = !moduleInstance.IsInherited;
                //                    moduleSettings.Settings = moduleInstance.Settings;
                moduleSettings.Mode = AdminControls.OneSettings.SettingMode.Module;
                moduleSettings.ItemId = moduleInstance.Id;
                moduleSettings.LoadSettingsControls(moduleInstance.Settings);
                moduleSettings.Databind();
                PlaceHolderNotInherited1.Visible = PlaceHolderNotInherited2.Visible = !moduleInstance.IsInherited;

                moduleSettings.Visible = moduleSettings.Visible && !moduleInstance.IsInherited;

                if (moduleInstance.ModuleName == "TemplateContent" && moduleInstance.Settings.ContainsKey("TemplateId"))
                {
                    var templateId = Int32.Parse(moduleInstance.Settings["TemplateId"].Value.ToString());
                    if (templateId > 0)
                    {
                        ButtonContentTemplateModalEdit.Attributes.Add("data-template-id", templateId.ToString());
                        ButtonContentTemplateModalEdit.Attributes.Add("data-content-template-instance-id", moduleInstance.Id.ToString());
                    }
                    else
                    {
                        ButtonContentTemplateModalEdit.Visible = false;
                    }
                }

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
                   // <span class="ch" data-toggle="tooltip" data-placement="bottom" title="" data-original-title="Changes waiting for publish">
                }   

                if (moduleInstance.Settings.ContainsKey("ExtraCssClass") && !string.IsNullOrWhiteSpace(moduleInstance.Settings["ExtraCssClass"].Value))
                    LiteralInstanceSummary.Text += "<span  data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"\" data-original-title=\"Extra CSS class\" class=\"glyphicon glyphicon-asterisk pull-right\" aria-hidden=\"true\"></span>";

                if (moduleInstance.Settings.ContainsKey("UniqueTranslation") && moduleInstance.Settings["UniqueTranslation"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    LiteralInstanceSummary.Text += "<span  data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"\" data-original-title=\"Unique translation\" class=\"glyphicon glyphicon-sunglasses pull-right\" aria-hidden=\"true\"></span>";

                if (ddlPlaceHolder != null)
                {
                    ddlPlaceHolder.Items.Clear();
                    ddlPlaceHolder.DataSource = BWebsite.ListPlaceHolders();
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
            SelectedPage_DataBind();
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
                                    TreeViewPages_DataBind();
                                }
                            }
                            break;
                        }
                    case "COMMAND_UNDELETE":
                        {
                            webSiteB.UnDeleteModuleInstance(moduleInstanceID);
                            break;
                        }
                    case "COMMAND_DELETE":
                        {
                            webSiteB.DeleteModuleInstance(moduleInstanceID);
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
                SelectedPage_DataBind();
            }
        }

        

        protected void ButtonAddPage_Click(object sender, EventArgs e)
        {
            var result = webSiteB.AddSubPage(TextBoxSubPage.Text, SelectedWebSiteId, SelectedPageId);
            switch (result)
            {
                case BWebsite.AddSubPageResult.Ok:
                    TextBoxSubPage.Text = "";
                    Notifier1.Title = "Added new page";
                    Notifier1.Message = "Add more pages or click on the page to edit it.";
                    break;
                case BWebsite.AddSubPageResult.NoTemplates:
                    Notifier1.Warning = "Unsucessfull add page";
                    Notifier1.Message = "There are no availible templates in the database!";
                    break;
                case BWebsite.AddSubPageResult.OkRootPage:
                    Notifier1.Title = "Added new root page.";
                    Notifier1.Message = "URL was automatically set as blank";
                    TextBoxSubPage.Text = "";
                    break;
                case BWebsite.AddSubPageResult.PartialLinkExistsOnThisLevel:
                    Notifier1.Warning = "Sucessfull add page";
                    Notifier1.Message = "This child page cannot be added because the page URL already exists at this level under this parent page!";
                    break;
                case BWebsite.AddSubPageResult.PartialLinkNotValid:
                    Notifier1.Warning = "Unsucessfull add page";
                    Notifier1.Message = "This URL cannot be updated because it contains invalid characters! Valid characters are a to z, A to Z and 0 to 9";
                    break;
                case BWebsite.AddSubPageResult.TriedToAddRootPageToNonEmptySite:
                    Notifier1.Warning = "Trying to add root page to nonempty website";
                    break;
            }

            TreeViewPages_DataBind();
            SelectedPage_DataBind();
            MultiView1.Visible = (TreeViewPages.Nodes.Count != 0);
        }

        public void TreeViewPages_SelectedNodeChanged(object sender, EventArgs e)
        {
            SelectedPageId = FormatTool.GetInteger(TreeViewPages.SelectedNode.Value);
            TreeViewPages.CollapseAll();
            ExpandLoop(TreeViewPages.SelectedNode);
            SelectedWebsite_ValidateDataBind();
            SelectedPage_DataBind();
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
                SelectedPage_DataBind();
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
                    SelectedPage_DataBind();
                    break;
                case BWebsite.DeletePageByIdResult.Deleted:
                    TreeViewPages_DataBind();
                    SelectedPage_DataBind();
                    break;
                case BWebsite.DeletePageByIdResult.HasChildren:
                    Notifier1.Warning = "Page has children, please delete them first.";
                    break;
                case BWebsite.DeletePageByIdResult.Error:
                    Notifier1.Warning = "Delete page error";
                    break;
            }
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            BOPage page = webSiteB.GetPage(SelectedPageId);

            if (page != null)
            {
                int menuGroupID = FormatTool.GetInteger(TextBoxMenuGroup.Text);
                int selectedTemplateID = FormatTool.GetInteger(ddlPageTemplate.SelectedValue);
                string newParLink = BWebsite.CleanStringForUrl(TextBoxUri.Text);
                bool validParLink = true;
                if (!page.IsRoot)
                {
                    int parentPageID = page.ParentId.Value;    
                    validParLink = webSiteB.ValidateParLinkAgainstDB(parentPageID, SelectedPageId, newParLink, SelectedWebSiteId);
                    if (!validParLink && page.ParLink != newParLink &&  SelectedPageId != parentPageID)
                    {
                        Notifier1.Warning = "This URL cannot be updated because the URL already exists at this level for this parent page!";
                        return;
                    }
                }
                else
                {
                    newParLink= "";
                }

                page.MenuGroup = menuGroupID;
                page.Template = new BOTemplate { Id = selectedTemplateID };
                page.Title = TextBoxTitle.Text;
                page.SubTitle = TextBoxSubtitle.Text;
                page.Teaser = TextBoxDescription.Text;
                page.Html = TextBoxSeoTitle.Text;
                page.ParLink = newParLink;
                page.BreakPersistance = CheckBoxBreakPersitence.Checked;
                page.RedirectToUrl = InputRedirectToUrl1.Text;
                if (CheckBoxSubPageRouting.Checked)
                {
                    page.SubRouteUrl = "{parameter}";
                }
                else
                {
                    page.SubRouteUrl = "";
                }
                webSiteB.ChangePage(page);

                OneSettingsPageSettings.Save();
                SelectedPage_DataBind();
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
                SelectedPage_DataBind();
            }
        }

        protected void ButtonAddInstance_Click(object sender, EventArgs e)
        {
            int selectedModuleID = FormatTool.GetInteger(DropDownListModules.SelectedItem.Value);

            if (selectedModuleID < 1)
            {
                Notifier1.Warning = "Please select a module to add.";
            }
            else
            {
                var placeHolderData = BWebsite.ListPlaceHolders();
                if (placeHolderData.Count < 1)
                    return;

                int selectedPlaceHolder = placeHolderData[0].Id.Value;
                var result = webSiteB.AddModulesInstance(SelectedPageId, selectedPlaceHolder, selectedModuleID);
            }
            TreeViewPages_DataBind();
            SelectedPage_DataBind();
        }

        protected void ButtonPublish_Click(object sender, EventArgs e)
        {
            if (!PublishRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

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
                        SelectedPage = webSiteB.GetPage(SelectedPageId);

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
            SelectedPage_DataBind();
            TreeViewPages_DataBind();
        }

        protected void ButtonUnPublish_Click(object sender, EventArgs e)
        {
            if (!PublishRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

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
                    Notifier1.Message = "Unpublish successfull";
                }
                else
                {
                    Notifier1.Warning = "Unpublish unsuccessfull";
                }
            }
            else
            {
                Notifier1.ExceptionName = "Page was never published";
            }
            SelectedPage_DataBind();
            TreeViewPages_DataBind();
        }

        protected void ButtonPublishAll_Click(object sender, EventArgs e)
        {
            if (!PublishAllRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

            var publishedCount = webSiteB.PublishAllPages(SelectedWebSiteId);
            Notifier1.Title = "Publishing pages.";
            if (publishedCount > 0)
            {
                Notifier1.Message = string.Format("Published {0} pages", publishedCount);
                SelectedPage_DataBind();
                TreeViewPages_DataBind();
            }
            else
            {
                Notifier1.Message = "No pages published";
                
            }
        }

        private void SetPreviewHyperLink(string uri)
        {
            if (SelectedWebsite != null)
            {
                HyperLinkPreview.Text = uri;
                HyperLinkPreview.Target = "_blank";
                HyperLinkPreview.NavigateUrl = SelectedWebsite.PreviewUrl.TrimEnd('/') + uri;
            }
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
