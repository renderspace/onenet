<%@ Page Title="One.NET admin" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Scaffold.aspx.cs" Inherits="OneMainWeb.adm.Scaffold" %>
<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register src="~/AdminControls/ScaffoldDynamicEditor.ascx" tagname="ScaffoldDynamicEditor" tagprefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    

<cc1:Notifier runat="server" ID="Notifier1" />

<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
    <asp:View ID="View1" runat="server">

        <div class="adminSection">
			<div class="col-md-2">
                <asp:LinkButton ID="ButtonInsert" runat="server" onclick="ButtonInsert_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add" CssClass="btn btn-success" />
			</div>
			<div class="col-md-6 validationGroup">
                <asp:TextBox ID="TextBoxId" runat="server" placeholder="Search by ID" CssClass="required digits "></asp:TextBox>
                <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Display by id" OnClick="ButtonDisplayById_Click" CssClass="btn btn-info causesValidation" />
			</div>
			<div class="col-md-4">
                <asp:LinkButton ID="ButtonExportToExcel" runat="server" onclick="ButtonExportToExcel_Click" Text="Export to Excel" CssClass="btn btn-info" />
			 </div>
        </div>


        <asp:GridView ID="GridViewItems" runat="server" AllowSorting="True"  CssClass="table table-hover table-clickable-row"
            AutoGenerateColumns="false" 
            onselectedindexchanged="GridViewItems_SelectedIndexChanged" 
            OnSorting="GridViewItems_Sorting">
        </asp:GridView>                

        <div class="text-center">
            <bll:PostbackPager id="PostbackPager1" OnCommand="PostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
        </div>
        <div class="form-group">
            <div class="col-sm-12">
                <asp:LinkButton ID="ButtonDeleteSelected" runat="server" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" onclick="ButtonDeleteSelected_Click" CssClass="btn btn-danger" />
            </div>
        </div>

    </asp:View>
    <asp:View ID="View2" runat="server">
        <uc1:ScaffoldDynamicEditor ID="DynamicEditor1" runat="server" OnExit="DynamicEditor1_Canceled" OnSaved="DynamicEditor1_Saved" OnError="DynamicEditor1_Error"></uc1:ScaffoldDynamicEditor>
    </asp:View>
</asp:MultiView>
    

</asp:Content>
