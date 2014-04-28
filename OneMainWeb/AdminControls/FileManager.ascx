<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileManager.ascx.cs" Inherits="OneMainWeb.AdminControls.FileManager" %>
<%@ Register TagPrefix="one" TagName="TreeCategorization" Src="~/AdminControls/TreeCategorization.ascx" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="CategoryEditor" Src="~/AdminControls/CategoryEditor.ascx" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="TextContentControl.ascx" %>
<%@ Import Namespace="One.Net.BLL" %>
<one:Notifier runat="server" ID="Notifier1" />
<div class="centerStructure">
    <div id="divUploadFile" runat="server" class="fmFiles">
	    <asp:FileUpload ID="fileUpload" runat="server" />
	    <asp:Button ID="cmdUpload" ValidationGroup="upload" OnClick="cmdUpload_Click" cssclass="leftButton" runat="server" Text="$upload" />
	    <asp:Button ID="cmdOverwrite" ValidationGroup="upload" OnClick="cmdOverwrite_Click" cssclass="leftButton" runat="server" Text="$overwrite" />	    
	</div>
	<div id="divShowFile" runat="server" class="fmFiles">
	    <asp:Literal ID="imagePreview" runat="server" />
	    <asp:Literal ID="imagePreviewSize" runat="server" />
	</div>
	
	<asp:Label ID="lblSearchMessage" runat="server" CssClass="warning"></asp:Label>
    <two:InputWithButton ValidationType="Integer" ValidationGroup="search" ID="InputWithButtonSearch" Text="$search_file_id" ButtonText="$search" onclick="cmdSearch_Click" runat="server" />
    
    <one:TreeCategorization ID="categorization" OnNodeAdded="categorization_NodeAdded" runat="server" OnAdaptedSelectedNodeChanged="categorization_SelectedNodeChanged" OnSelectedNodeChanged="categorization_SelectedNodeChanged" />
</div>

