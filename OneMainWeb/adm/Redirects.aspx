<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Redirects.aspx.cs" Inherits="OneMainWeb.Redirects" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <one:Notifier runat="server" ID="Notifier1" />
    <two:TabularMultiView ID="tabMultiview" runat="server" OnViewIndexChanged="tabMultiview_OnViewIndexChanged">
        <two:TabularView ID="tabListRedirects" runat="server" TabName="$redirect_list">
            <div class="searchFull">
                <asp:Button ID="cmdShowAddRedirect" runat="server" Text="$add_redirect" OnClick="cmdShowAddRedirect_Click" />
            </div>  
            <div class="centerFull">
                <div class="biggv">            
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
		                            <asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# Eval("Id") %>' Text='$cmd_edit' CommandName="Select"  /><br />
		                            <asp:LinkButton ID="LinkButton2" runat="server" CommandArgument='<%# Eval("Id") %>' Text='$cmd_delete' CommandName="Delete" />
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
                  </div>
              </div>                 
        </two:TabularView>
        <two:TabularView ID="tabSingleRedirect" runat="server" TabName="$redirect_single">
            <div class="centerFull">
                <div class="contentEntry">            
			        <two:Input id="InputFromLink" runat="server" Text="$from_link" ValidationGroup="REDIRECTS" />
			        <two:Input id="InputToLink" runat="server" Text="$to_link" ValidationGroup="REDIRECTS" />
			        <div class="save">
				        <asp:button ID="button3" runat="server" Text="$cancel" OnClick="CmdCancel_Click" ValidationGroup="CANCEL" />
				        <asp:button ID="button4" CommandName="SAVE" runat="server" Text="$save" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
				        <asp:button ID="button5" CommandName="SAVE_CLOSE" runat="server" Text="$save_close" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
			        </div>
			    </div>
			</div>
        </two:TabularView>        
    </two:TabularMultiView>

</asp:Content>
