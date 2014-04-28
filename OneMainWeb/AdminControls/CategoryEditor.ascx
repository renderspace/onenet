<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CategoryEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.CategoryEditor" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="TextContentControl.ascx" %>

<one:TextContentControl ID="txtCategory" runat="server" />
<two:LabeledCheckBox ID="chkIsSelectable" runat="server" Text="$is_selectable" />
<two:LabeledCheckBox ID="chkIsPrivate" runat="server" Text="$is_private" />
<two:InfoLabel ID="InfoLabelID" runat="server" Text="$id" />
<div style=" margin-top: 5px; float: right;" class="save">
    <asp:Button ValidationGroup="FN" ID="cmdDeleteNode" cssclass="left" runat="server" Text="$delete" OnClick="cmdDeleteNode_Click" />				
    <asp:Button ValidationGroup="FN" ID="cmdUpdateNode" cssclass="right" runat="server" Text="$update" OnClick="cmdUpdateNode_Click" />
    
    <asp:Button runat="server" Visible="false" ID="cmdMoveCategory" Text="$move" onclick="cmdMoveCategory_Click" />                        
	<two:modalpanel Visible="false" OnWindowClosed="moveCategoryPanel_WindowClosed" ShowCloseButton="true" id="moveCategoryPanel" runat="server" >
	    <div class="outerBorder">
	        <div class="innerBorder">
	            <two:InfoLabel ID="lblMoveCategory" runat="server" Text="$category_to_move"></two:InfoLabel>
	            <div class="treeHolder noImages">
	                <asp:TreeView ID="moveCategoryTree" OnAdaptedSelectedNodeChanged="moveCategoryTree_SelectedNodeChanged" OnSelectedNodeChanged="moveCategoryTree_SelectedNodeChanged" runat="server" SelectedNodeStyle-BackColor="Gray" Width="320px" />
	            </div>   
	        </div>
	    </div>
	</two:modalpanel>         
    
</div>

<asp:GridView ID="assignedGrid" runat="server" AutoGenerateColumns="False" CssClass="gvsmall" DataKeyNames="Id">
	<Columns><asp:BoundField DataField="Title" HeaderText="$categorised_under_this_category" /></Columns>
</asp:GridView>

