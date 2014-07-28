<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Scaffold.aspx.cs" Inherits="OneMainWeb.adm.Scaffold" %>

<%@ Register TagPrefix="cc1" TagName="MainPagedList" Src="~/AdminControls/ScaffoldMainPagedList.ascx" %>
<%@ Register TagPrefix="cc2" TagName="VirtualTableList" Src="~/AdminControls/ScaffoldVirtualTableList.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:MultiView runat="server" ID="MultiView1">
        <asp:View ID="View1" runat="server">
            <cc1:MainPagedList runat="server" ID="MainPagedList1"></cc1:MainPagedList> 
        </asp:View>
        <asp:View ID="View2" runat="server">
            <asp:Label ID="Label1" runat="server" Text="$no_configuration_tables_found_do_you_want_to_create_them"></asp:Label>
            <asp:Button ID="ButtonCreateTables" runat="server" Text="$create_config_tables" onclick="ButtonCreateTables_Click" />
        </asp:View>
    </asp:MultiView>
</asp:Content>
