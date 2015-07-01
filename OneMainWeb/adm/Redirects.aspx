<%@ Page Title="One.NET Redirects" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Redirects.aspx.cs" Inherits="OneMainWeb.Redirects" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged">
        <asp:View ID="View1" runat="server">


            <div class="adminSection">
			    <div class="col-md-2">
                    <asp:LinkButton ID="cmdShowAddRedirect" runat="server" Text="<span class='glyphicon glyphicon-plus'></span> Add" CssClass="btn btn-success" OnClick="cmdShowAddRedirect_Click" />
			    </div>
			    <div class="col-md-4 validationGroup">
                    <asp:TextBox ID="TextBoxSearch" runat="server" placeholder="Edit by id" CssClass="required int"></asp:TextBox>
                    <asp:LinkButton ID="ButtonEditById" runat="server" Text="Search" OnClick="ButtonEditById_Click" CssClass="btn btn-info causesValidation"  />
			    </div>
			    <div class="col-md-4">
                        <asp:LinkButton ID="LinkButtonExport" runat="server" Text="Export" CssClass="btn btn-default" OnClick="LinkButtonExport_Click" />
			        </div>
                <div class="col-md-2">
                    <label>
                            <asp:CheckBox runat="server" ID="CheckBoxShowAll" AutoPostBack="true" OnCheckedChanged="CheckBoxShowAll_CheckedChanged" /> Show all
                    </label>
                </div>
            </div>

            <asp:GridView ID="GridViewRedirects" runat="server"
                AllowSorting="True" 
                AutoGenerateColumns="False"
                DataKeyNames="Id"
                CssClass="table table-hover table-clickable-row"
                OnSorting="GridViewRedirects_Sorting"
                OnSelectedIndexChanged="GridViewRedirects_SelectedIndexChanged">
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
                    <asp:TemplateField HeaderText="From relative path" SortExpression="from_link">
	                    <ItemTemplate><%# Eval("FromLink") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="To relative path" SortExpression="to_link">
	                    <ItemTemplate><%# Eval("ToLink") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
	                    <ItemTemplate>
		                    <asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# Eval("Id") %>' Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CssClass="btn btn-info btn-xs  "  />
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
                </div>
            </asp:Panel>

            <asp:Panel runat="server" ID="PanelNoResults"  CssClass="col-md-12">
                <div class="alert alert-info" role="alert">
                    No redirects to show.
                </div>
            </asp:Panel>
     
        </asp:View>
        <asp:View ID="View2" runat="server">
             <div class="adminSection form-horizontal validationGroup">
                <div class="form-group">
                    <asp:Label runat="server" Text="From relative link" CssClass="col-sm-3 control-label" AssociatedControlID="InputFromLink" />
			        <div class="col-sm-9">
                        <asp:TextBox CssClass="form-control required" id="InputFromLink" runat="server" Text="" ValidationGroup="REDIRECTS" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="InputFromLink" CssClass="text-danger" ValidationGroup="REDIRECTS" ErrorMessage="From link is required." />
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" Text="To relative link" CssClass="col-sm-3 control-label" AssociatedControlID="InputToLink" />
                    <div class="col-sm-9">
			            <asp:TextBox CssClass="form-control required" id="InputToLink" runat="server" Text="" ValidationGroup="REDIRECTS" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="InputToLink" CssClass="text-danger" ValidationGroup="REDIRECTS" ErrorMessage="To link is required." />
                    </div>
                </div>
			    <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
				        <asp:LinkButton CssClass="btn btn-success causesValidation" ID="CmdSave" CommandName="SAVE" runat="server" Text="Save" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
				        <asp:LinkButton CssClass="btn btn-success causesValidation" ID="CmdSaveClose" CommandName="SAVE_CLOSE" runat="server" Text="Save and close" onclick="CmdSave_Click" ValidationGroup="REDIRECTS" />
                    </div>
			    </div>
			</div>
        </asp:View>
    </asp:MultiView>
</asp:Content>
