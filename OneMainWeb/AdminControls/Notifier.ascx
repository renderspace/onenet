<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Notifier.ascx.cs" Inherits="OneMainWeb.AdminControls.Notifier" %>
<asp:Panel runat="server" ID="PanelNotifierError" CssClass="alert alert-warning" Visible="false"  EnableViewState="false">
    <h2><asp:Label ID="Label5" runat="server" EnableViewState="false"></asp:Label></h2>
    <p><asp:Label ID="Label6" runat="server" EnableViewState="false"></asp:Label></p>
</asp:Panel>
<asp:Panel runat="server" ID="PanelNotifierSuccess" CssClass="alert alert-success" Visible="false"  EnableViewState="false">
    <h2><asp:Label ID="Label1" runat="server" EnableViewState="false"></asp:Label></h2>
    <p><asp:Label ID="Label2" runat="server" EnableViewState="false"></asp:Label></p>
</asp:Panel>
<asp:Panel runat="server" ID="PanelNotifierWarning" CssClass="alert alert-warning" Visible="false"  EnableViewState="false">
    <h2><asp:Label ID="Label3" runat="server" EnableViewState="false"></asp:Label></h2>
    <p><asp:Label ID="Label4" runat="server" EnableViewState="false"></asp:Label></p>
</asp:Panel>
