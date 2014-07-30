<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldMainPagedList.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldMainPagedList" %>

<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register src="~/AdminControls/ScaffoldDynamicEditor.ascx" tagname="ScaffoldDynamicEditor" tagprefix="uc1" %>

<cc1:Notifier runat="server" ID="Notifier1" />

<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
    <asp:View ID="View1" runat="server">

        <div class="searchFull">
			    <div class="col-md-2">
                    <asp:LinkButton ID="ButtonInsert" runat="server" onclick="ButtonInsert_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add" CssClass="btn btn-success" />
			    </div>
			    <div class="col-md-6">
                    <asp:TextBox ID="TextBoxId" runat="server" placeholder="Search by ID"></asp:TextBox>
                    <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Display by id" OnClick="ButtonDisplayById_Click" CssClass="btn btn-info" />
			    </div>
			    <div class="col-md-4">
                    <asp:LinkButton ID="ButtonExportToExcel" runat="server" onclick="ButtonExportToExcel_Click" Text="Export to Excel" CssClass="btn btn-info" />
			     </div>
            </div>

        <div class="listing">
            <div>
                
                
                
            </div>
            <asp:GridView ID="GridViewItems" runat="server" AllowSorting="True" 
                AutoGenerateColumns="false" 
                onselectedindexchanged="GridViewItems_SelectedIndexChanged" 
                OnSorting="GridViewItems_Sorting" CssClass="table table-hover">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" Text="<span class='glyphicon glyphicon-pencil'></span> Edit" CommandName="select" CssClass="btn btn-info btn-xs" OnClick="GridViewItems_SelectedIndexChanged" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>                
            <div class="buttonsMiddle">
                <asp:LinkButton ID="ButtonDeleteSelected" runat="server" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" onclick="ButtonDeleteSelected_Click" CssClass="btn btn-danger" />
            </div>
            <bll:PostbackPager id="PostbackPager1" OnCommand="PostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
            <asp:Literal ID="Literal1" runat="server" Text=""></asp:Literal>
        </div>    
    </asp:View>
    <asp:View ID="View2" runat="server">
        <uc1:ScaffoldDynamicEditor ID="DynamicEditor1" runat="server" OnExit="DynamicEditor1_Canceled" OnSaved="DynamicEditor1_Saved"></uc1:ScaffoldDynamicEditor>
    </asp:View>
</asp:MultiView>