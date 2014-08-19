<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Dictionary.aspx.cs" Inherits="OneMainWeb.Dictionary" Title="One.NET Dictionary" ValidateRequest="false" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register TagPrefix="uc2" TagName="LastChangeAndHistory" src="~/AdminControls/LastChangeAndHistory.ascx"   %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<one:Notifier runat="server" ID="Notifier1" />

        <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged">
            <asp:View ID="View1" runat="server">
                <div class="adminSection">
			        <div class="col-md-2">
                        <asp:LinkButton ID="ButtonInsert" runat="server" onclick="cmdAddDictionaryEntry_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add" CssClass="btn btn-success" />
			        </div>
			        <div class="col-md-6">
                        <asp:TextBox ID="TextBoxSearch" runat="server" placeholder="Search by keyword or text" ValidationGroup="search"></asp:TextBox>
                        <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Search" OnClick="cmdSearch_Click" CssClass="btn btn-info" ValidationGroup="search" />
			        </div>
			        <div class="col-md-4">
                        <asp:LinkButton ID="LinkButtonExport" runat="server" OnClick="LinkButtonExport_Click" CssClass="btn btn-info" Text="Import / export keywords" />
			         </div>
                </div>
                <asp:Label ID="LabelNoResults" runat="server" Visible="false" EnableViewState="false"></asp:Label>
                <asp:GridView ID="GridViewEntries" runat="server" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="true" OnSorting="GridViewEntries_Sorting" OnRowDeleting="GridViewEntries_RowDeleting"
                        EnableViewState="true" OnRowDataBound="GridViewEntries_RowDataBound"
						CssClass="table table-hover"
						DataKeyNames="KeyWord"
						OnSelectedIndexChanged="GridViewEntries_SelectedIndexChanged">
						<Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                                </HeaderTemplate>								    
							    <ItemTemplate>
							        <asp:Literal ID="litId" Visible="false" runat="server" Text='<%# Eval("KeyWord") %>' />
							        <asp:CheckBox ID="chkFor" runat="server" Text="" />
							    </ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Keyword" SortExpression="keyword">
								<ItemTemplate>
								    <asp:Label ID="LabelKeyWord" runat="server" Text='<%# Eval("KeyWord") %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Translation">
                                <ItemTemplate><%# Eval("Title") %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
								<ItemTemplate>
                                    <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("KeyWord") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs  " />
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
                        </div>
                </div>
                
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="adminSection form-horizontal validationGroup">
                    <div class="form-group">
                        <label class="col-sm-3 control-label">Keyword</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelKeyword"></asp:Label>
                            <asp:TextBox ID="txtKeyword" runat="server" CssClass="form-control required" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-3 control-label">Language</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelLanguage"></asp:Label>
                        </div>
                    </div>

		            <one:TextContentControl ID="txtTextContent" runat="server" HtmlRows="10" />

		            <div class="form-group">
                        <div class="col-sm-3">
						        <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
                            </div>
                        <div class="col-sm-9">
			                <asp:LinkButton ID="CancelButton" runat="server" CausesValidation="false" OnClick="CancelButton_Click" Text="Cancel" CssClass="btn btn-default" />
			                <asp:LinkButton ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" CssClass="btn btn-success causesValidation" />
			                <asp:LinkButton ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" CssClass="btn btn-success causesValidation" />
                        </div>
		            </div>
		        </div>	
            </asp:View>
            <asp:View ID="View3" runat="server">
                <div class="adminSection">
	                <asp:Button ID="CmdExport" runat="server" Text="$export_dictionary" OnClick="CmdExport_Click" />
                </div>
	            <div class="adminSection">
                    <asp:FileUpload ID="FileUploadImport" runat="server" />	        
	                <asp:Button ID="CmdImport" runat="server" Text="$import_dictionary" OnClick="CmdImport_Click" />
	                <div class="radio">
	                    <asp:RadioButtonList ID="RadioButtonListWriteTypes" runat="server" />
	                </div>
                </div>  
            </asp:View>
            <asp:View ID="View4" runat="server"></asp:View>
        </asp:MultiView>
</asp:Content>
