<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="FileManager.aspx.cs" Inherits="OneMainWeb.FileManager" Title="$file_manager" ValidateRequest="false" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register TagPrefix="one" TagName="FileManager" Src="~/AdminControls/FileManager.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <div class="topStructure">
            <asp:checkbox id="chkExpandTree" AutoPostBack="true" Runat="server"	Text="$expand_tree" OnCheckedChanged="chkExpandTree_CheckedChanged" /><br/>
		    <asp:checkbox id="CheckBoxEnableDelete" AutoPostBack="true" Runat="server" text="$enable_delete" OnCheckedChanged="ChkEnableDelete_CheckedChanged" />
        </div>
        <one:FileManager runat="server" ID="filemng2" ShowSelectLink="false" />
</asp:Content>
