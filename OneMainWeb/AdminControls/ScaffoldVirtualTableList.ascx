<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldVirtualTableList.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldVirtualTableList" %>
<%@ Import Namespace="OneMainWeb.AdminControls" %>

<asp:Repeater ID="RepeaterTables" runat="server">
<HeaderTemplate><ul></HeaderTemplate>
<FooterTemplate></ul></FooterTemplate>
<ItemTemplate>
    <li><a href="/adm/Scaffold.aspx?<%# ScaffoldMainPagedList.REQUEST_VIRTUAL_TABLE_ID %>=<%# Eval("Id") %>"><%# Eval("FriendlyName") %></a></li>
</ItemTemplate>
</asp:Repeater>