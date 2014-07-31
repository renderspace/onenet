<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileManager.ascx.cs" Inherits="OneMainWeb.AdminControls.FileManager" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL" %>
<one:Notifier runat="server" ID="Notifier1" />
    <div class="searchFull">
		<asp:Panel ID="PanelUpload" runat="server" CssClass="col-md-4">
            <asp:FileUpload ID="fileUpload" runat="server" />
            <asp:LinkButton ID="cmdUpload" ValidationGroup="upload" OnClick="cmdUpload_Click"  runat="server" Text="<span class='glyphicon glyphicon-plus'></span> Upload" CssClass="btn btn-success" />
	        <asp:LinkButton ID="cmdOverwrite" ValidationGroup="upload"  runat="server" Text="Overwrite" CssClass="btn btn-warning" Visible="false" /> 
                        
		</asp:Panel>
		<div class="col-md-4">
            <asp:Label ID="lblSearchMessage" runat="server" CssClass="warning"></asp:Label>
            <asp:TextBox ID="TextBoxSearch" runat="server" placeholder="Search ID" ValidationGroup="search"></asp:TextBox>
            <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Search"  CssClass="btn btn-info" OnClick="cmdSearch_Click" ValidationGroup="search" />
		</div>
		<div class="col-md-4">
                       
		</div>
    </div>


<div class="col-md-3">
    <section class="module tall">
        <header>
            <asp:Panel runat="server" ID="PanelAddFolder" CssClass="addStuff">
                <asp:TextBox runat="server" ID="TextBoxFolder" placeholder="Add folder"></asp:TextBox>
                <asp:LinkButton ID="ButtonAddFolder" runat="server"  ValidationGroup="AddFolder" OnClick="TreeNodeAdd_Click" text="<span class='glyphicon glyphicon-plus'></span> Add"  CssClass="btn btn-success" />
            </asp:Panel>
        </header>
        <div class="treeview">
	        <asp:TreeView EnableViewState="false" ID="TreeViewFolders" runat="server" PopulateNodesFromClient="false" OnSelectedNodeChanged="TreeViewFolders_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeViewFolders_SelectedNodeChanged" />
        </div>
    </section>
</div>

<div class="col-md-9">
<div class="mainEditor ce-it-2">
    <div class="contentEntry">
        <asp:Button OnClick="CmdRecursiveDelete_Click" id="CmdRecursiveDelete" runat="server" Text="$recursive_delete" />
        <asp:CheckBox ID="CheckBoxConfirm" runat="server" />
    </div> 
    <asp:PlaceHolder ID="filesHolder" runat="server">
        <div class="smallgv">
	    <asp:GridView ID="fileGrid" OnRowDataBound="fileGrid_RowDataBound" runat="server" AutoGenerateColumns="False" DataKeyNames="Id">
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
		        <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Literal runat="server" ID="imageSymbol" /><br />
                        <asp:Literal ID="fileIdLit" runat="server" Visible="false" Text='<%# Eval("Id") %>' />
						(<%# (Container.DataItem as BOFile).Size / 1024 %> kB)
                    </ItemTemplate>
		        </asp:TemplateField>		    
		        <asp:TemplateField HeaderText="Id">
                    <ItemTemplate>
                            <%# Eval("Id") %>
                    </ItemTemplate>
		        </asp:TemplateField>
		        <asp:TemplateField HeaderText="$file_name" ItemStyle-CssClass="FileName">
                    <ItemTemplate>
                        <asp:Label ID="lblFileName" runat="server" Text='<%# Eval("Name") %>' />
                    </ItemTemplate>
		        </asp:TemplateField>		        
		        <asp:TemplateField>
		            <ItemTemplate>
                        <a href="#" data-id='<%# DataBinder.Eval(Container.DataItem, "Id") %>'  class="btn btn-info btn-xs"><span class='glyphicon glyphicon-pencil'></span> Edit</a>
		            </ItemTemplate>
		        </asp:TemplateField>
		    </Columns>
	    </asp:GridView>
            <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
        
	       	</div>
    </asp:PlaceHolder>	  
</div>
</div>     