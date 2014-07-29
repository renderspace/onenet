<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="ContentEdit.aspx.cs" Inherits="OneMainWeb.adm.ContentEdit" Title="Content edit" ValidateRequest="false" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register src="../AdminControls/TextSpecialContent.ascx" tagname="TextSpecialContent" tagprefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:TextSpecialContent ID="TextSpecialContent1" runat="server" />
</asp:Content>