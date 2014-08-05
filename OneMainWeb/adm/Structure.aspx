<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Structure.aspx.cs" Inherits="OneMainWeb.adm.Structure" Title="$structure" EnableEventValidation="false" ValidateRequest="false"  %>
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
                <header><h3 class="tabs_involved">Tree structure</h3>
                    <asp:Panel runat="server" ID="PanelAddSubPage" CssClass="addStuff">
                        <asp:TextBox runat="server" ID="TextBoxSubPage" placeholder="Add new page"></asp:TextBox>
                        <asp:LinkButton ID="ButtonAddSubPage" runat="server"  ValidationGroup="AddPage" text="<span class='glyphicon glyphicon-plus'></span> Add" onclick="cmdAddChild_Click" CssClass="btn btn-success" />
                    </asp:Panel>
                </header>
                <div class="treeview">
	                <asp:TreeView OnUnload="pageTree_Unload" EnableViewState="false" ID="pageTree" runat="server" OnAdaptedSelectedNodeChanged="pageTree_SelectedNodeChanged" OnSelectedNodeChanged="pageTree_SelectedNodeChanged" PopulateNodesFromClient="false" />
                </div>
            </section>
        </div>
        <div class="col-md-9">
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
			        <div class="pageproperties form-horizontal">
                         <div class="form-group">
                                <div class="col-sm-4">
                                <asp:Label AssociatedControlID="TextBoxTitle" ID="LabelTitle" Text="Title" runat="server"></asp:Label>
                            </div>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255" ValidationGroup="PageSett" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
                         <div class="form-group">
                        <div class="col-sm-4">
                                <asp:Label AssociatedControlID="" ID="LabelDescription" Text="Description" runat="server"></asp:Label>
                            </div>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxDescription" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="PageSett" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
			             <div class="form-group">
                        <div class="col-sm-4">
			                    <asp:label id="lblTemplate1" runat="server"	Text="Template" AssociatedControlID="ddlPageTemplate" />
                            </div>
                            <div class="col-sm-8">
				                <asp:dropdownlist id="ddlPageTemplate" Runat="server" CssClass="form-control"></asp:dropdownlist>
                            </div>
			            </div>
                        <div class="form-group">
                        <div class="col-sm-4">
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="TextBoxUri" Text="URL"></asp:Label>
                            </div>
                           <div class="col-sm-2">
                                <asp:Label runat="server" ID="LabelUriPart"></asp:Label>
                            </div>
                           <div class="col-sm-6">
                                <asp:TextBox runat="server" ID="TextBoxUri" ValidationGroup="PageSett" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                         <div class="form-group">
                            <div class="col-sm-4">
                                <asp:Label AssociatedControlID="TextBoxMenuGroup" ID="Label2" Text="Menu group" runat="server"></asp:Label>
                            </div>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="TextBoxMenuGroup" MaxLength="2" ValidationGroup="PageSett" type="number" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        

                         <div class="form-group">
                        <div class="col-sm-4">
                                <asp:Label AssociatedControlID="InputRedirectToUrl1" ID="Label7" Text="Redirect to URL" runat="server"></asp:Label>
                            </div>
                            <div class="col-sm-8">
                                <asp:TextBox runat="server" ID="InputRedirectToUrl1" MaxLength="255" ValidationGroup="PageSett" type="url" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="checkbox">
                            <label class="col-sm-offset-4 col-sm-8">
                                <asp:CheckBox runat="server" ID="CheckBoxBreakPersitence"  ValidationGroup="PageSett" />
                                Break persistance
                            </label>
                        </div>

 
                        <one:onesettings OnSettingsSaved="moduleSettings_SettingsSaved" ID="OneSettingsPageSettings" runat="server" Mode="Page" Text="Page settings" DisplayCommands="false"  />	
                        
			        </div>
                    
				    <div class="form-group">
                        <div class="col-sm-4">
						    <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
                        </div>

                        <div class="col-sm-8">
					        <asp:LinkButton id="ButtonDelete" CssClass="left btn btn-danger" Runat="server" Text="Delete" onclick="cmdDelete_Click" Visible="false" />
			                <asp:LinkButton ID="ButtonUndoDelete" CssClass="left btn btn-info" runat="server" Text="Undelete" OnClick="cmdUnDelete_Click" Visible="false" />
			                <asp:LinkButton ID="ButtonPublish"  runat="server" OnClick="ButtonPublish_Click" Cssclass="right btn-success btn" />				     
			                <asp:LinkButton ID="ButtonUnPublish" CssClass="right btn btn-info" runat="server" OnClick="ButtonUnPublish_Click" />
                            <asp:LinkButton	id="cmdSave" Runat="server"	CssClass="btn-success btn" Text="Save page" onclick="cmdSave_Click" ValidationGroup="PageSett" />
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
                     <asp:Label runat="server" ID="LabelNoRoot" Text="Website doesn't have a root page. Use form on the left to add it."></asp:Label>
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
