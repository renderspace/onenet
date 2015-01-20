<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Structure.aspx.cs" Inherits="OneMainWeb.adm.Structure" Title="One.NET site structure" EnableEventValidation="false" ValidateRequest="false"  %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="OneSettings" Src="~/AdminControls/OneSettings.ascx" %>
<%@ Register src="~/AdminControls/LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" tagprefix="uc2" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <uc1:Notifier ID="Notifier1" runat="server" />
    
        <div class="col-md-3">
            <section class="module tall">
                <header><h3 class="tabs_involved">Tree structure  <asp:LinkButton	id="LinkButtonPublishAll" Runat="server"	CssClass="btn-success btn" Text="Publish all changes" OnClick="ButtonPublishAll_Click" /></h3>
                    <asp:Panel runat="server" ID="PanelAddSubPage" CssClass="addStuff validationGroup">
                        <div class="form-group">
                            <asp:TextBox runat="server" ID="TextBoxSubPage" placeholder="Add new page" CssClass="required"></asp:TextBox>
                            <asp:LinkButton ID="ButtonAddPage" runat="server"  ValidationGroup="AddPage" text="<span class='glyphicon glyphicon-plus'></span> Add page" onclick="ButtonAddPage_Click" CssClass="btn btn-success causesValidation" />
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelMove">
                        <span class="pull-left movePage">Move current page:</span>
                        <div class="upDown">
                            <span class="pull-left">
						        <asp:Button id="ButtonMovePageUp" CommandName="Up" runat="server" Text=" &#9650; " OnClick="CmdMovePage_Click" />
                            </span>
                            <span class="pull-right">
                                    <asp:Button id="ButtonMovePageDown" CommandName="Down" runat="server" Text=" &#9660; " OnClick="CmdMovePage_Click" />
                            </span>
                        </div>
                    </asp:Panel>
                </header>
                <div class="treeview">
	                <asp:TreeView OnUnload="TreeViewPages_Unload" EnableViewState="false" ID="TreeViewPages" runat="server" OnAdaptedSelectedNodeChanged="TreeViewPages_SelectedNodeChanged" OnSelectedNodeChanged="TreeViewPages_SelectedNodeChanged" PopulateNodesFromClient="false" />
                </div>
            </section>
        </div>
        <div class="col-md-9">
            <div class="page-and-module-settings">
        <asp:MultiView ID="MultiView1" runat="server">
            <asp:View ID="View1" runat="server">
                <section class="module page-settings">
                    <div class="with_buttons">
                        <h3>
                            <asp:Literal ID="LiteralLegend" runat="server" EnableViewState="false"></asp:Literal>  
                            <asp:Image runat="server" ID="ImagePageStatus" Visible="false" CssClass="right" />
                        </h3>
                        
                    </div>
			        <div class="pageproperties form-horizontal validationGroup">
                         <div class="form-group">
                            <label class="col-sm-4 control-label">Title</label>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255" ValidationGroup="PageSett" CssClass="form-control required"></asp:TextBox>
                            </div>
                        </div>
                         <div class="form-group">
                            <label class="col-sm-4 control-label">Meta description</label>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxDescription" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="PageSett" CssClass="form-control" placeholder="Short description of the page, used on menus and also displayed in Google and Facebook results. Important for SEO. Some modules may override this text."></asp:TextBox>
                            </div>
                        </div>
			             <div class="form-group">
                            <label class="col-sm-4 control-label">Template</label>
                            <div class="col-sm-8">
				                <asp:dropdownlist id="ddlPageTemplate" Runat="server" CssClass="form-control"></asp:dropdownlist>
                            </div>
			            </div>
                        <div class="form-group">
                            <label class="col-sm-4 control-label">URL</label>
                           <div class="col-sm-2">
                                <asp:Label runat="server" ID="LabelUriPart"></asp:Label>
                            </div>
                           <div class="col-sm-6">
                                <asp:TextBox runat="server" ID="TextBoxUri" ValidationGroup="PageSett" CssClass="form-control"></asp:TextBox>
                                <p class="help-block">Important for SEO, please user short text and minus sign to signify space.</p>
                            </div>
                        </div>

                         <div class="form-group">
                            <label class="col-sm-4 control-label">Menu group</label>
                            <div class="col-sm-2">
                                <asp:TextBox runat="server" ID="TextBoxMenuGroup" MaxLength="2" ValidationGroup="PageSett" type="number" CssClass="form-control required digits"></asp:TextBox>
                            </div>
                             <div class="col-sm-6">
                                <p class="help-block">Signifies in which navigation menu will this page be displayed.<br /> Menu numbers are defined in template.</p>
                            </div>
                        </div>

                        

                         <div class="form-group">
                            <label class="col-sm-4 control-label">Redirect to URL</label>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="InputRedirectToUrl1" MaxLength="255" ValidationGroup="PageSett" CssClass="form-control absrelurl" placeholder="Force redirect to some other page. Use absolute URL. "></asp:TextBox>
                            </div>
                        </div>

                        <div class="checkbox">
                            <label class="col-sm-offset-4 col-sm-8">
                                <asp:CheckBox runat="server" ID="CheckBoxSubPageRouting"  ValidationGroup="PageSett" />
                                Single subpage routing (e.g. for single article module)
                            </label>
                        </div>

                        <div class="form-group">
                             <label class="col-sm-4 control-label">Image</label>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxSubtitle" MaxLength="255" ValidationGroup="PageSett" type="url" CssClass="form-control" placeholder="Image used by Facebook when sharing. Use at least 1200 x 630 pixels and absolute path. Some modules may override it."></asp:TextBox>
                            </div>
                        </div>

                        

                        <div class="checkbox">
                            <label class="col-sm-offset-4 col-sm-8">
                                <asp:CheckBox runat="server" ID="CheckBoxBreakPersitence"  ValidationGroup="PageSett" />
                                Break persistance
                            </label>
                        </div>

                        <asp:Panel CssClass="form-group" runat="server" ID="PanelFbDebug" Visible="false">
                             <label class="col-sm-4 control-label"><asp:HyperLink runat="server" ID="HyperLinkFBDebug" Target="_blank">FB Debug</asp:HyperLink></label>
                        </asp:Panel>

                        <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsPageSettings" runat="server" Mode="Page" Text="Page settings" DisplayCommands="false"  />	                        
                    
				        <div class="form-group">
                            <div class="col-sm-4">
						        <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
                            </div>

                            <div class="col-sm-8">
					            <asp:LinkButton id="ButtonDelete" CssClass="left btn btn-danger" Runat="server" Text="Delete" onclick="cmdDelete_Click" Visible="false" />
			                    <asp:LinkButton ID="ButtonUndoDelete" CssClass="left btn btn-info" runat="server" Text="Undelete" OnClick="ButtonUndelete_Click" Visible="false" />
			                    <asp:LinkButton ID="ButtonPublish"  runat="server" OnClick="ButtonPublish_Click" Cssclass="right btn-success btn" />				     
			                    <asp:LinkButton ID="ButtonUnPublish" CssClass="right btn btn-info" runat="server" OnClick="ButtonUnPublish_Click" Text="Unpublish" />
                                <asp:LinkButton	id="cmdSave" Runat="server"	CssClass="btn-success btn causesValidation" Text="Save page" onclick="cmdSave_Click" ValidationGroup="PageSett" />
                            </div>
				        </div>
                    </div>
                </section>
                <section class="module module-settings">
                <div class="with_buttons">
                    <h3><asp:Literal ID="LiteralModulesOnPage" runat="server" EnableViewState="false"></asp:Literal></h3>
                        <asp:Panel ID="PanleAddInstance" runat="server" CssClass="addStuff">
				        <asp:dropdownlist id="ddlModuleTypes" Runat="server" />
                        <asp:LinkButton	id="cmdAddInstance"	Runat="server" text="Add module instance" CssClass="btn btn-success" onclick="cmdAddInstance_Click"><span class="glyphicon glyphicon-plus"></span> Add</asp:LinkButton>
                    </asp:Panel>
                </div>
               

                <%-- *************************************** Module instances ********************************************* --%>	
		        <asp:Repeater ID="RepeaterModuleInstances" runat="server" OnItemDataBound="RepaterModuleInstances_ItemDataBound" OnItemCommand="RepeaterModuleInstances_ItemCommand" >
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <FooterTemplate>
                    </FooterTemplate>
		            <ItemTemplate>
                        <div class="moduleInstance">
                            <h4>
                                <asp:LinkButton	ID="cmdDeleteInstance" Runat="server" CommandName="COMMAND_DELETE" CommandArgument='<%# Eval("Id") %>' Text="<span class='glyphicon glyphicon-trash'></span>" CssClass="btn btn-danger pull-left"  />
                                <span class="m-ops"><%# RenderModuleName(Eval("Changed"), Eval("PendingDelete"), Eval("Name"), Eval("Id"))%>
                                
                                <asp:label ID="LabelModuleDistinctName" runat="server" Visible="false" CssClass="ModuleDistinctName"></asp:label> 
                                </span>
								<span class="m-btns">
                                    
								<asp:LinkButton	ID="ButtonEdit" Runat="server"	CssClass="btn btn-info" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>' 
                                    Text='<span class="glyphicon glyphicon-pencil"></span> Edit'	/>

                                <asp:PlaceHolder ID="PlaceHolderNotInherited2" runat="server">
                                    
                                    <asp:Button	ID="cmdUndeleteInstance" Runat="server" CommandName="COMMAND_UNDELETE" CommandArgument='<%#	Eval("Id")	 %>' Text='Undelete' CssClass="btn" />
						            <asp:LinkButton	ID="cmdMoveUp" Runat="server" CommandName="COMMAND_MOVE_UP" CommandArgument='<%#	Eval("Id")	%>'	Text="<span class='glyphicon glyphicon-arrow-up'></span>"  CssClass="btn s-btn" />
						            <asp:LinkButton	ID="cmdMoveDown" Runat="server"	CommandName="COMMAND_MOVE_DOWN" CommandArgument='<%# Eval("Id") %>' Text="<span class='glyphicon glyphicon-arrow-down'></span>"  CssClass="btn s-btn" />
                                </asp:PlaceHolder>
								</span>
                            </h4>

                            <div class="col-sm-4">
                                <details runat="server" id="HtmlDetails"  class="form-horizontal">
                                    <summary>
                                        <asp:Literal ID="LiteralInstanceSummary" Runat="server" />
                                    </summary>
                                        <asp:PlaceHolder ID="PlaceHolderNotInherited1" runat="server">
                                            <div class="form-group">
							                    <label class="col-sm-3 control-label">Placeholder</label>
                                                <div class="col-sm-9">
							                        <asp:DropDownList runat="server" ID="ddlPlaceHolder" CssClass="form-control" />
                                                </div>
							                </div>
							                <div class="form-group">
								                <label class="col-sm-3 control-label">Inherited from depth</label>
                                                <div class="col-sm-9">
								                    <asp:DropDownList runat="server" ID="ddlPersistentFromDGrid" CssClass="form-control" />
                                                </div>
							                </div>
                                            <div class="form-group">
                                               <label class="col-sm-3 control-label">Inherited to</label>
                                                <div class="col-sm-9">
                                                    <asp:DropDownList runat="server" ID="ddlPersistentToDGrid"	CssClass="form-control" />
                                                </div>
							                </div>
                                            <div class="form-group">
                                                 <div class="col-sm-offset-3 col-sm-9">
                                                    <asp:LinkButton ID="cmdUpdateDetails" Runat="server" CssClass="btn btn-success" CommandName="COMMAND_SAVE_INSTANCE"	CommandArgument='<%# Eval("Id") %>' Text='Save' ValidationGroup="MI"  />
                                                 </div>
                                            </div>
                                        </asp:PlaceHolder>
                                </details>
                            </div>
                            <div class="col-sm-8">
                                <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="moduleSettings" runat="server" Text="Module settings" DisplayCommands="true"/>
                            </div>
                        </div>
		            </ItemTemplate>
		        </asp:Repeater>
            </section>
            </asp:View>
             <asp:View ID="View2" runat="server">
                <section class="module">
                    <asp:Label runat="server" ID="LabelMessage"></asp:Label>
                </section>
             </asp:View>
             <asp:View ID="View3" runat="server">
                <section class="module">
                    <asp:Label runat="server" ID="LabelNoWebSite" Text="$no_web_site"></asp:Label>
                </section>
             </asp:View>
            <asp:View ID="View4" runat="server">
                <section class="module">
                    Please select page
                </section>
             </asp:View>
        </asp:MultiView>
    </div>
        </div>
</asp:Content>
