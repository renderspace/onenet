<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldVirtualTableList.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldVirtualTableList" %>
<%@ Import Namespace="OneMainWeb.AdminControls" %>

<asp:Repeater ID="RepeaterTables" runat="server">
<HeaderTemplate><ul></HeaderTemplate>
<FooterTemplate></ul></FooterTemplate>
<ItemTemplate>
    <li><a  <%# (((Request[OneMainWeb.adm.Scaffold.REQUEST_VIRTUAL_TABLE_ID] != null) && (Request[OneMainWeb.adm.Scaffold.REQUEST_VIRTUAL_TABLE_ID].ToString() == Eval("Id").ToString())) ? "class=\"selected\"" : "")  %>  href="/adm/Scaffold.aspx?<%# OneMainWeb.adm.Scaffold.REQUEST_VIRTUAL_TABLE_ID %>=<%# Eval("Id") %>"><%# Eval("FriendlyName") %></a></li>
</ItemTemplate>
</asp:Repeater>