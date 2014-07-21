<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Dictionary.aspx.cs" Inherits="OneMainWeb.Dictionary" Title="$dictionary" ValidateRequest="false" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<one:Notifier runat="server" ID="Notifier1" />
<div class="topStructure">
    <asp:checkbox ID="CheckboxShowUntranslated" OnCheckedChanged="CheckboxShowUntranslated_CheckedChanged" AutoPostBack="true" Runat="server" Text="$show_untranslated" />		
</div>
    <div class="topCommands">
        <asp:LinkButton ID="LinkButtonKeywords" runat="server" OnClick="LinkButtonKeywords_Click">Keywords</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonExport" runat="server" OnClick="LinkButtonExport_Click">Import/Export</asp:LinkButton>
    </div>

        <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged">
            <asp:View ID="View1" runat="server">
                <div class="searchFull">
	                <asp:Button ID="cmdAddDictionaryEntry" runat="server" Text="$add_dictionary_entry" OnClick="cmdAddDictionaryEntry_Click" />
                    <two:InputWithButton ID="InputWithButtonSearch" ValidationGroup="search" runat="server" Text="$search_keyword" ButtonText="$search" OnClick="cmdSearch_Click" IsLinkButton="false"  />
	            </div>
                <div class="centerFull">
                        <div class="biggv">  
                            <asp:Label ID="LabelNoResults" runat="server" Visible="false" EnableViewState="false"></asp:Label>
                            <asp:GridView ID="GridViewEntries" runat="server" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="true" OnSorting="GridViewEntries_Sorting" OnRowDeleting="GridViewEntries_RowDeleting"
                                    EnableViewState="true" OnRowDataBound="GridViewEntries_RowDataBound"
						            CssClass="gv"
						            DataKeyNames="KeyWord"
						            OnSelectedIndexChanged="GridViewEntries_SelectedIndexChanged">
						            <Columns>
							            <asp:TemplateField HeaderText="$keyword" SortExpression="keyword">
								            <ItemTemplate>
								                <asp:Label ID="LabelKeyWord" runat="server" Text='<%# Eval("KeyWord") %>' />
								            </ItemTemplate>
							            </asp:TemplateField>
							            <asp:TemplateField>
								            <ItemTemplate>
								                    <asp:LinkButton ID="cmdDelete" CommandArgument='<%# Eval("KeyWord") %>' CommandName="Delete" runat="server" Text="$delete"  />
								            </ItemTemplate>
							            </asp:TemplateField>
							            <asp:TemplateField HeaderText="$meaning">
                                           <ItemTemplate><%# Eval("Title") %></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
								            <ItemTemplate>
								                <div style="width: 50px;">
								                    <asp:ImageButton ID="cmdEditButton" runat="server" CommandName="Select"	CommandArgument='<%# Eval("KeyWord") %>'  />
										            <asp:LinkButton Text="$edit" CommandName="Select" CommandArgument='<%# Eval("KeyWord") %>' ID="cmdEdit" runat="server" />
										        </div>
								            </ItemTemplate>
							            </asp:TemplateField>
							        </Columns>
                            </asp:GridView>
                            <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                        </div>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="centerFull">
		        <div class="contentEntry">
                    Keyword: <asp:Label runat="server" ID="LabelKeyword"></asp:Label>

		            <two:Input ID="txtKeyword" runat="server" Text="$keyword" />
		            <one:TextContentControl ID="txtTextContent" runat="server" 
		            TitleLabel="$dict_entry_title" SubTitleLabel="$dict_entry_sub_title" 
		            TeaserLabel="$dict_entry_teaser" HtmlLabel="$dict_entry_html" HtmlRows="10" />
		            <div class="save">
			            <asp:Button ID="CancelButton" runat="server" CausesValidation="false" OnClick="CancelButton_Click" Text="$cancel" />
			            <asp:Button ID="InsertUpdateButton" runat="server" CausesValidation="True" OnClick="InsertUpdateButton_Click" />
			            <asp:Button ID="InsertUpdateCloseButton" runat="server" CausesValidation="True" OnClick="InsertUpdateCloseButton_Click" />
		            </div>
		        </div>
		    </div>	
            </asp:View>
            <asp:View ID="View3" runat="server">
                <div class="searchFull">
	                <asp:Button ID="CmdExport" runat="server" Text="$export_dictionary" OnClick="CmdExport_Click" />
                </div>
	            <div class="searchFull">
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
