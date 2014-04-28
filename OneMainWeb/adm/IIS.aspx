<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="IIS.aspx.cs" Inherits="OneMainWeb.adm.IIS" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="one" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <one:Notifier runat="server" ID="notifier" />

    <two:TabularMultiView ID="tabMultiview" runat="server" OnViewIndexChanged="TabMultiview_OnViewIndexChanged">
		<two:TabularView ID="TabularView2" runat="server" TabName="$app_pools">
			<div class="centerFull">
			    <div class="biggv">  
				    <asp:GridView	ID="GridViewAppPools" OnRowDataBound="GridViewAppPools_RowDataBound"
								    runat="server"
								    CssClass="gv"
								    AutoGenerateColumns="false"
								    AllowPaging="false"
								    AllowSorting="false"
								    DataKeyNames="Name">
					    <Columns>
                            <asp:BoundField HeaderText="$title" DataField="Name" />
                            <asp:TemplateField HeaderText="$app_pool_state">
                                <ItemTemplate>
                                    <asp:Literal ID="LiteralState" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div> 
            <div class="centerFull">
			    <div class="contentEntry">
                    <two:Input ValidationGroup="apppool" Required="true" Text="$app_pool_name" runat="server" ID="InputAppPoolName" />
                    <two:Input ValidationGroup="apppool" Required="true" Text="$app_pool_dot_net_runtime" runat="server" ID="InputAppPoolRuntime" Value="v4.0" />
			        <div class="save">
				        <asp:Button ValidationGroup="apppool" ID="AddAppPoolButton" runat="server" CausesValidation="true" OnClick="CreateNewAppPool_Click" Text="$add_new_pool" />
			        </div>
			    </div>
			</div>	                       
        </two:TabularView>        
		<two:TabularView ID="TabularView0" runat="server" TabName="$websites">
			<div class="centerFull">
			    <div class="biggv">  
				    <asp:GridView	ID="GridViewWebsites" OnRowDataBound="GridViewWebsites_RowDataBound"
								    runat="server"
								    CssClass="gv"
								    AutoGenerateColumns="false"
								    AllowPaging="false"
								    AllowSorting="false"
								    DataKeyNames="Name">
					    <Columns>
                            <asp:BoundField HeaderText="$title" DataField="Name" />
                            <asp:TemplateField HeaderText="$app_pool">
                                <ItemTemplate>
                                    <asp:Literal ID="LiteralAppPool" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="$site_path">
                                <ItemTemplate>
                                    <asp:Literal ID="LiteralPath" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>    
            <div class="centerFull">
			    <div class="contentEntry">
                    <div class="select">
                        <asp:Label runat="server" ID="LabelAppPools" AssociatedControlID="DropDownListAppPools"></asp:Label>
                        <asp:DropDownList ID="DropDownListAppPools" runat="server" />
                    </div>
                    <two:Input ValidationGroup="website" Required="true" Text="$web_site_name" runat="server" ID="InputWebSiteName" />
			        <div class="save">
				        <asp:Button ValidationGroup="website" ID="AddWebSiteButton" runat="server" CausesValidation="true" OnClick="CreateNewWebSite_Click" Text="$add_new_site" />
			        </div>
			    </div>
			</div>	                      
        </two:TabularView>   
    </two:TabularMultiView>
</asp:Content>
