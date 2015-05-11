<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Articles.aspx.cs" Inherits="OneMainWeb.Articles" Title="One.NET Articles" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register src="~/AdminControls/LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" TagPrefix="uc2" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<one:Notifier runat="server" ID="Notifier1" />


    <asp:MultiView ID="Multiview1" runat="server" OnActiveViewChanged="Multiview1_ActiveViewChanged">
        <asp:View ID="View1" runat="server">

            <div class="adminSection">
			    <div class="col-md-2">
			        <asp:LinkButton ID="cmdAddArticle" runat="server" text="<span class='glyphicon glyphicon-plus'></span> Add" OnClick="cmdAddArticle_Click" CssClass="btn btn-success" />			
			    </div>
			    <div class="col-md-6 validationGroup">
                    <asp:TextBox runat="server" ID="TextBoxShowById" placeholder="Search by ID or title" CssClass="required"></asp:TextBox>
                    <asp:LinkButton runat="server"  OnClick="cmdShowById_Click" ID="LinkButtonShowById" CssClass="btn btn-info causesValidation" Text="Search"></asp:LinkButton>
			    </div>
			    <div class="col-md-4 validationGroup">
                    <asp:DropDownList OnDataBound="DropDownListRegularFilter_DataBound" DataTextField="Title" DataValueField="Id" AppendDataBoundItems="False" ID="DropDownListRegularFilter" runat="server"   />                  
    	            <asp:LinkButton ID="cmdFilterArticles" runat="server" Text="Filter" OnClick="cmdFilterArticles_Click" CssClass="btn btn-info causesValidation" />                                    
			     </div>
            </div>
			
				<asp:GridView ID="GridViewArticles" runat="server" CssClass="table table-hover" OnSorting="GridViewArticles_Sorting"
					AllowSorting="True"
					AutoGenerateColumns="False"
					DataKeyNames="Id"
					OnSelectedIndexChanged="GridViewArticles_SelectedIndexChanged">
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
							<%# RenderStatusIcons(Eval("MarkedForDeletion"), Eval("IsChanged")) %>
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
                <asp:Panel runat="server" ID="PanelGridButtons" CssClass="form-group">
                    <div class="col-sm-12">
                        <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
                        <asp:LinkButton CssClass="btn btn-warning" ID="ButtonPublish" OnClick="ButtonPublish_Click" runat="server" CausesValidation="false" Text="Publish selected" />
                        <asp:LinkButton CssClass="btn btn-info" ID="ButtonRevert" OnClick="ButtonRevert_Click" runat="server" CausesValidation="false" Text="Revert selected to published state" />
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="PanelNoResults"  CssClass="col-md-12">
                     <div class="alert alert-info" role="alert">
                        No articles to show.
                         </div>
                </asp:Panel>
			 
        </asp:View>
        <asp:View ID="View2" runat="server">

            <script type="text/javascript" charset="utf-8">
                $(document).ready(function () {
                    $("#datetimepicker-article").datetimepicker({ format: 'MM/DD/YYYY H:mm' });
                });
            </script>

            <div class="adminSection form-horizontal validationGroup">
                <div class="form-group">
                    <div class="col-sm-9 col-sm-offset-3 jumbotron jumbo-less-padding">
                        <div class="col-sm-5">
                            <asp:Label runat="server" AssociatedControlID="ListBoxRegulars">Choose from possible categories:</asp:Label>
                            <asp:ListBox ID="ListBoxRegulars" runat="server" Rows="5" CssClass="form-control" />
                        </div>
                        <div class="col-sm-1">
                            <asp:LinkButton ValidationGroup="ATR" ID="cmdAssignRegularToArticle" runat="server"  OnClick="cmdAssignRegularToArticle_Click" CssClass="btn btn-info"> 
                                <span class="glyphicon glyphicon-arrow-right"></span></asp:LinkButton>
                                <br />
							<asp:LinkButton ValidationGroup="ATR" ID="cmdRemoveRegularFromArticle" runat="Server" CssClass="btn btn-info" OnClick="cmdRemoveRegularFromArticle_Click" >
                                <span class="glyphicon glyphicon-arrow-left"></span></asp:LinkButton>
                        </div>
                        <div class="col-sm-6">
                             <asp:Label runat="server" AssociatedControlID="ListBoxAssignedToArticle">Categories assigned to current article:</asp:Label>
								<asp:ListBox ID="ListBoxAssignedToArticle" runat="server" Rows="5" CssClass="form-control" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label AssociatedControlID="TextBoxDate" runat="server" ID="LabelDate" Text="Display date" Cssclass="col-sm-3 control-label"></asp:Label>
                    <div class="col-sm-9">
                        <div class='input-group datetime' id='datetimepicker-article'>
                                <asp:TextBox runat="server" ClientIDMode="Static" ID="TextBoxDate" CssClass="form-control" />
                                <span class="input-group-addon"><span class="glyphicon glyphicon-hourglass"></span></span>
                        </div>
                    </div>
                </div>

                <one:TextContentControl ID="TextContentEditor" runat="server" />
			    <div class="form-group">
                    <div class="col-sm-3">
						<uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
                    </div>
                    <div class="col-sm-9">
			            <span>Id: </span><asp:Label CssClass="articleId" ID="LabelId" runat=server></asp:Label>
				        <asp:LinkButton ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click"  CssClass="btn btn-success causesValidation" />
				        <asp:LinkButton ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" CssClass="btn btn-success causesValidation" />
                    </div>
			    </div>
            </div>         
        </asp:View>
    </asp:MultiView>
</asp:Content>
