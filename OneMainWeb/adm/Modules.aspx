<%@ Page Title="Modules" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Modules.aspx.cs" Inherits="OneMainWeb.adm.Modules" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <one:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="Multiview1" ActiveViewIndex="0" OnActiveViewChanged="Multiview1_ActiveViewChanged">
        <asp:View ID="View1" runat="server">
            <div class="adminSection validationGroup">
		    </div>
	        <asp:GridView ID="GridViewModules" runat="server" CssClass="gv" AutoGenerateColumns="false" AllowPaging="false" DataKeyNames="Id" OnRowCommand="GridViewModules_RowCommand" OnRowDataBound="GridViewModules_RowDataBound" >
		        <Columns>
			        <asp:BoundField HeaderText="Id" DataField="Id"  />
			        <asp:BoundField HeaderText="Module name" DataField="Name"  />
                    <asp:BoundField HeaderText="No. of unpublished instances" DataField="NoUnpublishedInstances" />
                    <asp:BoundField HeaderText="No. of published instances" DataField="NoPublishedInstances"  />
                    <asp:BoundField HeaderText="No. of settings in database" DataField="NoSettingsInDatabase"  />
                    <asp:BoundField HeaderText="No. of settings in file" DataField="NoSettingsInModule"  />
                    <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton  CommandName="Install" CommandArgument='<%# Eval("Name") %>' ID="LinkButtonInstall" runat="server" CssClass="btn btn-info btn-xs" CausesValidation="false" Text="Install" />
                                <asp:LinkButton  CommandName="UpdateSettings" CommandArgument='<%# Eval("Name") %>' ID="LinkButtonUpdateSettings" runat="server" CssClass="btn btn-info btn-xs" CausesValidation="false" Text="Update settings" />
                            </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton  CommandName="ShowInstances" CommandArgument='<%# Eval("Id") %>' ID="LinkButtonShowInstances" runat="server" CssClass="btn btn-default btn-xs" CausesValidation="false" Text="<span class='glyphicon glyphicon-pencil'></span> Show instances" />
                            </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton  CommandName="Delete" CommandArgument='<%# Eval("Name") %>' ID="LinkButtonDelete" runat="server" CssClass="btn btn-danger btn-xs" CausesValidation="false" Text=" Delete" />
                            </ItemTemplate>
                    </asp:TemplateField>
                    
		        </Columns>
	        </asp:GridView>
        </asp:View>
        <asp:View ID="View3" runat="server">
            <div class="adminSection form-horizontal validationGroup">

                <asp:GridView ID="GridViewUsage" runat="server" CssClass="gv" AutoGenerateColumns="false" AllowPaging="false" DataKeyNames="Id">
                    <Columns>
                        <asp:BoundField HeaderText="Id" DataField="Id"  />
                        <asp:TemplateField>
                                <ItemTemplate>
                                        <asp:HyperLink runat="server" ID="HyperLink1" Target="_blank" NavigateUrl='<%# Eval("url") %>' Text='<%# Eval("url") %>'></asp:HyperLink>
                                </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
			    
			    <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
				
                    </div>
			    </div>
			</div>
        </asp:View>
    </asp:MultiView>
        
</asp:Content>
