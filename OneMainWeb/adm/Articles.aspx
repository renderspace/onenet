<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles.aspx.cs" Inherits="OneMainWeb.Articles" Title="$articles" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register src="~/AdminControls/LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" TagPrefix="uc2" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />


    <asp:MultiView ID="Multiview1" runat="server" ActiveViewIndex="0" OnActiveViewChanged="Multiview1_ActiveViewChanged">
        <asp:View ID="View1" runat="server">

            <div class="adminSection">
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
			
				<asp:GridView ID="articleGridView" runat="server" CssClass="table table-hover"
					AllowSorting="True"
					AutoGenerateColumns="False"
					DataKeyNames="Id"
					OnSelectedIndexChanged="articleGridView_SelectedIndexChanged">
					<Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                            </HeaderTemplate>								    
							<ItemTemplate>
							    <asp:Literal ID="litId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							    <asp:CheckBox ID="chkFor" runat="server" Text="" />
							</ItemTemplate>
						</asp:TemplateField>
                        <asp:TemplateField HeaderText="Id" SortExpression="a.id">
							<ItemTemplate>
								<%# Eval("Id") %>
							</ItemTemplate>
						</asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
							<ItemTemplate>
								    <img src='<%# RenderStatusIcons(Eval("MarkedForDeletion"), Eval("IsChanged")) %>' alt="" />
								</div>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Title" SortExpression="cds.title">
							<ItemTemplate><%# Eval("Title") %></ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Display date" SortExpression="a.display_date">
							<ItemTemplate><%# ((DateTime)Eval("DisplayDate")).ToShortDateString()%></ItemTemplate>
						</asp:TemplateField>
							    
						<asp:TemplateField HeaderText="Categories">
							<ItemTemplate><%# Eval("RegularsList")%></ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField>
							<ItemTemplate>
                                <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="LinkButton1" runat="server" CssClass="btn btn-info btn-xs  " />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
                <div class="text-center">
                    <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                </div>	       
                <div class="form-group">
                    <div class="col-sm-12">
                        <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
                        <asp:LinkButton CssClass="btn btn-warning" ID="ButtonPublish" OnClick="ButtonPublish_Click" runat="server" CausesValidation="false" Text="Publish selected" />
                        <asp:LinkButton CssClass="btn btn-info" ID="ButtonRevert" OnClick="ButtonRevert_Click" runat="server" CausesValidation="false" Text="Revert selected to published state" />
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

            <div class="adminSection form-horizontal">
                <div class="form-group">
                    <label class="col-sm-3 control-label">Article category</label>
                    <div class="col-sm-9">
                        <div class="col-sm-4">
                            <asp:ListBox ID="lbRegulars" runat="server" Rows="5" CssClass="form-control" />
                        </div>
                        <div class="col-sm-2">
                            <asp:Button ValidationGroup="ATR" ID="cmdAssignRegularToArticle" runat="server" Text="$assign" OnClick="cmdAssignRegularToArticle_Click" /><br />
							<asp:Button ValidationGroup="ATR" ID="cmdRemoveRegularFromArticle" runat="Server" Text="$remove"
								OnClick="cmdRemoveRegularFromArticle_Click" />
                        </div>
                        <div class="col-sm-6">
								<asp:ListBox ID="lbRegularsAssignedToArticle" runat="server" Rows="5" CssClass="form-control" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label AssociatedControlID="TextBoxDate" runat="server" ID="LabelDate" Text="$display_date" Cssclass="col-sm-3 control-label"></asp:Label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ClientIDMode="Static" ID="TextBoxDate" CssClass="form-control" />
                    </div>
                </div>

                <one:TextContentControl ID="TextContentEditor" runat="server" />
			    <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
			            <span>Id: </span><asp:Label CssClass="articleId" ID="LabelId" runat=server></asp:Label>
				        <asp:LinkButton ID="CancelButton" runat="server" CausesValidation="false" OnClick="CancelButton_Click" Text="$cancel" CssClass="btn btn-primary" />
				        <asp:LinkButton ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click"  CssClass="btn btn-success" />
				        <asp:LinkButton ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" CssClass="btn btn-success" />
				        <asp:Label ID="AutoPublishWarning" runat="server" Text="$autopublish_warning"></asp:Label>
                    </div>
			    </div>
            </div>         
        </asp:View>
    </asp:MultiView>
</asp:Content>
