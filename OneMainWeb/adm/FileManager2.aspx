<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="FileManager2.aspx.cs" Inherits="OneMainWeb.FileManager2" Title="$file_manager" ValidateRequest="false" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register Assembly="CKFinder" Namespace="CKFinder" TagPrefix="CKFinder" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<script type="text/javascript">
	function ShowFileInfo(fileUrl, data) {
		fileUrl = unescape(fileUrl);
		var filesIndex = fileUrl.indexOf("/_files/");
		if (filesIndex > -1) {
			fileUrl = fileUrl.substring(filesIndex);
		}
		window.open(fileUrl);
		return false;
	}
	</script>
	<CKFinder:FileBrowser ID="FileBrowser1" BasePath="/ckfinder" SelectThumbnailFunction="ShowFileInfo" SelectFunction="ShowFileInfo" runat="server" Height="700" />
</asp:Content>