<div class="mainEditor">
    <div class="contentEntry">
        <asp:Button OnClick="CmdRecursiveDelete_Click" id="CmdRecursiveDelete" runat="server" Text="$recursive_delete" />
        <asp:CheckBox ID="CheckBoxConfirm" runat="server" />
        <one:CategoryEditor OnCategoryMoved="categoryEditor_CategoryMoved" OnCategoryFailedToMove="categoryEditor_CategoryFailedToMove" ID="categoryEditor" ShowCategorizedItemsGrid="false" ShowIsSelectable="false" ShowIsPrivate="true" runat="server" OnCategoryUpdated="categoryEditor_CategoryUpdated" OnCategoryDeleted="categoryEditor_CategoryDeleted" UseFckEditor="false" TitleVisible="true" SubTitleVisible="false" TeaserVisible="false" HtmlVisible="false" />       
    </div> 
    <asp:PlaceHolder ID="filesHolder" runat="server">
        <div class="smallgv">
	    <asp:GridView ID="fileGrid" OnRowDataBound="fileGrid_RowDataBound" OnRowCommand="fileGrid_RowCommand" runat="server" AutoGenerateColumns="False" DataSourceID="FileListODS" CssClass="gvsmall" DataKeyNames="Id">
		    <Columns>
		        <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Literal runat="server" ID="imageSymbol" /><br />
                        <asp:Literal ID="fileIdLit" runat="server" Visible="false" Text='<%# Eval("Id") %>' />
						(<%# (Container.DataItem as BOFile).Size / 1024 %> kB)
                    </ItemTemplate>
		        </asp:TemplateField>		    
		        <asp:TemplateField HeaderText="$id">
                    <ItemTemplate>
                        <span class="itemId">
                            <%# Eval("Id") %>
                            <asp:Literal ID="LiteralHiddenFileId" runat="server" Text='<%# Eval("Id") %>' Visible="false" ></asp:Literal>
                        </span>
                        <span class="button">
                            <asp:LinkButton ID="cmdDelete" CommandArgument='<%# Container.DataItemIndex %>' CommandName="Delete" runat="server" Text="Delete" />
                        </span>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <%# Eval("Id") %>
                        <asp:Literal ID="LiteralHiddenFileId" runat="server" Text='<%# Eval("Id") %>' Visible="false" ></asp:Literal>                        
                    </EditItemTemplate>
		        </asp:TemplateField>
		        <asp:TemplateField HeaderText="$file_name" ItemStyle-CssClass="FileName">
                    <ItemTemplate>
                        <asp:Label ID="lblFileName" runat="server" Text='<%# Eval("Name") %>' /><br />
                        <%# Eval("Content") != null && (bool)Eval("Content.IsRated") ? ResourceManager.GetString("$votes") + " : " + Eval("Content.Votes").ToString() + " " + ResourceManager.GetString("$score") + " : " + Eval("Content.Score").ToString() : string.Empty%>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <strong><asp:Label ID="lblFileName" runat="server" Text='<%# Eval("Name") %>' /></strong>
                        <em><asp:Label ID="Label1" runat="server" Text='<%# Eval("Content.ContentId") %>' /></em><br />
                        <one:TextContentControl ID="txtFileContent" runat="server" HtmlVisible="false" Title='<%# Eval("Content.Title") %>' SubTitle='<%# Eval("Content.SubTitle") %>' Teaser='<%# Eval("Content.Teaser") %>' Html=''  />
                    </EditItemTemplate>
		        </asp:TemplateField>		        
		        <asp:TemplateField>
		            <ItemTemplate>
		                <asp:LinkButton ID="cmdEdit" CommandArgument='<%# Container.DataItemIndex %>' CommandName="EditRow" runat="server" Text="$edit" />
                        <asp:Literal id="litSelectButton" runat="server" Text='<%# GetFileUrl(DataBinder.Eval(Container.DataItem, "Id")) %>' />		                
		            </ItemTemplate>
		            <EditItemTemplate>
		                <asp:LinkButton ID="cmdSave" CommandArgument='<%# Container.DataItemIndex %>' CommandName="UpdateFile" runat="server" Text="$update" />
		                <asp:LinkButton ID="cmdCancel" CommandArgument='<%# Container.DataItemIndex %>' CommandName="Cancel" runat="server" Text="$cancel" />
		            </EditItemTemplate>
		        </asp:TemplateField>
		        <asp:TemplateField>
		            <ItemTemplate>
                    <asp:LinkButton runat="server" ID="cmdMoveFile" Text="$move" CommandName="MoveFile" CommandArgument='<%# Container.DataItemIndex %>' />                        
	                <two:modalpanel ShowCloseButton="true" OnWindowClosed="moveFoldersPanel_WindowClosed" Visible="false" id="moveFoldersPanel" runat="server" >
	                    <div class="outerBorder">
	                        <div class="innerBorder">
	                            <two:InfoLabel ID="lblMovingFileName" Text="$file_to_move" runat="server" Value='<%# Eval("Name") %>' />	    
	                            <div id="treeHolder" runat="server" class="treeHolder">
	                                <asp:TreeView ID="moveFoldersTree" OnAdaptedSelectedNodeChanged="moveFoldersTree_SelectedNodeChanged" OnSelectedNodeChanged="moveFoldersTree_SelectedNodeChanged" Width="320px" runat="server" SelectedNodeStyle-BackColor="Gray" />
	                            </div>
	                        </div>
	                    </div>
	                </two:modalpanel>    		            
		            </ItemTemplate>
		        </asp:TemplateField>
		    </Columns>
	    </asp:GridView>
        
	       	</div>
	    <asp:ObjectDataSource OnDeleted="FileListODS_Deleted" OnSelecting="FileListODS_Selecting" DeleteMethod="Delete" EnablePaging="False" ID="FileListODS" runat="server" TypeName="OneMainWeb.AdminControls.FileHelper" SelectMethod="List">
		    <SelectParameters>
			    <asp:ControlParameter Name="folderId" ControlID="categorization" PropertyName="SelectedCategory.Id" Type="Int32" />
		    </SelectParameters>
		    <DeleteParameters>
		        <asp:Parameter Name="Id" Type="Int32" />
		    </DeleteParameters>
	    </asp:ObjectDataSource>		
    </asp:PlaceHolder>	  
     
</div>