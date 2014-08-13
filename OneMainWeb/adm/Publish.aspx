<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="OneMainWeb.Publish" Title="One.NET Publishing" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />
    <div class="adminSection">    
        <asp:LinkButton ID="LinkButtonArticles" runat="server" OnClick="LinkButtonArticles_Click">Articles</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonPages" runat="server" OnClick="LinkButtonPages_Click">Pages</asp:LinkButton>
    </div>

    <asp:MultiView runat="server" ID="Multiview1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            
					    <asp:GridView ID="GridViewArticles" runat="server" CssClass="table table-hover" OnSorting="GridViewArticles_Sorting"
						    AllowSorting="True"
						    AutoGenerateColumns="False"
						    DataKeyNames="Id"
						    OnRowCommand="GridViewArticles_RowCommand">
						    <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                    </HeaderTemplate>								    
							        <ItemTemplate>
							            <asp:Literal ID="litArticleId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							            <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							        </ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="Title" SortExpression="title">
								    <ItemTemplate><%# Eval("Title") %></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="Display date" SortExpression="display_date">
								    <ItemTemplate><%# ((DateTime)Eval("DisplayDate")).ToShortDateString()%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="Categories">
								    <ItemTemplate><%# Eval("RegularsList")%></ItemTemplate>
							    </asp:TemplateField>
							    <asp:TemplateField HeaderText="LastChanged">
								    <ItemTemplate><%# Eval("DisplayLastChanged")%></ItemTemplate>
							    </asp:TemplateField>
							    
						    </Columns>
					    </asp:GridView>
			    
                <div class="text-center">
                    <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                </div>
                <div class="form-group">
                    <div class="col-sm-12">
                        <asp:LinkButton CssClass="btn btn-warning" ID="PublishArticlesButton" OnClick="PublishArticlesButton_Click" runat="server" CausesValidation="false" Text="Publish selected" />
                    </div>
                </div>
        </asp:View>
        <asp:View ID="View2" runat="server">
				<asp:GridView ID="GridViewPages" runat="server"  CssClass="table table-hover" OnSorting="GridViewPages_Sorting"
					AllowSorting="True"
					AutoGenerateColumns="False"
					DataKeyNames="Id"
					OnRowCommand="GridViewPages_RowCommand">
					<Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                            </HeaderTemplate>							    
							<ItemTemplate>
							    <asp:Literal ID="litPageId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							    <asp:CheckBox ID="chkForPublish" runat="server" Text="" />
							</ItemTemplate>
						</asp:TemplateField>	
						<asp:TemplateField HeaderText="Id">
							<ItemTemplate><%# Eval("Id") %></ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Page title">
							<ItemTemplate><%# Eval("Title") %></ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="LastChanged">
							<ItemTemplate><%# Eval("DisplayLastChanged")%></ItemTemplate>
						</asp:TemplateField>
							    							    
											    
					</Columns>				    
				</asp:GridView>		
                <div class="text-center">
                    <two:PostbackPager id="TwoPostbackPager2" OnCommand="TwoPostbackPager2_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                </div>
                <div class="form-group">
                    <div class="col-sm-12">
                        <asp:LinkButton CssClass="btn btn-warning" ID="PublishPagesButton" OnClick="PublishPagesButton_Click" runat="server" CausesValidation="false" Text="Publish selected" />
                    </div>
                </div>			

        </asp:View>
    </asp:MultiView>

</asp:Content>
