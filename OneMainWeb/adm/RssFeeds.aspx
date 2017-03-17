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
			        <asp:LinkButton ID="AddRssFeed" runat="server" OnClick="AddRssFeed_Click" text="<span class='glyphicon glyphicon-plus'></span> Add RSS" CssClass="btn btn-success" />			
			    </div>

			    <div class="centerFull">
                    <div class="biggv">	

			            <asp:GridView ID="FeedsGridView" runat="server" PageSize="25" PageIndex="0"
				            PagerSettings-Mode="NumericFirstLast"
				            PagerSettings-LastPageText="Last"
				            PagerSettings-FirstPageText="First"
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
                <div class="adminSection form-horizontal validationGroup">

                    <div class="form-group">
                         <asp:Label AssociatedControlID="InputTitle" runat="server" ID="LabelInputTitle" Text="Title" Cssclass="col-sm-3 control-label"></asp:Label>
                         <div class="col-sm-9">
			                <asp:TextBox ID="InputTitle" runat="server" Text="" ClientIDMode="Static" CssClass="form-control" />
                         </div>
                    </div>
                    <div class="form-group">
                        <asp:Label AssociatedControlID="InputDescription" runat="server" ID="Label1" Text="Description" Cssclass="col-sm-3 control-label"></asp:Label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="InputDescription" Rows="4" TextMode="MultiLine" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <asp:Label AssociatedControlID="InputLinkToList" runat="server" ID="Label2" Text="Link to list" Cssclass="col-sm-3 control-label"></asp:Label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="InputLinkToList" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <asp:Label AssociatedControlID="InputLinkToSingle" runat="server" ID="Label3" Text="Link to single" Cssclass="col-sm-3 control-label"></asp:Label>
                        <div class="col-sm-9">
                             <asp:TextBox ID="InputLinkToSingle" runat="server"  CssClass="form-control" />
                        </div>
                    </div>

			        <div class="form-group">
                        <asp:Label ID="lblProviders" runat="server" Text="Providers" AssociatedControlID="ddlProviders" CssClass="col-sm-3 control-label" />            
                        <div class="col-sm-9">
                            <asp:DropDownList AutoPostBack="true" OnSelectedIndexChanged="ddlProviders_SelectedIndexChanged" ID="ddlProviders" runat="server" CssClass="form-control" />
                        </div>
			        </div>
			        <div class="form-group">
                        <label class="col-sm-3 control-label">Categories</label>
                        <div class="col-sm-9">
                            <asp:CheckBoxList ID="chlCategories" runat="server" />
                        </div>
			        </div>
			        <div class="form-group">
                        <div class="col-sm-12">
				            <asp:LinkButton ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" CssClass="btn btn-success causesValidation" Text="Save" />
				            <asp:LinkButton  ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" CssClass="btn btn-success causesValidation" Text="Save and close" />
                        </div>
			        </div>
                    
                </div>
			        
            </asp:View>
        </asp:MultiView>
</asp:Content>
