<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="OneMainWeb.Publish" Title="$publishing" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<script language="javascript" type="text/javascript">

// select/deselect all checkboxes

function SelectAllCheckboxes(parentCheckBox)
{
    var children = parentCheckBox.children;
    var theBox = (parentCheckBox.type == "checkbox") ? parentCheckBox: parentCheckBox.children.item[0];
    var checkboxes = theBox.form.elements;
    for( i=0; i<checkboxes.length; i++)
    {
        if(checkboxes[i].type == "checkbox" && checkboxes[i].id != "gvProducts_ctl01_chkAll")
        {
            if(checkboxes[i].checked)
            {
                checkboxes[i].checked = false;
            }
            else
            {
                checkboxes[i].checked = true;
            }
        }
    } 
    theBox.checked = !theBox.checked;
}
</script>


	<one:Notifier runat="server" ID="Notifier1" />
    <asp:LinkButton ID="LinkButtonArticles" runat="server" OnClick="LinkButtonArticles_Click">Articles</asp:LinkButton>
    <asp:LinkButton ID="LinkButtonPages" runat="server" OnClick="LinkButtonPages_Click">Pages</asp:LinkButton>

    <asp:MultiView runat="server" ID="Multiview1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            <div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="articleGridView" runat="server" PageSize="25" PageIndex="0"
						    PagerSettings-Mode="NumericFirstLast"
						    PagerSettings-LastPageText="$last"
						    PagerSettings-FirstPageText="$first"
						    PagerSettings-PageButtonCount="7"
						    AllowSorting="True"
						    AllowPaging="True"
						    AutoGenerateColumns="False"
						    DataSourceID="ObjectDataSourceArticleList"
						    CssClass="gv"
						    DataKeyNames="Id"
						    OnRowCommand="articleGridView_RowCommand">
						    <Columns>
							    <asp:TemplateField HeaderText="$article" SortExpression="title">
								    <ItemTemplate><%# Eval("Title") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$display_date" SortExpression="display_date">
								    <ItemTemplate><%# ((DateTime)Eval("DisplayDate")).ToShortDateString()%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$RegularsList">
								    <ItemTemplate><%# Eval("RegularsList")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$LastChanged">
								    <ItemTemplate><%# Eval("LastChanged")%><br /><%# Eval("LastChangedBy")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
                                    <HeaderTemplate>
                                        <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                    </HeaderTemplate>								    
							        <ItemTemplate>
							            <asp:Literal ID="litArticleId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							            <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							        </ItemTemplate>
							    </asp:TemplateField>
						    </Columns>
						    <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />
					    </asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourceArticleList" runat="server" SelectMethod="SelectArticles"
						    TypeName="OneMainWeb.UnpublishedArticleDataSource" OnSelecting="ObjectDataSourceArticleList_Selecting" SelectCountMethod="SelectArticleCount" SortParameterName="sortBy">
				       </asp:ObjectDataSource>
			    </div>
    		    <asp:Button CssClass="right" ID="PublishArticlesButton" OnClick="PublishArticlesButton_Click" runat="server" CausesValidation="false" Text="$publish_selected" />
		    </div>

        </asp:View>
        <asp:View ID="View2" runat="server">
            <div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="pageGridView" runat="server" PageSize="25" PageIndex="0"
						    PagerSettings-Mode="NumericFirstLast"
						    PagerSettings-LastPageText="$last"
						    PagerSettings-FirstPageText="$first"
						    PagerSettings-PageButtonCount="7"
						    AllowSorting="True"
						    AllowPaging="True"
						    AutoGenerateColumns="False"
						    DataSourceID="ObjectDataSourcePageList"
						    CssClass="gv"
						    DataKeyNames="Id"
						    OnRowCommand="pageGridView_RowCommand">
						    <Columns>
							    <asp:TemplateField HeaderText="$id">
								    <ItemTemplate><%# Eval("Id") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$page">
								    <ItemTemplate><%# Eval("Title") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$LastChanged">
								    <ItemTemplate><%# Eval("LastChanged")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$LastChangedBy">
								    <ItemTemplate><%# Eval("LastChangedBy")%></ItemTemplate>
							    </asp:TemplateField>
							    							    
							    <asp:TemplateField>
                                    <HeaderTemplate>
                                        <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                    </HeaderTemplate>							    
							        <ItemTemplate>
							            <asp:Literal ID="litPageId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							            <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							        </ItemTemplate>
							    </asp:TemplateField>						    
						    </Columns>
						    <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />						    
						</asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourcePageList" runat="server" SelectMethod="SelectPages"
						    TypeName="OneMainWeb.UnpublishedPageDataSource" OnSelecting="ObjectDataSourcePageList_Selecting" SelectCountMethod="SelectPageCount" SortParameterName="sortBy">
				       </asp:ObjectDataSource>						
			    </div>
   		        <asp:Button CssClass="right" ID="PublishPagesButton" OnClick="PublishPagesButton_Click" runat="server" CausesValidation="false" Text="$publish_selected" />
			 </div>

        </asp:View>
    </asp:MultiView>

</asp:Content>
