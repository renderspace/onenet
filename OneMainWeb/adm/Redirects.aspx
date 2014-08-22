<%@ Page Title="One.NET Redirects" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Redirects.aspx.cs" Inherits="OneMainWeb.Redirects" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged">
        <asp:View ID="View1" runat="server">
            <div class="adminSection">
                <div class="col-md-2">
                    <asp:Button ID="cmdShowAddRedirect" runat="server" Text="$add_redirect" CssClass="btn btn-success" OnClick="cmdShowAddRedirect_Click" />
                </div>
            </div>  
            <asp:GridView ID="GridViewRedirects" runat="server" PageSize="10" PageIndex="0"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-LastPageText="$last"
                PagerSettings-FirstPageText="$first"
                PagerSettings-PageButtonCount="7" 
                AllowSorting="True" 
                AllowPaging="True" 
                AutoGenerateColumns="False"
                DataSourceID="RedirectListSource" 
                DataKeyNames="Id"
                CssClass="table table-hover"
                OnRowCommand="GridViewRedirects_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="$from_link" SortExpression="from_link">
	                    <ItemTemplate><%# Eval("FromLink") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="$to_link" SortExpression="to_link">
	                    <ItemTemplate><%# Eval("ToLink") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
	                    <ItemTemplate>
		                    <asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# Eval("Id") %>' Text='$cmd_edit' CommandName="Select" CssClass="btn btn-info btn-xs  "  />
		                    <asp:LinkButton ID="LinkButton2" runat="server" CommandArgument='<%# Eval("Id") %>' Text='$cmd_delete' CommandName="Delete" CssClass="btn btn-warning btn-xs  " />
	                    </ItemTemplate>
                    </asp:TemplateField>                        
                </Columns>
                <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
                    PageButtonCount="7" />                        
            </asp:GridView>

            <asp:ObjectDataSource OnDeleted="RedirectListSource_Deleted" MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
                EnablePaging="True" ID="RedirectListSource" runat="server" SelectMethod="Select"
                TypeName="OneMainWeb.RedirectHelper" DeleteMethod="DeleteRedirect" OnSelecting="RedirectListSource_Selecting" SelectCountMethod="SelectCount" SortParameterName="sortBy">
                <SelectParameters>
                    <asp:Parameter Name="sortDirection" DefaultValue="ASC" Type="string" />
                </SelectParameters> 
            </asp:ObjectDataSource>                     
     
        </asp:View>
        <asp:View ID="View2" runat="server">
             <div class="adminSection form-horizontal validationGroup">
                <div class="form-group">
                    <asp:Label runat="server" Text="$from_link" CssClass="col-sm-3 control-label" AssociatedControlID="InputFromLink" />
			        <div class="col-sm-9">
                        <asp:TextBox CssClass="form-control required" id="InputFromLink" runat="server" Text="$from_link" ValidationGroup="REDIRECTS" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="InputFromLink" CssClass="text-danger" ValidationGroup="REDIRECTS" ErrorMessage="From link is required." />
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" Text="$to_link" CssClass="col-sm-3 control-label" AssociatedControlID="InputToLink" />
                    <div class="col-sm-9">
			            <asp:TextBox CssClass="form-control required" id="InputToLink" runat="server" Text="$to_link" ValidationGroup="REDIRECTS" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="InputToLink" CssClass="text-danger" ValidationGroup="REDIRECTS" ErrorMessage="To link is required." />
                    </div>
                </div>
			    <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
				        <asp:button CssClass="btn btn-success" ID="button3" runat="server" Text="$cancel" OnClick="CmdCancel_Click" ValidationGroup="CANCEL" />
				        <asp:button CssClass="btn btn-success causesValidation" ID="button4" CommandName="SAVE" runat="server" Text="Save" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
				        <asp:button CssClass="btn btn-success causesValidation" ID="button5" CommandName="SAVE_CLOSE" runat="server" Text="Save and close" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
                    </div>
			    </div>
			</div>
        </asp:View>
    </asp:MultiView>
</asp:Content>
