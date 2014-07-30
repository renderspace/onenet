<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentControl.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentControl" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<div class="input">
    <div class="layout-left">
        <asp:Label AssociatedControlID="TextBoxTitle" ID="LabelTitle" Text="Title" runat="server"></asp:Label>
    </div>
    <div class="layout-right">
        <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255"></asp:TextBox>
    </div>
</div>

<div class="input">
    <div class="layout-left">
        <asp:Label AssociatedControlID="TextBoxSubTitle" ID="Label1" Text="Subtitle" runat="server"></asp:Label>
    </div>
    <div class="layout-right">
        <asp:TextBox runat="server" ID="TextBoxSubTitle" MaxLength="255"></asp:TextBox>
    </div>
</div>

<two:Input Text="Teaser" TextMode="MultiLine" Rows="5" Required="false" runat="server" ID="txtTeaser" />
<two:Input ID="txtHtml" runat="server" TextMode="MultiLine" Rows="20" Required="false" ContainerCssClass="ck" />