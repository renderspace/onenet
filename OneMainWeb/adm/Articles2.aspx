<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles2.aspx.cs" Inherits="OneMainWeb.adm.Articles2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
    <!-- script src="//cdn.ckeditor.com/4.6.2/full/ckeditor.js"></!-->
    <script src="/ckeditor5/ckeditor.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="app"></div>
    <script src="/ClientApp/dist/build.js"></script>
</asp:Content>
