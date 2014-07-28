<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="FileManager.aspx.cs" Inherits="OneMainWeb.FileManager" Title="$file_manager" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register TagPrefix="one" TagName="FileManager" Src="~/AdminControls/FileManager.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="topStructure">
            <asp:checkbox id="chkExpandTree" AutoPostBack="true" Runat="server"	Text="$expand_tree" OnCheckedChanged="chkExpandTree_CheckedChanged" /><br/>
        </div>
        <one:FileManager runat="server" ID="filemng2" ShowSelectLink="false" />
</asp:Content>
