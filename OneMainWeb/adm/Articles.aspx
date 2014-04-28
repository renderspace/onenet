<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles.aspx.cs" Inherits="OneMainWeb.Articles" Title="$articles" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register Src="~/AdminControls/History.ascx" TagName="History" TagPrefix="uc1" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />
	<div class="topStructure">
		<asp:checkbox id="chkAutoPublish" OnCheckedChanged="chkAutoPublish_CheckedChanged" AutoPostBack="true" Runat="server" Text="$autopublish_label" />
		<asp:checkbox id="chkUseFck" OnCheckedChanged="chkUseFck_CheckedChanged" AutoPostBack="true" Runat="server" Text="$usefck_label" />		
		<asp:checkbox id="CheckboxShowUntranslated" OnCheckedChanged="CheckboxShowUntranslated_CheckedChanged" AutoPostBack="true" Runat="server" Text="$show_untranslated" />		
	</div>

	<two:TabularMultiView ID="tabMultiview" runat="server" OnViewIndexChanged="tabMultiview_OnViewIndexChanged">
		<two:TabularView ID="tabListArticles" runat="server" TabName="$article_list">
			<div class="searchFull">
			    <asp:Button ID="cmdAddArticle" runat="server" Text="$add_article" OnClick="cmdAddArticle_Click" />			
			</div>
			<div class="searchFull">
			    <two:InputWithButton ValidationType="int" ID="InputWithButtonShowById" ValidationGroup="ShowById" ButtonText="$show" Text="$article_ShowById" runat="server" OnClick="cmdShowById_Click" />
			</div>
			<div class="searchFull">
                <two:CleanDropDownList OnDataBound="ddlRegularFilter_DataBound" DataTextField="Title" DataValueField="Id" AppendDataBoundItems="False" ID="ddlRegularFilter" runat="server" DataSourceID="RegularSource"  />
                <asp:ObjectDataSource ID="RegularSource" runat="server" OnSelecting="ObjectDataSourceRegularSource_Selecting"
                    SelectMethod="ListRegulars" TypeName="OneMainWeb.ArticleDataSource">
                    <SelectParameters>
                        <asp:Parameter Name="sortBy" Type="string" DefaultValue="id" />
                    </SelectParameters>
                </asp:ObjectDataSource>                     
    	        <asp:Button ID="cmdFilterArticles" runat="server" Text="$filter_articles" OnClick="cmdFilterArticles_Click" />                                    
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
		</two:TabularView>

		<two:TabularView ID="tabViewEditArticle" runat="server" TabName="$view_edit_article">
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
			<div class="centerFull">
			    <div class="contentEntry">
			        <two:DateEntry ID="txtDisplayDate" runat="server" Text="$display_date" ShowCalendar="true" />
                    <one:TextContentControl ID="TextContentEditor" runat="server" />
			        <div class="save">
			            <span>Id: </span><asp:Label CssClass="articleId" ID="LabelId" runat=server></asp:Label>
				        <asp:Button ID="CancelButton" runat="server" CausesValidation="false" OnClick="CancelButton_Click" Text="$cancel" />
				        <asp:Button ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" />
				        <asp:Button ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" />
				        <asp:Label ID="AutoPublishWarning" runat="server" Text="$autopublish_warning"></asp:Label>
			        </div>
			        
                    <uc1:History runat="server" OnRevertToAudit="HistoryControl_RevertToAudit" id="HistoryControl" />			        
			        
			    </div>
			</div>			
		</two:TabularView>
		
		<two:TabularView ID="tabListRegulars" runat="server" TabName="$regular_list">
		    <div class="searchFull">
		        <two:Input ID="txtNewRegular" Required="false" runat="server" Text="$insert_new_regular" />                
                <asp:Button ID="cmdAddRegular" Text="$add_regular" runat="server" OnClick="cmdAddRegular_Click" />
		    </div>
			<div class="centerFull">
                <div class="biggv"> 

	                <asp:GridView OnSelectedIndexChanged="regularGridView_SelectedIndexChanged" DataSourceID="ObjectDataSourceRegularList" ID="regularGridView" runat="server" CssClass="gv" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="true" DataKeyNames="Id">
		                <Columns>
			                <asp:BoundField HeaderText="$id" DataField="Id" SortExpression="Id" ReadOnly="True" />
			                <asp:BoundField HeaderText="$title" DataField="Title" NullDisplayText="" SortExpression="title" ReadOnly="False" />

			                <asp:TemplateField HeaderText="$delete">
				                <ItemTemplate>
					                <asp:LinkButton ID="cmdDeleteRegular" runat="server" OnClick="cmdDeleteRegular_Click" Text="$delete" CommandArgument='<%# ((BORegular) Container.DataItem).Id %>' />
				                </ItemTemplate>
			                </asp:TemplateField>
                            <asp:CommandField SelectText="$edit" ShowSelectButton="true" />
		                </Columns>
	                </asp:GridView>
                    
                    
					<asp:ObjectDataSource ID="ObjectDataSourceRegularList" runat="server" OnSelecting="ObjectDataSourceRegularList_Selecting"
					    SelectMethod="ListRegulars" SortParameterName="sortBy" 
					    TypeName="OneMainWeb.ArticleDataSource">
				    </asp:ObjectDataSource>
                    
                
                </div>
            </div>
        </two:TabularView>
        
		<two:TabularView ID="tabViewEditRegular" runat="server" TabName="$view_edit_regular">

			<div class="centerFull">
			    <div class="contentEntry">
			        <one:TextContentControl ID="TxtRegularContent" runat="server" TitleLabel="$regular_title" SubTitleLabel="$regular_sub_title" TeaserLabel="$regular_teaser" HtmlLabel="$regular_html" HtmlRows="10" />
			        <div class="save">
				        <asp:Button ID="RegularCancelButton" runat="server" CausesValidation="false" CommandName="Cancel" Text="$cancel" OnClick="RegularCancelButton_Click" />
				        <asp:Button ID="RegularInsertUpdateButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateButton_Click" />
				        <asp:Button ID="RegularInsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateCloseButton_Click" />
			        </div>
			    </div>
			</div>

        </two:TabularView>  		
    </two:TabularMultiView>
</asp:Content>
