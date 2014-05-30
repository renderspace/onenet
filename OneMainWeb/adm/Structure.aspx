<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Structure.aspx.cs" Inherits="OneMainWeb.adm.Structure" Title="$structure" EnableEventValidation="false" ValidateRequest="false"  %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="OneSettings" Src="~/AdminControls/OneSettings.ascx" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <uc1:Notifier ID="Notifier1" runat="server" />
    
    <article class="module width_quarter tall">
        <header><h3 class="tabs_involved">Content Manager</h3>
            <asp:Panel runat="server" ID="PanelAddSubPage" CssClass="addStuff">
                <asp:TextBox runat="server" ID="TextBoxSubPage" placeholder="Add new page"></asp:TextBox>
                <asp:Button ID="ButtonAddSubPage" runat="server"  ValidationGroup="AddPage" text="$add_module_instance_button" onclick="cmdAddChild_Click" />
            </asp:Panel>
        </header>
        <div class="treeHolder">
	        <asp:TreeView OnUnload="pageTree_Unload" EnableViewState="false" ID="pageTree" runat="server" OnAdaptedSelectedNodeChanged="pageTree_SelectedNodeChanged" OnSelectedNodeChanged="pageTree_SelectedNodeChanged" PopulateNodesFromClient="false" />
        </div>
    </article>


    <asp:MultiView ID="MultiView1" runat="server">
        <asp:View ID="View0" runat="server">
            <article class="module width_3_quarter">
                <h1><asp:Literal ID="LiteralNoAccess" runat="server" EnableViewState="false"></asp:Literal></h1>
            </article>
        </asp:View>
        <asp:View ID="View1" runat="server">
            <article class="module width_3_quarter">
                <header class="with_buttons">
                    <h3>
                        <asp:Literal ID="LiteralLegend" runat="server" EnableViewState="false"></asp:Literal>  
                        <asp:Image runat="server" ID="ImagePageStatus" Visible="false" CssClass="right" />
                    </h3>
                    <div class="buttons">
                        <asp:Label runat="server" ID="LabelChanged"></asp:Label>
						<asp:Button id="cmdMovePageUp" CommandName="Up" runat="server" Text="$move_up" OnClick="CmdMovePage_Click" />
						<asp:Button id="cmdMovePageDown" CommandName="Down" runat="server" Text="$move_down" OnClick="CmdMovePage_Click" />
					</div>
                </header>
			    <div class="pageproperties">
                    <div class="input">
                        <asp:Label AssociatedControlID="" ID="LabelTitle" Text="Title" runat="server"></asp:Label>
                        <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255" ValidationGroup="PageSett"></asp:TextBox>
                    </div>
                    <div class="input">
                        <asp:Label AssociatedControlID="" ID="LabelDescription" Text="Description" runat="server"></asp:Label>
                        <asp:TextBox runat="server" ID="TextBoxDescription" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="PageSett"></asp:TextBox>
                    </div>
			        <div class="select">
			            <asp:label id="lblTemplate1" runat="server"	Text="$page_template" AssociatedControlID="ddlPageTemplate" />
				        <asp:dropdownlist id="ddlPageTemplate" Runat="server"></asp:dropdownlist>
			        </div>
                    <div class="input uri">
                        <asp:Label ID="Label1" runat="server" AssociatedControlID="TextBoxUri" Text="$page_url"></asp:Label>
                        <asp:Label runat="server" ID="LabelUriPart"></asp:Label>
                        <asp:TextBox runat="server" ID="TextBoxUri" ValidationGroup="PageSett"></asp:TextBox>
                    </div>
			        <two:ValidInput ValidationType="integer" ID="TwoInputMenuGroup" runat="server" Text="$page_menu_group" ValidationGroup="PageSett" />    
			        <asp:Panel runat="server" ID="PanelMenuGroups" CssClass="select">
			            <asp:Label ID="LabelMenuGroups" Text="$page_menu_group" runat="server" AssociatedControlID="DropDownListMenuGroups" />
			            <asp:DropDownList ID="DropDownListMenuGroups" runat="server" />
			        </asp:Panel>
			        <two:LabeledCheckBox ID="LabeledCheckBoxBreakPersistance" runat="server" Text="$break_persistance" />
			        <two:Input  ID="InputRedirectToUrl" runat="server" Text="$redirectToUrl" ValidationGroup="PageSett" Required="false" />    
                    <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsPageSettings" runat="server" Mode="Page" Text="$label_page_settings" DisplayCommands="false"  />	
			    </div>
                <footer>    
				    <div class="submit_link">
					    <asp:button id="ButtonDelete" CssClass="left" Runat="server" Text="$delete_page" onclick="cmdDelete_Click" Visible="false" />
			            <asp:button ID="ButtonUndoDelete" CssClass="left" runat="server" Text="$undelete_page" OnClick="cmdUnDelete_Click" Visible="false" />
			            <asp:button ID="ButtonPublish"  runat="server" OnClick="ButtonPublish_Click" Cssclass="right alt_btn" />				     
			            <asp:button ID="ButtonUnPublish" CssClass="right" runat="server" OnClick="ButtonUnPublish_Click" />
                        <asp:button	id="cmdSave" Runat="server"	CssClass="button green" Text="$update" onclick="cmdSave_Click" ValidationGroup="PageSett" />
				    </div>
			    </footer>	
            </article>
            <article class="module width_3_quarter">
                <header class="with_buttons">
                    <h3><asp:Literal ID="LiteralModulesOnPage" runat="server" EnableViewState="false"></asp:Literal></h3>
                     <asp:Panel ID="PanleAddInstance" runat="server" CssClass="addStuff">
                        <asp:Label id="lblAddModuleInstance" Runat="server" Text="$add_module_instance" AssociatedControlID="ddlModuleTypes" />
				        <asp:dropdownlist id="ddlModuleTypes" Runat="server" />
                        <asp:button	id="cmdAddInstance"	Runat="server" text="$add_module_instance_button" onclick="cmdAddInstance_Click" />
                    </asp:Panel>
                </header>
               

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
                            <h4><%# RenderModuleName(Eval("Changed"), Eval("PendingDelete"), Eval("Name"), Eval("Id"))%></h4>

                            <%# (bool) Eval("IsInherited") ? ResourceManager.GetString("$inherited") : "" %>


                            <asp:Button	ID="cmdDeleteInstance" Runat="server" CommandName="COMMAND_DELETE" CommandArgument='<%# Eval("Id") %>' Text='$label_delete_instance' />
                            <asp:Button	ID="cmdUndeleteInstance" Runat="server" CommandName="COMMAND_UNDELETE" CommandArgument='<%#	Eval("Id")	 %>' Text='$label_undelete_mi' />
						    <asp:Button	ID="cmdMoveUp" Runat="server" CommandName="COMMAND_MOVE_UP" CommandArgument='<%#	Eval("Id")	%>'	Text="$move_up" />
						    <asp:Button	ID="cmdMoveDown" Runat="server"	CommandName="COMMAND_MOVE_DOWN" CommandArgument='<%# Eval("Id") %>' Text="$move_down" />

                            <asp:Label ID="Label3" runat="server" Text="$persistency" CssClass="instanceTitle"></asp:Label>
    						                        
						    <asp:Label ID="Label4" runat="server" Text="$l_from" CssClass="emph"></asp:Label>
							<asp:Label ID="lblPersistentFromDGrid" Runat="server" />
                									    
							<asp:Label ID="Label5" runat="server" Text="$l_to" CssClass="emph"></asp:Label>
							<asp:Label ID="lblPersistentToDGrid" Runat="server"	/>

                            <asp:Label ID="lblPlaceHolder" Runat="server" />
                            <asp:label ID="LabelModuleDistinctName" runat="server" Visible="false" CssClass="ModuleDistinctName"></asp:label>

                            <asp:Button ID="cmdEditButton" runat="server" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>'  />
                            <asp:Button	ID="cmdEdit" Runat="server"	CssClass="edit_button" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>' 
                                Text='$label_edit'	/>

                            <div class="select">
							    <asp:Label ID="LabelPlaceHolder" runat="server" Text="$place_holder" AssociatedControlID="ddlPlaceHolder"></asp:Label>
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

                            <asp:Button ID="cmdUpdateDetails" Runat="server" CssClass="sbutton" CommandName="COMMAND_SAVE_INSTANCE"	CommandArgument='<%# Eval("Id") %>' Text='$update' ValidationGroup="MI"  />

                            <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="moduleSettings" runat="server" Text="$label_module_settings" DisplayCommands="true"/>

                        </div>
		            </ItemTemplate>
		        </asp:Repeater>
        </article>
        </asp:View>
         <asp:View ID="View2" runat="server">
            <article class="module width_3_quarter">
                 <asp:Label runat="server" ID="LabelNoRoot" Text="$no_root_page"></asp:Label>
            </article>
         </asp:View>
         <asp:View ID="View3" runat="server">
            <article class="module width_3_quarter">
                <asp:Label runat="server" ID="LabelNoWebSite" Text="$no_web_site"></asp:Label>
            </article>
         </asp:View>
    </asp:MultiView>
</asp:Content>
