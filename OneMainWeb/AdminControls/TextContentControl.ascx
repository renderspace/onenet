<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentControl.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentControl" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary"  %>

<two:Input Text="$title" Required="false" runat="server" ID="txtTitle" />
<two:Input Text="$sub_title" Required="false" runat="server" ID="txtSubTitle" />
<two:Input Text="$teaser" TextMode="MultiLine" Rows="5" Required="false" runat="server" ID="txtTeaser" />
<two:Input ID="txtHtml" runat="server" TextMode="MultiLine" Rows="25" Required="false" ContainerCssClass="ck" />