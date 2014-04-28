<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Structure.aspx.cs" Inherits="OneMainWeb.adm.Structure" Title="$structure" EnableEventValidation="false" ValidateRequest="false"  %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary"  %>
<%@ Register TagPrefix="one" TagName="OneSettings" Src="~/AdminControls/OneSettings.ascx" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <uc1:Notifier ID="Notifier1" runat="server" />
    
    <div class="topStructure">
		<asp:checkbox id="chkEnableDelete" AutoPostBack="true" Runat="server" text="$enable_delete" OnCheckedChanged="chkEnableDelete_CheckedChanged" />
	</div>	
	
    <div class="centerStructure structure">
        <asp:checkbox id="chkShowPagesWithoutTranslation" AutoPostBack="true" Runat="server" Text="$show_pages_without_translation" OnCheckedChanged="chkShowPagesWithoutTranslation_CheckedChanged" /><br/>
	    <asp:checkbox id="chkEnableStructureEditing" AutoPostBack="true" Runat="server"	Text="$enable_structure_editing" OnCheckedChanged="chkEnableStructureEditing_CheckedChanged" /><br/>
	    <asp:checkbox id="chkExpandTree" AutoPostBack="true" Runat="server"	Text="$expand_tree" OnCheckedChanged="chkExpandTree_CheckedChanged" /><br/>
	    	    
	    <div id="divAddChild" runat="server" style="width: 100%;">
	        <two:InputWithButton ID="InputWithButtonAddSubPage" Text="$add_subpage" ValidationGroup="AddPage" ButtonText="$add_page" onclick="cmdAddChild_Click" runat="server" />
		    <br style="clear: both;" />
	    </div>	
	    <div>&nbsp;</div>
	    <br style="clear: both; "/> 
	    <div id="treeHolder" runat="server" class="treeHolder">
	        <asp:TreeView OnUnload="pageTree_Unload" EnableViewState="false" ID="pageTree" runat="server" OnAdaptedSelectedNodeChanged="pageTree_SelectedNodeChanged" OnSelectedNodeChanged="pageTree_SelectedNodeChanged" PopulateNodesFromClient="false" />
		</div>
    </div>
    
    <div class="mainEditor" id="rightSettingsNoAccess" runat="server" visible="false">
        <h1><asp:Literal ID="LiteralNoAccess" runat="server" EnableViewState="false"></asp:Literal></h1>
    </div>
    
    <div class="mainEditor" id="rightSettings" runat="server">
	    <asp:PlaceHolder ID="PlaceHolderControls" Visible="true" runat="server">
		    <div class="statusBars">
		        <div class="pageButtons">
		            <asp:Image runat="server" ID="ImagePageStatus" Visible="false" />
			        <div style="width: 450px; margin-top: 5px; float: right;">
			            <asp:button id="ButtonDelete" CssClass="left" Runat="server" Text="$delete_page" onclick="cmdDelete_Click" Visible="false" />
			            <asp:button ID="ButtonUndoDelete" CssClass="left" runat="server" Text="$undelete_page" OnClick="cmdUnDelete_Click" Visible="false" />
			            <asp:button ID="ButtonPublish" CssClass="right" runat="server" OnClick="ButtonPublish_Click" />				     
			            <asp:button ID="ButtonUnPublish" CssClass="right" runat="server" OnClick="ButtonUnPublish_Click" />				     			            
			        </div>   
				</div>
		    </div>

		    <div class="propertyArea">
		        <fieldset>
			        <legend><asp:Literal ID="LiteralLegend" runat="server" EnableViewState="false"></asp:Literal></legend>
			        <div id="show_fields" class="pageproperties">
			                <div class="statusBars">
			                    <div class="lastChanged">
			                        <two:InfoLabel ContainerCssClass="lastChangedStatus" ID="InfoLabelLastChange" runat="server" Text="$last_changed_date"></two:InfoLabel>
						            <asp:Button id="cmdMovePageUp" CommandName="Up" runat="server" Text="$move_up" OnClick="CmdMovePage_Click" />
						            <asp:Button id="cmdMovePageDown" CommandName="Down" runat="server" Text="$move_down" OnClick="CmdMovePage_Click" />
						            <asp:Button ID="cmdEdit" OnClientClick="javascript:swapIt('edit_fields', 'show_fields'); return false;" runat="server" class="sbutton" EnableViewState="false" />
						        </div>
					        </div>
					        <two:InfoLabel id="InfoLabelSelectedPage" runat="server" Text="$page_title"></two:InfoLabel>
				            <two:InfoLabel id="InfoLabelTemplate" runat="server" Text="$page_template"></two:InfoLabel>
				            <two:InfoLabel id="InfoLabelUrl" runat="server" Text="$page_url" ContainerCssClass="urllabel"></two:InfoLabel>
				            <two:InfoLabel id="InfoLabelMenuGroup" runat="server" Text="$page_menu_group"></two:InfoLabel>
				            <two:InfoLabel id="InfoLabelBreakPersistance" runat="server" Text="$break_persistance"></two:InfoLabel>
			                <two:InfoLabel id="InfoLabelSelectedPageId" runat="server" Text="$page_id"></two:InfoLabel>			            
			                <two:InfoLabel id="InfoLabelRedirectToUrl" runat="server" Text="$redirectToUrl"></two:InfoLabel>			            
			                
			                <two:InfoLabel id="InfoLabelView" runat="server" Text="$viewGroups"></two:InfoLabel>			            
			                <two:InfoLabel id="InfoLabelEdit" runat="server" Text="$editGroups"></two:InfoLabel>			            
			                <two:InfoLabel id="InfoLabelSSL" runat="server" Text="$requireSSL"></two:InfoLabel>			            
			                <two:InfoLabel id="InfoLabelPrimaryLanguage" runat="server" Text="$label_website_primary_language" Visible="false"></two:InfoLabel>
				    </div>
        			
			        <div id="edit_fields" class="pageproperties" style="display: none;">
			            <two:Input ID="TwoInputTitle" runat="server" Text="$page_title" ValidationGroup="PageSett" />
			            <div class="select">
			                <asp:label id="lblTemplate1" runat="server"	Text="$page_template" AssociatedControlID="ddlPageTemplate" />
				            <asp:dropdownlist id="ddlPageTemplate" Runat="server"></asp:dropdownlist>
			            </div>
			                <two:Input ID="TwoInputUrl1" runat="server" Text="$page_url"  ValidationGroup="PageSett" />
			            <two:ValidInput ValidationType="integer" ID="TwoInputMenuGroup" runat="server" Text="$page_menu_group" ValidationGroup="PageSett" />    
			            <asp:Panel runat="server" ID="PanelMenuGroups" CssClass="select">
			                <asp:Label ID="LabelMenuGroups" Text="$page_menu_group" runat="server" AssociatedControlID="DropDownListMenuGroups" />
			                <asp:DropDownList ID="DropDownListMenuGroups" runat="server" />
			            </asp:Panel>
			            <two:LabeledCheckBox ID="LabeledCheckBoxBreakPersistance" runat="server" Text="$break_persistance" />
			            <two:Input  ID="InputRedirectToUrl" runat="server" Text="$redirectToUrl" ValidationGroup="PageSett" Required="false" />    
			            
			            <two:Input  ID="InputView" runat="server" Text="$viewGroups" ValidationGroup="PageSett" Required="false" />    
			            <two:Input  ID="InputEdit" runat="server" Text="$editGroups" ValidationGroup="PageSett" Required="false" />    
			            <two:LabeledCheckBox  ID="LabeledCheckBoxSSL" runat="server" Text="$requireSSL" />    
			            
				        <div class="save">
				            <asp:button	id="cmdSave" Runat="server"	CssClass="button" Text="$update" onclick="cmdSave_Click" ValidationGroup="PageSett" />
				        </div>
			        </div>

                    <div class="pagesettings">
			            <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsPageSettings" runat="server" Mode="Page" Text="$label_page_settings"  />	
			            <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsWebSite" runat="server" Mode="WebSite" Text="$label_website_settings" Visible="false" />
    			    </div>
		        </fieldset>
		        <%-- *************************************** Add Page ********************************************* --%>
		        <asp:PlaceHolder id="divAddInstance" runat="server" Visible="false" EnableViewState="true"	>
		        <fieldset>
			        <legend><asp:Literal ID="LiteralAddInstance" runat="server" EnableViewState="false"></asp:Literal></legend>
			        <div class="select">
				        <asp:Label id="lblAddModuleInstance" Runat="server" Text="$add_module_instance" AssociatedControlID="ddlModuleTypes" />
				        <asp:dropdownlist id="ddlModuleTypes" Runat="server" />
			        </div>
			        <div class="save">
			            <asp:button	id="cmdAddInstance"	Runat="server" text="$add_module_instance_button" onclick="cmdAddInstance_Click" />
			        </div>
		        </fieldset>
		        </asp:PlaceHolder>					
        		
    		    <%-- *************************************** Module instances ********************************************* --%>	
		        <fieldset class="fieldsetInstances">
		            <legend><asp:Literal ID="LiteralModulesOnPage" runat="server" EnableViewState="false"></asp:Literal></legend>
    		        
		            <asp:Repeater ID="RepeaterModuleInstances" runat="server" OnItemDataBound="RepaterModuleInstances_ItemDataBound" OnItemCommand="RepeaterModuleInstances_ItemCommand" >
		                <ItemTemplate>
		                    <div class="statusBars">
		                        <div class="instanceButtons">
		                                
		                                <input type="button" onclick="javascript:swapIt('edit_fields_<%# Eval("Id")	%>', 'show_fields_<%# Eval("Id")	%>')" value="<%# ResourceManager.GetString("$change_position") %>" style="display: <%# (bool) Eval("IsInherited") ? "none" : "inline"  %>;" />
		                                <div class="left">
		                                    <%# (bool) Eval("IsInherited") ? ResourceManager.GetString("$inherited") : "" %>
						                    <asp:LinkButton	ID="cmdDeleteInstance" Runat="server" CommandName="COMMAND_DELETE" CommandArgument='<%# Eval("Id") %>' Text='$label_delete_instance'	/>
						                </div>
						                <div class="left" style="margin-left: 8px;">
						                <%# RenderModuleName(Eval("Changed"), Eval("PendingDelete")) %>
							            [<%# Eval("Id")	%>]
						                </div>
						                <div class="right" style="width: 40px;">
						                    <asp:LinkButton	ID="cmdUndeleteInstance" Runat="server" CommandName="COMMAND_UNDELETE" CommandArgument='<%#	Eval("Id")	 %>' Text='$label_undelete_mi'	/>
						                    <asp:LinkButton	ID="cmdMoveUp" Runat="server" CommandName="COMMAND_MOVE_UP" CommandArgument='<%#	Eval("Id")	%>'	Text="$move_up"  />
						                    <asp:LinkButton	ID="cmdMoveDown" Runat="server"	CommandName="COMMAND_MOVE_DOWN" CommandArgument='<%# Eval("Id") %>' Text="$move_down"	 />
						                </div>
						        </div>
		                    </div>
		                    <div id="show_fields_<%# Eval("Id")	%>" class="instanceAllSettings">
		                          <div class="instancePosition">
		                              <table>
		                              <tbody>
						                    <tr>
						                        <td width="150px">
						                            <strong><%# ResourceManager.GetString("$" + Eval("Name"))%></strong>
						                            <br />
						                            <asp:label ID="LabelModuleDistinctName" runat="server" Visible="false" CssClass="ModuleDistinctName"></asp:label>
						                        </td>
						                        <td width="100px">
							                        <asp:Label ID="lblPlaceHolder" Runat="server" />
						                        </td>
						                        <td>
						                            <div>
						                                <asp:Label ID="Label3" runat="server" Text="$persistency" CssClass="instanceTitle"></asp:Label>
						                                <br />
    						                        
						                                <asp:Label ID="Label4" runat="server" Text="$l_from" CssClass="emph"></asp:Label>
							                            <asp:Label ID="lblPersistentFromDGrid" Runat="server" />
                									    
							                            <asp:Label ID="Label5" runat="server" Text="$l_to" CssClass="emph"></asp:Label>
							                            <asp:Label ID="lblPersistentToDGrid" Runat="server"	/>
                									    
							                        </div>
						                        </td>
						                        <td align="center">
						                            <div style="width: 50px;">
						                                <asp:ImageButton ID="cmdEditButton" runat="server" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>'  />
							                            <asp:LinkButton	ID="cmdEdit" Runat="server"	CssClass="edit_button" CommandName="COMMAND_EDIT_INSTANCE"	CommandArgument='<%# Eval("Id") %>' Text='$label_edit'	/>
							                        </div>
						                        </td>
						                    </tbody>
						              </table>
						              <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="moduleSettings" runat="server" Text="$label_module_settings"/>
		                          </div>
		                    </div>
		                    <div id="edit_fields_<%#	Eval("Id")	%>" style="display:none;" class="instanceAllSettings">
		                        <div>
		                            <table class="instancePosition">
						                <tbody>
						                <tr>
							                <td width="150px">
						                            <strong><%# ResourceManager.GetString("$" + Eval("Name"))%></strong>
						                    </td>
							                <td>
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
							                </td>
						                </tr>
						                </tbody>
						                </table>
    		                            <asp:Button ID="cmdUpdateDetails" Runat="server" CssClass="sbutton" CommandName="COMMAND_SAVE_INSTANCE"	CommandArgument='<%# Eval("Id") %>' Text='$update' ValidationGroup="MI"  />
    		                       </div>
		                    </div>
		                </ItemTemplate>
		            </asp:Repeater>
		        </fieldset>
		    </div> <!-- end of contentArea -->
		</asp:PlaceHolder>
		<asp:PlaceHolder ID="PlaceHolderAddRootPage" runat="server" Visible="false">
		    <div class="statusBars">
		        <asp:Label runat="server" ID="LabelNoRoot" Text="$no_root_page"></asp:Label>
		    </div>
		</asp:PlaceHolder>
		<asp:PlaceHolder ID="PlaceHolderAddWebSite" runat="server" Visible="false">
		    <div class="statusBars">
		        <asp:Label runat="server" ID="LabelNoWebSite" Text="$no_web_site"></asp:Label>
		    </div>
		</asp:PlaceHolder>
	</div>
	
</asp:Content>
