<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Editor.aspx.cs" Inherits="OneMainWeb.adm.Editor" Title="Untitled Page" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="System.IO" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
	<div class="topStructure">
		<asp:checkbox id="chkEnableDelete" AutoPostBack="true" Runat="server" text="$enable_delete" />
	</div>
    <two:TabularMultiView ID="TabularMultiView1" runat="server" OnViewIndexChanged="TabularMultiView1_OnViewIndexChanged">
        <two:TabularView ID="TabularView1" runat="server" TabName="$file_list">
			<div class="searchFull" runat="server" id="uploadDiv">
	            <asp:FileUpload ID="fileUpload" runat="server" />
	                <asp:Button ID="cmdUpload" ValidationGroup="upload" OnClick="cmdUpload_Click" cssclass="leftButton" runat="server" Text="$upload" />
            </div>
            <div class="centerFull">
                <div class="biggv">
                    <asp:GridView ID="DataGridFiles" OnRowCommand="DataGridFiles_RowCommand" OnRowDataBound="DataGridFiles_RowDataBound" runat="server" PageSize="10" PageIndex="0" PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-LastPageText="$last" PagerSettings-FirstPageText="$first" PagerSettings-PageButtonCount="7"
                        AllowSorting="false" AllowPaging="false" AutoGenerateColumns="False" CssClass="gv">
                        <Columns>
                            <asp:TemplateField HeaderText="$file" SortExpression="title">
                                <ItemTemplate>
                                   <asp:Literal ID="imgLit" runat="server" />
                                   <asp:LinkButton runat="server" ID="cmdMoveInto" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="cmdDelete" runat="server" Text='$delete' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="cmdEdit" CommandName="EditFile" runat="server" Text='$edit' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </two:TabularView>
        <two:TabularView ID="TabularView2" runat="server" TabName="$edit_file">
			<div class="centerFull">
			    <div class="contentEntry">            
                    <two:InfoLabel runat="server" ID="lblFilePath" Text="$file" />
                    <two:Input ID="txtFileContent" ValidationGroup="sr"  runat="server" TextMode="MultiLine" Rows="20" Required="true" />
                    <div class="save" runat="server" id="DivSaveButtons">
                        <asp:Button ID="CancelButton" Text="$cancel" ValidationGroup="sr"  OnClick="CancelButton_Click" runat="server" CausesValidation="false"  />
                        <asp:Button ID="UpdateButton" Text="$update" ValidationGroup="sr"  runat="server" CausesValidation="True"  OnClick="UpdateButton_Click" />
                        <asp:Button ID="UpdateCloseButton" Text="$update_close" ValidationGroup="sr"  OnClick="UpdateCloseButton_Click" runat="server" CausesValidation="True" />
                    </div>	           
                </div>
            </div>
        </two:TabularView>
    </two:TabularMultiView>

    
</asp:Content>
