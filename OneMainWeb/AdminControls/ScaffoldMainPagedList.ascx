<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldMainPagedList.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldMainPagedList" %>

<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register src="~/AdminControls/ScaffoldDynamicEditor.ascx" tagname="ScaffoldDynamicEditor" tagprefix="uc1" %>

<div class="top">
    <cc1:Notifier runat="server" ID="Notifier1" />
</div>  
<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
    <asp:View ID="View1" runat="server">
        <div class="listing">
            <div class="buttonsTop">
                <asp:Button ID="ButtonInsert" runat="server" onclick="ButtonInsert_Click"  Text="$add_new_item" />
                <asp:Button ID="ButtonExportToExcel" runat="server" onclick="ButtonExportToExcel_Click" Text="$export_to_excel" />
            </div>
            <asp:GridView ID="GridViewItems" runat="server" AllowSorting="True" 
                AutoGenerateColumns="false" 
                onselectedindexchanged="GridViewItems_SelectedIndexChanged" 
                OnSorting="GridViewItems_Sorting">
            </asp:GridView>                
            <div class="buttonsMiddle">
                <asp:Button ID="ButtonDeleteSelected" runat="server" Text="$delete_selected_items" onclick="ButtonDeleteSelected_Click" />
            </div>
            <bll:PostbackPager id="PostbackPager1" OnCommand="PostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
            <asp:Literal ID="Literal1" runat="server" Text=""></asp:Literal>
        </div>    
    </asp:View>
    <asp:View ID="View2" runat="server">
        <uc1:ScaffoldDynamicEditor ID="DynamicEditor1" runat="server" OnExit="DynamicEditor1_Canceled" OnSaved="DynamicEditor1_Saved"></uc1:ScaffoldDynamicEditor>
    </asp:View>
</asp:MultiView>