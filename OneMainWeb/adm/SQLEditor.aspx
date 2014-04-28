<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SQLEditor.aspx.cs" Inherits="OneMainWeb.adm.SQLEditor" Title="$sql_edit" %>

<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <uc1:Notifier ID="Notifier1" runat="server" />
    <div class="topStructure">

    </div>
    
    <div class="searchFull">
<%--
		<two:DropDownList id="DropDownConnection" runat="server" />
--%>
        <asp:DropDownList id="DropDownConnection" runat="server" />
	    <div class="input"><asp:TextBox id="txtSql" runat="server" TextMode="MultiLine" Width="100%" Rows="10" /></div>
	    <div class="save"><asp:Button id="cmdExec" runat="server" OnClick="CmdExec_Click" Text="$execute" /></div>
    </div>
    <div class="centerFull">      
        
	
		<div class="biggv">   
			<asp:DataGrid id="sqlGrid" Runat="server" AutoGenerateColumns="true" CssClass="gv" BorderWidth="0">
				<AlternatingItemStyle CssClass="talt" />
				<HeaderStyle CssClass="theader" />
				<FooterStyle CssClass="tfooter" />
			</asp:DataGrid>
		</div>
    </div>
		
</asp:Content>
