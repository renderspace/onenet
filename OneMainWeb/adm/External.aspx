<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" ValidateRequest="false" AutoEventWireup="true" CodeBehind="External.aspx.cs" Inherits="OneMainWeb.External" Title="One.NET admin" EnableEventValidation="false" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
</asp:Content>
