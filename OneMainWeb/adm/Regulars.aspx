<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Regulars.aspx.cs" Inherits="OneMainWeb.adm.Regulars" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <one:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="Multiview1" OnActiveViewChanged="Multiview1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            <div class="searchFull">
		         <asp:TextBox runat="server" ID="TextBoxRegular" MaxLength="255" placeholder="New article category"></asp:TextBox>
            
                <asp:LinkButton ID="cmdAddRegular" text="<span class='glyphicon glyphicon-plus'></span> Add" runat="server" OnClick="cmdAddRegular_Click"  CssClass="btn btn-success" />		
		    </div>
		    <div class="centerFull">
	            <asp:GridView OnSelectedIndexChanged="regularGridView_SelectedIndexChanged" ID="regularGridView" runat="server" CssClass="gv" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="true" DataKeyNames="Id">
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
			            <asp:BoundField HeaderText="$id" DataField="Id" SortExpression="Id" ReadOnly="True" />
			            <asp:BoundField HeaderText="$title" DataField="Title" NullDisplayText="" SortExpression="title" ReadOnly="False" />
                        <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton  CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="LinkButton1" runat="server" CssClass="btn btn-info btn-xs" CausesValidation="false" Text="<span class='glyphicon glyphicon-pencil'></span> Edit" />
                                </ItemTemplate>
                        </asp:TemplateField>
		            </Columns>
	            </asp:GridView>
                <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
            </div>
        </asp:View>
        <asp:View ID="View3" runat="server">
            <div class="centerFull">
			    <div class="contentEntry">
			        <one:TextContentControl ID="TxtRegularContent" runat="server" TitleLabel="$regular_title" SubTitleLabel="$regular_sub_title" TeaserLabel="$regular_teaser" HtmlLabel="$regular_html" HtmlRows="10" />
			        <div class="save">
				        <asp:Button ID="RegularCancelButton" runat="server" CausesValidation="false" CommandName="Cancel" Text="$cancel" OnClick="RegularCancelButton_Click" />
				        <asp:Button ID="RegularInsertUpdateButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateButton_Click" Text="Save" />
				        <asp:Button ID="RegularInsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="RegularInsertUpdateCloseButton_Click"  Text="Save & close"/>
			        </div>
			    </div>
			</div>
        </asp:View>
    </asp:MultiView>
        
        
</asp:Content>
