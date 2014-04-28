<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Scaffold.aspx.cs" Inherits="OneMainWeb.adm.Scaffold" %>

<%@ Register TagPrefix="cc1" TagName="MainPagedList" Src="~/AdminControls/ScaffoldMainPagedList.ascx" %>
<%@ Register TagPrefix="cc2" TagName="VirtualTableList" Src="~/AdminControls/ScaffoldVirtualTableList.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:PlaceHolder ID="PlaceHolderControls" runat="server">

        <section class="column">
            <article class="module width_full tall">
                <header><h3 class="tabs_involved">bka bkaaa Manager</h3></header>
                <cc1:MainPagedList runat="server" ID="MainPagedList1"></cc1:MainPagedList>        
            </article>
        </section>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="PlaceHolderConfig" runat="server">
        <asp:Label ID="Label1" runat="server" Text="$no_configuration_tables_found_do_you_want_to_create_them"></asp:Label>
        <asp:Button ID="ButtonCreateTables" runat="server" Text="$create_config_tables" onclick="ButtonCreateTables_Click" />
    </asp:PlaceHolder>

</asp:Content>
