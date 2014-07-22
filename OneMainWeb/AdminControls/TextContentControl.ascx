<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentControl.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentControl" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<two:Input Text="Title" Required="false" runat="server" ID="txtTitle" />
<two:Input Text="Subtitle" Required="false" runat="server" ID="txtSubTitle" />
<two:Input Text="Teaser" TextMode="MultiLine" Rows="5" Required="false" runat="server" ID="txtTeaser" />
<two:Input ID="txtHtml" runat="server" TextMode="MultiLine" Rows="25" Required="false" ContainerCssClass="ck" />