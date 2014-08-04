<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentControl.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentControl" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<div class="form-group">

    <asp:Label AssociatedControlID="TextBoxTitle" ID="LabelTitle" Text="Title" runat="server" CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255"  CssClass="form-control"></asp:TextBox>
    </div>
</div>

<div class="form-group">
    <asp:Label AssociatedControlID="TextBoxSubTitle" ID="Label1" Text="Subtitle" runat="server" CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxSubTitle" MaxLength="255" CssClass="form-control"></asp:TextBox>
    </div>
</div>

<div class="form-group">
    <asp:Label AssociatedControlID="TextBoxTeaser" ID="Label2" Text="Teaser" runat="server"  CssClass="col-sm-3 control-label"></asp:Label>
    <div class="col-sm-9">
        <asp:TextBox runat="server" ID="TextBoxTeaser" TextMode="MultiLine"  Rows="5" CssClass="form-control"></asp:TextBox>
    </div>
</div>


<div class="form-group">
    <div class="col-sm-12">
        <asp:TextBox runat="server" ID="TextBoxHtml"  TextMode="MultiLine" Rows="20" CssClass="form-control"></asp:TextBox>
    </div>
</div>