<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles.aspx.cs" Inherits="OneMainWeb.Articles" Title="$articles" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register src="~/AdminControls/LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" tagprefix="uc2" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />


    <asp:MultiView ID="Multiview1" runat="server" ActiveViewIndex="0" OnActiveViewChanged="Multiview1_ActiveViewChanged">
        <asp:View ID="View1" runat="server">

            <div class="searchFull">
			    <div class="col-md-2">
			        <asp:LinkButton ID="cmdAddArticle" runat="server" text="<span class='glyphicon glyphicon-plus'></span> Add" OnClick="cmdAddArticle_Click" CssClass="btn btn-success" />			
			    </div>
			    <div class="col-md-6">
                    <asp:TextBox runat="server" ID="TextBoxShowById" placeholder="Search by ID or title" ValidationGroup="ShowById"></asp:TextBox>
                    <asp:LinkButton runat="server"  OnClick="cmdShowById_Click" ID="LinkButtonShowById" CssClass="btn btn-info" ValidationGroup="ShowById" Text="Search"></asp:LinkButton>
			    </div>
			    <div class="col-md-4">
                    <asp:DropDownList OnDataBound="ddlRegularFilter_DataBound" DataTextField="Title" DataValueField="Id" AppendDataBoundItems="False" ID="ddlRegularFilter" runat="server"   />                  
    	            <asp:LinkButton ID="cmdFilterArticles" runat="server" Text="Filter" OnClick="cmdFilterArticles_Click" CssClass="btn btn-info" />                                    
			     </div>
            </div>
			<div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="articleGridView" runat="server" PageSize="10" PageIndex="0"
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
						    OnSelectedIndexChanged="articleGridView_SelectedIndexChanged"
						    OnRowDataBound="articleGridView_RowDataBound"
						    OnRowDeleted="articleGridView_Deleted"
						    OnRowCommand="articleGridView_RowCommand">
						    <Columns>
							    <asp:TemplateField HeaderText="$article" SortExpression="cds.title">
								    <ItemTemplate><%# Eval("Title") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$display_date" SortExpression="a.display_date">
								    <ItemTemplate><%# ((DateTime)Eval("DisplayDate")).ToShortDateString()%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$id" SortExpression="a.id">
							        <ItemTemplate>
							            <asp:LinkButton Text="$delete" CommandName="Delete" CommandArgument='<%# Eval("Id") %>' ID="cmdDelete" runat="server" />
							            <br />
								        <%# Eval("Id") %>
							        </ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$RegularsList">
								    <ItemTemplate><%# Eval("RegularsList")%></ItemTemplate>
							    </asp:TemplateField>
							    
							    <asp:TemplateField HeaderText="$status">
								    <ItemTemplate>
								        <div class="publishButtons">
								            <asp:LinkButton Text="$publish" CommandName="Publish" CommandArgument='<%# Eval("Id") %>' ID="cmdPublish" runat="server" /><br />
								            <asp:LinkButton Text="$unpublish" CommandName="UnPublish" CommandArgument='<%# Eval("Id") %>' ID="cmdUnPublish" runat="server" /><br />
								            <img src='<%# RenderStatusIcons(Eval("MarkedForDeletion"), Eval("IsChanged")) %>' alt="" /><br />
										    <asp:LinkButton Text="$revert_to_published" CommandName="RevertToPublished" CommandArgument='<%# Eval("Id") %>' ID="cmdRevertToPublished" runat="server" />
										</div>
								    </ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
								    <ItemTemplate>
								        <div style="width: 50px;">
								            <asp:ImageButton ID="cmdEditButton" runat="server" CommandName="Select"	CommandArgument='<%# Eval("Id") %>'  />
										    <asp:LinkButton Text="$edit" CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" runat="server" />
										</div>
								    </ItemTemplate>
							    </asp:TemplateField>
						    </Columns>
						    <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />
					    </asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourceArticleList" runat="server" SelectMethod="SelectArticles"
						    TypeName="OneMainWeb.ArticleDataSource" OnSelecting="ObjectDataSourceArticleList_Selecting" DeleteMethod="MarkForDeletion" SelectCountMethod="SelectArticleCount" SortParameterName="sortBy">
						    <SelectParameters>
						        <asp:ControlParameter Name="strRegularId" ControlID="ddlRegularFilter" PropertyName="SelectedValue" />
						    </SelectParameters>
						    <DeleteParameters>
							    <asp:Parameter Name="Id" Type="Int32" />
						    </DeleteParameters>
				       </asp:ObjectDataSource>
			    </div>
		    </div>
        </asp:View>
        <asp:View ID="View2" runat="server">

            <script src="/Javascript/jquery.ui.datepicker-sl.js"></script>
            <script type="text/javascript" charset="utf-8">
                $(document).ready(function () {
                    $("#TextBoxDate").datepicker();
                });
            </script>

            <div class="searchFull">
				<table>
					<tr>
						<td style="width: 48%">
							<div class="select">
								<asp:ListBox ID="lbRegulars" runat="server" />
							</div>
						</td>
						<td>
							<asp:Button ValidationGroup="ATR" ID="cmdAssignRegularToArticle" runat="server" Text="$assign" OnClick="cmdAssignRegularToArticle_Click" /><br />
							<asp:Button ValidationGroup="ATR" ID="cmdRemoveRegularFromArticle" runat="Server" Text="$remove"
								OnClick="cmdRemoveRegularFromArticle_Click" />
						</td>
						<td style="width: 48%">
							<div class="listbox">
								<asp:Label ID="lblRegularAssignedToArticleMark" runat="server" Text="$article_regulars" />
								<asp:ListBox ID="lbRegularsAssignedToArticle" runat="server" Rows="5" />
							</div>
						</td>
					</tr>
				</table>
			</div>
            <div class="centerFullOuter">
			    <div class="centerFull">
			        <div class="contentEntry">
                        <div class="input">
                            <asp:Label AssociatedControlID="TextBoxDate" runat="server" ID="LabelDate" Text="$display_date"></asp:Label>
                            <asp:TextBox runat="server" ClientIDMode="Static" ID="TextBoxDate"></asp:TextBox>
                        </div>
                        <one:TextContentControl ID="TextContentEditor" runat="server" />
			            <div class="save">
			                <span>Id: </span><asp:Label CssClass="articleId" ID="LabelId" runat=server></asp:Label>
				            <asp:Button ID="CancelButton" runat="server" CausesValidation="false" OnClick="CancelButton_Click" Text="$cancel" />
				            <asp:Button ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" />
				            <asp:Button ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" />
				            <asp:Label ID="AutoPublishWarning" runat="server" Text="$autopublish_warning"></asp:Label>
			            </div>
			        
                        <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
			        
			        </div>
			    </div>
            </div>
        </asp:View>
        

    </asp:MultiView>
</asp:Content>
