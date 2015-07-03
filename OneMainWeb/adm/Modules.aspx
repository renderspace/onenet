<%@ Page Title="Modules" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Modules.aspx.cs" Inherits="OneMainWeb.adm.Modules" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <one:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="Multiview1" ActiveViewIndex="0" OnActiveViewChanged="Multiview1_ActiveViewChanged">
        <asp:View ID="View1" runat="server">
            <div class="adminSection validationGroup">
                <asp:Button runat="server" ID="Button1" OnClick="Button1_Click" />
		    </div>
	        <asp:GridView ID="GridViewModules" runat="server" CssClass="gv" AutoGenerateColumns="false" AllowPaging="false" DataKeyNames="Id" OnSelectedIndexChanged="GridViewModules_SelectedIndexChanged" >
		        <Columns>
			        <asp:BoundField HeaderText="Id" DataField="Id"  />
			        <asp:BoundField HeaderText="Module name" DataField="Name"  />
                    <asp:BoundField HeaderText="No. of unpublished instances" DataField="NoUnpublishedInstances" />
                    <asp:BoundField HeaderText="No. of published instances" DataField="NoPublishedInstances"  />
                    <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton  CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="LinkButton1" runat="server" CssClass="btn btn-info btn-xs" CausesValidation="false" Text="<span class='glyphicon glyphicon-pencil'></span> Edit" />
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
