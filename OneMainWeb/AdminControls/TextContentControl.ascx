<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentControl.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentControl" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<asp:Panel CssClass="form-group" runat="server" ID="PanelTitle">
    <asp:Label AssociatedControlID="TextBoxTitle" ID="LabelTitle" Text="Title" runat="server" CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="4000"  CssClass="form-control"></asp:TextBox>
    </div>
</asp:Panel>
<asp:Panel CssClass="form-group" runat="server" ID="PanelSubTitle">
    <asp:Label AssociatedControlID="TextBoxSubTitle" ID="Label1" Text="Subtitle" runat="server" CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxSubTitle" MaxLength="255" CssClass="form-control"></asp:TextBox>
    </div>
</asp:Panel>
<asp:Panel CssClass="form-group" runat="server" ID="PanelTeaser">
    <asp:Label AssociatedControlID="TextBoxTeaser" ID="Label2" Text="Teaser" runat="server"  CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxTeaser" TextMode="MultiLine"  Rows="5" CssClass="form-control"></asp:TextBox>
    </div>
</asp:Panel>
<div class="form-group">
    <div class="col-sm-12">
        <asp:TextBox runat="server" ID="TextBoxHtml"  TextMode="MultiLine" Height="600"  CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
    </div>
</div>