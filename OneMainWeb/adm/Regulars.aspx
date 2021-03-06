﻿<%@ Page Title="One.NET Regulars" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Regulars.aspx.cs" Inherits="OneMainWeb.adm.Regulars" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="Multiview1" OnActiveViewChanged="Multiview1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            <div class="adminSection validationGroup">
		        <asp:TextBox runat="server" ID="TextBoxRegular" MaxLength="255" placeholder="New article category" CssClass="required"></asp:TextBox>
                <asp:LinkButton ID="cmdAddRegular" text="<span class='glyphicon glyphicon-plus'></span> Add" runat="server" OnClick="cmdAddRegular_Click"  CssClass="btn btn-success causesValidation" />		
		    </div>
	        <asp:GridView OnSelectedIndexChanged="GridViewRegular_SelectedIndexChanged" ID="GridViewRegular" runat="server" CssClass="table table-hover table-clickable-row" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="true" DataKeyNames="Id" OnSorting="GridViewRegular_Sorting">
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
			        <asp:BoundField HeaderText="Id" DataField="Id" SortExpression="Id" ReadOnly="True" />
			        <asp:BoundField HeaderText="Category title" DataField="Title" NullDisplayText="" SortExpression="title" ReadOnly="False" />
			        <asp:BoundField HeaderText="Human readable url" DataField="HumanReadableUrl" NullDisplayText="" SortExpression="human_readable_url" ReadOnly="False" />
                    <asp:BoundField HeaderText="No. of articles" DataField="ArticleCount" NullDisplayText="" SortExpression="ArticleCount" ReadOnly="False" />
                    <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton  CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="LinkButton1" runat="server" CssClass="btn btn-info btn-xs" CausesValidation="false" Text="<span class='glyphicon glyphicon-pencil'></span> Edit" />
                            </ItemTemplate>
                    </asp:TemplateField>
		        </Columns>
	        </asp:GridView>
            <div class="form-group">
                <div class="col-sm-12">
                    <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
                </div>
            </div>
        </asp:View>
        <asp:View ID="View3" runat="server">
            <div class="adminSection form-horizontal validationGroup">
                <script>

                window.TextBoxHumanReadableUrlClientId = '<%=TextBoxHumanReadableUrl.ClientID %>';

                </script>
			    <one:TextContentControl TabIndex="1" ID="TxtRegularContent" runat="server" HtmlRows="10" />
                <div class="form-group">
                    <asp:Label AssociatedControlID="TextBoxHumanReadableUrl" runat="server" ID="LabelHumanReadableUrl" Text="Human readable url" Cssclass="col-sm-3 control-label"></asp:Label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" TabIndex="5" ClientIDMode="Static" ID="TextBoxHumanReadableUrl" CssClass="human-readable-url-input form-control" />
                    </div>
                </div>
			    <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
				        <asp:LinkButton TabIndex="6" ID="RegularInsertUpdateButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateButton_Click" Text="Save" CssClass="btn btn-success causesValidation" />
				        <asp:LinkButton TabIndex="7" ID="RegularInsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateCloseButton_Click"  Text="Save & close" CssClass="btn btn-success causesValidation" />
                    </div>
			    </div>
			</div>
        </asp:View>
    </asp:MultiView>
        
        
</asp:Content>
