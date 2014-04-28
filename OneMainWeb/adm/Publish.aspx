<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="OneMainWeb.Publish" Title="$publishing" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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

	<two:TabularMultiView ID="tabMultiview" runat="server" OnViewIndexChanged="tabMultiview_OnViewIndexChanged">
		<two:TabularView ID="tabListArticles" runat="server" TabName="$article_list">

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
							    <asp:TemplateField HeaderText="$schedule_date">
							        <ItemTemplate>
							            <%# ResourceManager.GetString("$check_temporal_publish_tab_for_review") %>
							        </ItemTemplate>
							        <EditItemTemplate>
							            <two:DateEntry ValidationGroup="sd" ShowCalendar="true" ID="txtScheduledAt" runat="server" />
					                    <asp:Literal ID="litArticleId1" runat="server" Visible="false" Text='<%# Eval("Id") %>' />		            
							        </EditItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
							        <ItemTemplate>
							            <asp:LinkButton  CommandArgument='<%# Container.DataItemIndex %>' ID="cmdSchedule" runat="server" Text="$schedule" CommandName="Schedule" />
							        </ItemTemplate>
							        <EditItemTemplate>
                                        <asp:LinkButton ValidationGroup="sd" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdUpdate" runat="server" Text="$update" CommandName="Save" />							            
                                        <asp:LinkButton CausesValidation="false" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdCancel" runat="server" Text="$cancel" CommandName="Cancel" />		
							        </EditItemTemplate>
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

        </two:TabularView>
        
		<two:TabularView ID="tabListPages" runat="server" TabName="$page_list">

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
							    <asp:TemplateField HeaderText="$schedule_date">
							        <ItemTemplate>
							            <%# ResourceManager.GetString("$check_temporal_publish_tab_for_review") %>
							        </ItemTemplate>
							        <EditItemTemplate>
							            <two:DateEntry ValidationGroup="sd" ShowCalendar="true" ID="txtScheduledAt" runat="server" />
					                    <asp:Literal ID="litPageId1" runat="server" Visible="false" Text='<%# Eval("Id") %>' />		            
							        </EditItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
							        <ItemTemplate>
							            <asp:LinkButton  CommandArgument='<%# Container.DataItemIndex %>' ID="cmdSchedule" runat="server" Text="$schedule" CommandName="Schedule" />
							        </ItemTemplate>
							        <EditItemTemplate>
                                        <asp:LinkButton ValidationGroup="sd" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdUpdate" runat="server" Text="$update" CommandName="Save" />							            
                                        <asp:LinkButton CausesValidation="false" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdCancel" runat="server" Text="$cancel" CommandName="Cancel" />		
							        </EditItemTemplate>
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
	    </two:TabularView>
        
		<two:TabularView ID="tabCommentList" runat="server" TabName="$comment_list">

			<div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="commentGridView" runat="server" PageSize="25" PageIndex="0"
						    PagerSettings-Mode="NumericFirstLast"
						    PagerSettings-LastPageText="$last"
						    PagerSettings-FirstPageText="$first"
						    PagerSettings-PageButtonCount="7"
						    AllowSorting="True"
						    AllowPaging="True"
						    AutoGenerateColumns="False"
						    DataSourceID="ObjectDataSourceCommentList"
						    CssClass="gv"
						    DataKeyNames="Id"
						    OnRowCommand="commentGridView_RowCommand">
						    <Columns>
							    <asp:TemplateField HeaderText="$comment" SortExpression="title">
								    <ItemTemplate><%# Eval("Comment") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$commented_at" SortExpression="CommentedAt">
								    <ItemTemplate><%# ((DateTime)Eval("CommentedAt")).ToShortDateString()%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$Name">
								    <ItemTemplate><%# Eval("Name")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$schedule_date">
							        <ItemTemplate>
							            <%# ResourceManager.GetString("$check_temporal_publish_tab_for_review") %>
							        </ItemTemplate>
							        <EditItemTemplate>
							            <two:DateEntry ValidationGroup="sd" ShowCalendar="true" ID="txtScheduledAt" runat="server" />
					                    <asp:Literal ID="litCommentId1" runat="server" Visible="false" Text='<%# Eval("Id") %>' />		            
							        </EditItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
							        <ItemTemplate>
							            <asp:LinkButton  CommandArgument='<%# Container.DataItemIndex %>' ID="cmdSchedule" runat="server" Text="$schedule" CommandName="Schedule" />
							        </ItemTemplate>
							        <EditItemTemplate>
                                        <asp:LinkButton ValidationGroup="sd" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdUpdate" runat="server" Text="$update" CommandName="Save" />							            
                                        <asp:LinkButton CausesValidation="false" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdCancel" runat="server" Text="$cancel" CommandName="Cancel" />		
							        </EditItemTemplate>
							    </asp:TemplateField>
							    
							    <asp:TemplateField>
                                    <HeaderTemplate>
                                        <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                    </HeaderTemplate>								    
							        <ItemTemplate>
							            <asp:Literal ID="litCommentId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							            <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							        </ItemTemplate>
							    </asp:TemplateField>
						    </Columns>
						    <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />
					    </asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourceCommentList" runat="server" SelectMethod="SelectComments"
						    TypeName="OneMainWeb.UnpublishedCommentsDataSource" OnSelecting="ObjectDataSourceCommentList_Selecting" SelectCountMethod="SelectCommentCount" SortParameterName="sortBy">
				       </asp:ObjectDataSource>
			    </div>
    		    <asp:Button CssClass="right" ID="Button1" OnClick="PublishCommentsButton_Click" runat="server" CausesValidation="false" Text="$publish_selected" />
		    </div>

        </two:TabularView>   

		<two:TabularView ID="tabEventList" runat="server" TabName="$event_list">

			<div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="eventGridView" runat="server" PageSize="25" PageIndex="0"
						    PagerSettings-Mode="NumericFirstLast"
						    PagerSettings-LastPageText="$last"
						    PagerSettings-FirstPageText="$first"
						    PagerSettings-PageButtonCount="7"
						    AllowSorting="True"
						    AllowPaging="True"
						    AutoGenerateColumns="False"
						    DataSourceID="ObjectDataSourceEventList"
						    CssClass="gv"
						    DataKeyNames="Id"
						    OnRowCommand="eventGridView_RowCommand">
						    <Columns>
							    <asp:TemplateField HeaderText="$event" SortExpression="title">
								    <ItemTemplate><%# Eval("Title") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$from" SortExpression="title">
								    <ItemTemplate><%# Eval("BeginDate") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$to" SortExpression="title">
								    <ItemTemplate><%# Eval("EndDate") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$LastChanged">
								    <ItemTemplate><%# Eval("LastChanged")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="$LastChangedBy">
								    <ItemTemplate><%# Eval("LastChangedBy")%></ItemTemplate>
							    </asp:TemplateField>
							    
							    
							    <asp:TemplateField HeaderText="$schedule_date">
							        <ItemTemplate>
							            <%# ResourceManager.GetString("$check_temporal_publish_tab_for_review") %>
							        </ItemTemplate>
							        <EditItemTemplate>
							            <two:DateEntry ValidationGroup="sd" ShowCalendar="true" ID="txtScheduledAt" runat="server" />
					                    <asp:Literal ID="litEventId1" runat="server" Visible="false" Text='<%# Eval("Id") %>' />		            
							        </EditItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField>
							        <ItemTemplate>
							            <asp:LinkButton  CommandArgument='<%# Container.DataItemIndex %>' ID="cmdSchedule" runat="server" Text="$schedule" CommandName="Schedule" />
							        </ItemTemplate>
							        <EditItemTemplate>
                                        <asp:LinkButton ValidationGroup="sd" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdUpdate" runat="server" Text="$update" CommandName="Save" />							            
                                        <asp:LinkButton CausesValidation="false" CommandArgument='<%# Container.DataItemIndex %>' ID="cmdCancel" runat="server" Text="$cancel" CommandName="Cancel" />		
							        </EditItemTemplate>
							    </asp:TemplateField>
							    
							    
							    
							    <asp:TemplateField>
                                    <HeaderTemplate>
                                        <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                    </HeaderTemplate>								    
							        <ItemTemplate>
							            <asp:Literal ID="litEventId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							            <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							        </ItemTemplate>
							    </asp:TemplateField>
						    </Columns>
						    <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />
					    </asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourceEventList" runat="server" SelectMethod="SelectEvents"
						    TypeName="OneMainWeb.UnpublishedEventDataSource" OnSelecting="ObjectDataSourceEventList_Selecting" SelectCountMethod="SelectEventCount" SortParameterName="sortBy">
				       </asp:ObjectDataSource>
			    </div>
    		    <asp:Button CssClass="right" ID="PublishEventsButton" OnClick="PublishEventsButton_Click" runat="server" CausesValidation="false" Text="$publish_selected" />
		    </div>

        </two:TabularView>
        
		<two:TabularView ID="tabPublisher" runat="server" TabName="$timed_publisher">

			<div class="searchFull">
			    <two:LabeledCheckBox ID="chkPublished" runat="server" Text="$show_published" />
			    <asp:Button ID="cmdDisplay" OnClick="cmdDisplay_Click" runat="server" Text="$display" />
			</div>

			<div class="centerFull">
                <div class="biggv">     
					    <asp:GridView ID="publisherGridview" runat="server" PageSize="25" PageIndex="0"
						    PagerSettings-Mode="NumericFirstLast"
						    PagerSettings-LastPageText="$last"
						    PagerSettings-FirstPageText="$first"
						    PagerSettings-PageButtonCount="7"
						    AllowSorting="True"
						    AllowPaging="True"
						    AutoGenerateColumns="False"
						    DataSourceID="ObjectDataSourcePublisherList"
						    CssClass="gv"
						    DataKeyNames="Id">
						    <Columns>
						        <asp:BoundField ItemStyle-HorizontalAlign="center" DataField="SubSystem" HeaderText="$subsystem" SortExpression="subsystem" />
						        <asp:BoundField ItemStyle-HorizontalAlign="center" DataField="FkId" HeaderText="$fkid" SortExpression="fkid" />
                                <asp:BoundField DataField="ScheduledAt" HeaderText="$scheduled_at" SortExpression="scheduled_at" />
                                <asp:BoundField DataField="PublishedAt" HeaderText="$published_at" SortExpression="published_at" />						   
				                <asp:TemplateField>
				                    <ItemTemplate>
				                        <asp:LinkButton runat="server" ID="cmdDelete" CommandName="Delete" Text="$delete" />
				                    </ItemTemplate>
				                </asp:TemplateField>		        
						        
						    </Columns>
 <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
							    PageButtonCount="7" />						    
						</asp:GridView>
				       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
						    EnablePaging="True" ID="ObjectDataSourcePublisherList" runat="server" SelectMethod="SelectPublisherData"
						    TypeName="OneMainWeb.PublisherDataSource" OnSelecting="ObjectDataSourcePublisherList_Selecting" SelectCountMethod="SelectPublisherDataCount" SortParameterName="sortBy" DeleteMethod="Delete">
			            <DeleteParameters>
			                <asp:Parameter Name="Id" Type="Int32" ConvertEmptyStringToNull="false" />
			            </DeleteParameters>			    
				       </asp:ObjectDataSource>
						
			    </div>
			 </div>
			 
	    </two:TabularView>
	    
    </two:TabularMultiView>

</asp:Content>
