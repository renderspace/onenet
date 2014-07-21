<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Structure.aspx.cs" Inherits="OneMainWeb.adm.Structure" Title="$structure" EnableEventValidation="false" ValidateRequest="false"  %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="OneSettings" Src="~/AdminControls/OneSettings.ascx" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <uc1:Notifier ID="Notifier1" runat="server" />
    
    <section class="module tall">
        <header><h3 class="tabs_involved">Tree structure</h3>
            <asp:Panel runat="server" ID="PanelAddSubPage" CssClass="addStuff">
                <asp:TextBox runat="server" ID="TextBoxSubPage" placeholder="Add new page"></asp:TextBox>
                <asp:Button ID="ButtonAddSubPage" runat="server"  ValidationGroup="AddPage" text="Add" onclick="cmdAddChild_Click" />
            </asp:Panel>
        </header>
        <div class="treeview">
	        <asp:TreeView OnUnload="pageTree_Unload" EnableViewState="false" ID="pageTree" runat="server" OnAdaptedSelectedNodeChanged="pageTree_SelectedNodeChanged" OnSelectedNodeChanged="pageTree_SelectedNodeChanged" PopulateNodesFromClient="false" />
        </div>
    </section>


    <div class="page-and-module-settings">
        <asp:MultiView ID="MultiView1" runat="server">
            <asp:View ID="View0" runat="server">
                <section class="module">
                    <h1><asp:Literal ID="LiteralNoAccess" runat="server" EnableViewState="false"></asp:Literal></h1>
                </section>
            </asp:View>
            <asp:View ID="View1" runat="server">
                <section class="module page-settings">
                    <div class="with_buttons">
                        <h3>
                            <asp:Literal ID="LiteralLegend" runat="server" EnableViewState="false"></asp:Literal>  
                            <asp:Image runat="server" ID="ImagePageStatus" Visible="false" CssClass="right" />

                            <span class="right buttons">
						        <asp:Button id="cmdMovePageUp" CommandName="Up" runat="server" Text=" &#9650; " OnClick="CmdMovePage_Click" />
						        <asp:Button id="cmdMovePageDown" CommandName="Down" runat="server" Text=" &#9660; " OnClick="CmdMovePage_Click" />
					        </span>
                        </h3>
                        
                    </div>
			        <div class="pageproperties">
                        <div class="input">
                            <div class="layout-left">
                                <asp:Label AssociatedControlID="TextBoxTitle" ID="LabelTitle" Text="Title" runat="server"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255" ValidationGroup="PageSett"></asp:TextBox>
                            </div>
                        </div>
                        <div class="input multi-row">
                            <div class="layout-left">
                                <asp:Label AssociatedControlID="" ID="LabelDescription" Text="Description" runat="server"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:TextBox runat="server" ID="TextBoxDescription" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="PageSett"></asp:TextBox>
                            </div>
                        </div>
			            <div class="select">
                            <div class="layout-left">
			                    <asp:label id="lblTemplate1" runat="server"	Text="Template" AssociatedControlID="ddlPageTemplate" />
                            </div>
                            <div class="layout-right">
				                <asp:dropdownlist id="ddlPageTemplate" Runat="server"></asp:dropdownlist>
                            </div>
			            </div>
                        <div class="input uri">
                            <div class="layout-left">
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="TextBoxUri" Text="URL"></asp:Label>
                            </div>
                            <div class="layout-mid">
                                <asp:Label runat="server" ID="LabelUriPart"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:TextBox runat="server" ID="TextBoxUri" ValidationGroup="PageSett"></asp:TextBox>
                            </div>
                        </div>

                        <div class="input">
                            <div class="layout-left">
                                <asp:Label AssociatedControlID="TextBoxMenuGroup" ID="Label2" Text="Menu group" runat="server"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:TextBox runat="server" ID="TextBoxMenuGroup" MaxLength="2" ValidationGroup="PageSett" type="number"></asp:TextBox>
                            </div>
                        </div>

                        <div class="checkbox">
                            <div class="layout-left">
                                <asp:Label AssociatedControlID="CheckBoxBreakPersitence" ID="Label6" Text="Break persistance" runat="server"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:CheckBox runat="server" ID="CheckBoxBreakPersitence"  ValidationGroup="PageSett" />
                            </div>
                        </div>

                        <div class="input">
                            <div class="layout-left">
                                <asp:Label AssociatedControlID="InputRedirectToUrl1" ID="Label7" Text="Redirect to URL" runat="server"></asp:Label>
                            </div>
                            <div class="layout-right">
                                <asp:TextBox runat="server" ID="InputRedirectToUrl1" MaxLength="255" ValidationGroup="PageSett" type="url"></asp:TextBox>
                            </div>
                        </div>
 
                        <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsPageSettings" runat="server" Mode="Page" Text="Page settings" DisplayCommands="false"  />	
                        
			        </div>
                    <div class="lastChange"><asp:Label runat="server" ID="LabelChanged"></asp:Label></div>
				    <div class="submit-links">
					    <asp:button id="ButtonDelete" CssClass="left delete-btn" Runat="server" Text="Delete" onclick="cmdDelete_Click" Visible="false" />
			            <asp:button ID="ButtonUndoDelete" CssClass="left" runat="server" Text="Undelete" OnClick="cmdUnDelete_Click" Visible="false" />
			            <asp:button ID="ButtonPublish"  runat="server" OnClick="ButtonPublish_Click" Cssclass="right alt_btn" />				     
			            <asp:button ID="ButtonUnPublish" CssClass="right" runat="server" OnClick="ButtonUnPublish_Click" />
                        <asp:button	id="cmdSave" Runat="server"	CssClass="save-btn" Text="Save page" onclick="cmdSave_Click" ValidationGroup="PageSett" />
				    </div>
                    
                </section>
                <section class="module module-settings">
                <div class="with_buttons">
                    <h3><asp:Literal ID="LiteralModulesOnPage" runat="server" EnableViewState="false"></asp:Literal></h3>
                        <asp:Panel ID="PanleAddInstance" runat="server" CssClass="addStuff">
				        <asp:dropdownlist id="ddlModuleTypes" Runat="server" />
                        <asp:button	id="cmdAddInstance"	Runat="server" text="Add module instance" onclick="cmdAddInstance_Click" />
                    </asp:Panel>
                </div>
               

                <%-- *************************************** Module instances ********************************************* --%>	
		        <asp:Repeater ID="RepeaterModuleInstances" runat="server" OnItemDataBound="RepaterModuleInstances_ItemDataBound" OnItemCommand="RepeaterModuleInstances_ItemCommand" >
                    <HeaderTemplate>
                        <div class="moduleInstances">
                    </HeaderTemplate>
                    <FooterTemplate>
                        </div>
                    </FooterTemplate>
		            <ItemTemplate>
                        <div class="moduleInstance">
                            <h4><%# RenderModuleName(Eval("Changed"), Eval("PendingDelete"), Eval("Name"), Eval("Id"))%> <asp:Button	ID="ButtonEdit" Runat="server"	CssClass="edit_button" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>' 
                                    Text='Edit content'	/></h4>
                            <h5><%# (bool) Eval("IsInherited") ? ResourceManager.GetString("Inherited") : "" %></h5>
                            <asp:label ID="LabelModuleDistinctName" runat="server" Visible="false" CssClass="ModuleDistinctName"></asp:label>

                            <asp:Panel runat="server" ID="PanelNotInherited">
                                <asp:Button	ID="cmdDeleteInstance" Runat="server" CommandName="COMMAND_DELETE" CommandArgument='<%# Eval("Id") %>' Text='Delete instance' CssClass="delete-btn"  />
                                <asp:Button	ID="cmdUndeleteInstance" Runat="server" CommandName="COMMAND_UNDELETE" CommandArgument='<%#	Eval("Id")	 %>' Text='Undelete' />
						        <asp:Button	ID="cmdMoveUp" Runat="server" CommandName="COMMAND_MOVE_UP" CommandArgument='<%#	Eval("Id")	%>'	Text=" &#9650; " />
						        <asp:Button	ID="cmdMoveDown" Runat="server"	CommandName="COMMAND_MOVE_DOWN" CommandArgument='<%# Eval("Id") %>' Text=" &#9660; " />

                                <asp:Label ID="Label3" runat="server" Text="$persistency" CssClass="instanceTitle"></asp:Label>

                                <details>
                                    <summary>
                                        <asp:Label ID="Label4" runat="server" Text="From" CssClass="emph"></asp:Label>
							            <asp:Label ID="lblPersistentFromDGrid" Runat="server" />
                									    
							            <asp:Label ID="Label5" runat="server" Text="To" CssClass="emph"></asp:Label>
							            <asp:Label ID="lblPersistentToDGrid" Runat="server"	/>

                                        <asp:Label ID="lblPlaceHolder" Runat="server" />
                                    </summary>

                                        <div class="select">
							                <asp:Label ID="LabelPlaceHolder" runat="server" Text="Placeholder" AssociatedControlID="ddlPlaceHolder"></asp:Label>
							                <asp:DropDownList runat="server" ID="ddlPlaceHolder" />
							            </div>
							            <div class="select">
								            <asp:Label ID="Label10" runat="server" Text="$l_from" AssociatedControlID="ddlPersistentFromDGrid"></asp:Label>
								            <asp:DropDownList runat="server" ID="ddlPersistentFromDGrid"  />
							            </div>
                                        <div class="select">    
                                            <asp:Label ID="Label11" runat="server" Text="$l_to" AssociatedControlID="ddlPersistentToDGrid"></asp:Label>
                                            <asp:DropDownList runat="server" ID="ddlPersistentToDGrid"	/>
							            </div>

                                        <asp:Button ID="cmdUpdateDetails" Runat="server" CssClass="save-btn" CommandName="COMMAND_SAVE_INSTANCE"	CommandArgument='<%# Eval("Id") %>' Text='$update' ValidationGroup="MI"  />
                                </details>

                                <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="moduleSettings" runat="server" Text="Module settings" DisplayCommands="true"/>
                            </asp:Panel>
                        </div>
		            </ItemTemplate>
		        </asp:Repeater>
            </section>
            </asp:View>
             <asp:View ID="View2" runat="server">
                <section class="module">
                     <asp:Label runat="server" ID="LabelNoRoot" Text="$no_root_page"></asp:Label>
                </section>
             </asp:View>
             <asp:View ID="View3" runat="server">
                <section class="module">
                    <asp:Label runat="server" ID="LabelNoWebSite" Text="$no_web_site"></asp:Label>
                </section>
             </asp:View>
        </asp:MultiView>
    </div>
</asp:Content>
