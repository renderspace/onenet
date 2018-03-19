<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles2.aspx.cs" Inherits="OneMainWeb.adm.Articles2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
    <script src="https://cdn.ckeditor.com/4.6.1/full/ckeditor.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="app"></div>
    <script src="/ClientApp/dist/build.js"></script>
</asp:Content>
