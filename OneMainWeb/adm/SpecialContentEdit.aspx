<%@ Page ValidateRequest="false"  Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SpecialContentEdit.aspx.cs" Inherits="OneMainWeb.adm.SpecialContentEdit" Title="One.NET Special content"  %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register src="../AdminControls/TextSpecialContent.ascx" tagname="TextSpecialContent" tagprefix="uc1" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:TextSpecialContent ID="TextSpecialContent1" runat="server" />
</asp:Content>