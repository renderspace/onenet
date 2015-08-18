<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="RssFeeds.aspx.cs" Inherits="OneMainWeb.RssFeeds" Title="One.NET RSS" EnableEventValidation="false" %>
<%@ Import Namespace="One.Net.BLL.WebConfig"%>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />

        <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="TabMultiview_OnViewIndexChanged" ActiveViewIndex="0">
            <asp:View ID="View1" runat="server">
                <div class="adminSection">
			        <asp:Button ID="AddRssFeed" runat="server" Text="Add RSS feed" OnClick="AddRssFeed_Click" />
			    </div>

			    <div class="centerFull">
                    <div class="biggv">	

			            <asp:GridView ID="FeedsGridView" runat="server" PageSize="25" PageIndex="0"
				            PagerSettings-Mode="NumericFirstLast"
				            PagerSettings-LastPageText="$last"
				            PagerSettings-FirstPageText="$first"
				            PagerSettings-PageButtonCount="7"
				            AllowSorting="True"
				            AutoGenerateColumns="False"
				            DataSourceID="ObjectDataSourceRssFeedList"
				            CssClass="table table-hover"
				            DataKeyNames="Id"
				            OnSelectedIndexChanged="FeedsGridView_SelectedIndexChanged"
				            OnRowCommand="FeedsGridView_RowCommand">
				            <Columns>
					            <asp:TemplateField HeaderText="Id" SortExpression="id">
						            <ItemTemplate><%# Eval("Id")%></ItemTemplate>
					            </asp:TemplateField>					        
					            <asp:TemplateField HeaderText="Title" SortExpression="title">
						            <ItemTemplate><%# Eval("Title")%></ItemTemplate>
					            </asp:TemplateField>
					            <asp:TemplateField HeaderText="Type" SortExpression="type">
						            <ItemTemplate><%# Eval("Type")%></ItemTemplate>
					            </asp:TemplateField>
					            <asp:TemplateField HeaderText="Categories" SortExpression="categories">
						            <ItemTemplate><%# Eval("DisplayCategories") %></ItemTemplate>
					            </asp:TemplateField>
						        <asp:TemplateField>
						            <ItemTemplate>
						                <asp:LinkButton Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("Id") %>' ID="cmdDelete" runat="server" />
						            </ItemTemplate>
						        </asp:TemplateField>					        
						        <asp:TemplateField>
						            <ItemTemplate>
						                <asp:LinkButton Text="Edit" CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" runat="server" />
						            </ItemTemplate>
						        </asp:TemplateField>					        
					        </Columns>			    
			            </asp:GridView>
			           <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
					        EnablePaging="false" ID="ObjectDataSourceRssFeedList" runat="server" SelectMethod="SelectRssFeeds"
					        TypeName="OneMainWeb.RssFeedDataSource" DeleteMethod="Delete" OnSelecting="ObjectDataSourceRssFeedList_Selecting" 
					        SelectCountMethod="SelectRssFeedCount" SortParameterName="sortBy">
			           </asp:ObjectDataSource>			        
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="centerFull">
			        <div class="contentEntry">
			            <asp:TextBox ID="InputTitle" runat="server" Text="$title" />
			            <asp:TextBox ID="InputDescription" Rows="4" TextMode="MultiLine" runat="server" Text="$description" />
			            <asp:TextBox ID="InputLinkToList" runat="server" Text="$link_to_list" />
			            <asp:TextBox ID="InputLinkToSingle" runat="server" Text="$link_to_single" />
			            <div class="select">
			                <asp:Label ID="lblProviders" runat="server" Text="$providers" AssociatedControlID="ddlProviders" />
			                <asp:DropDownList AutoPostBack="true" OnSelectedIndexChanged="ddlProviders_SelectedIndexChanged" ID="ddlProviders" runat="server" />
			            </div>
			            <div class="checkboxlist">
			                <asp:Label ID="lblCategories" runat="server" Text="$categories" AssociatedControlID="chlCategories" />
			                <asp:CheckBoxList ID="chlCategories" runat="server" />
			            </div>
			            <div class="form-group">
				            <asp:Button ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" />
				            <asp:Button ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" />
			            </div>
			        </div>
			    </div>	
            </asp:View>
            <asp:View ID="View3" runat="server"></asp:View>
            <asp:View ID="View4" runat="server"></asp:View>
        </asp:MultiView>
</asp:Content>
