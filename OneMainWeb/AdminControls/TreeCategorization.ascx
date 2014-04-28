<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TreeCategorization.ascx.cs" Inherits="OneMainWeb.AdminControls.TreeCategorization" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

	<div id="divAddChild" runat="server" style="width: 100%;">
	
	    <two:InputWithButton ID="InputWithButtonAddTreeNode" ValidationGroup="NNN1" runat="server" Text="$new_category" ButtonText="$add" OnClick="CmdAddTreeNode_Click"  />
		<br style="clear: both" />
	</div>
	<div style="width: 100%;">&nbsp;</div>
		        <br style="clear: both;" />   
	<div id="treeHolder" runat="server" class="treeHolder noImages">
		<asp:TreeView OnAdaptedSelectedNodeChanged="categoriesTree_SelectedNodeChanged" OnSelectedNodeChanged="categoriesTree_SelectedNodeChanged" ID="categoriesTree" runat="server" 
			BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" Width="270" />
	</div>


