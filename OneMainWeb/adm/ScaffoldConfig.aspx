<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="ScaffoldConfig.aspx.cs" Inherits="OneMainWeb.adm.ScaffoldConfig" %>
<%@ Register TagPrefix="cc1" TagName="ScaffoldConfig" Src="~/AdminControls/ScaffoldConfig.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <cc1:ScaffoldConfig runat="server" ID="ScaffoldConfig1"></cc1:ScaffoldConfig>
</asp:Content>
