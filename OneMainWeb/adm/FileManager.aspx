<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="FileManager.aspx.cs" Inherits="OneMainWeb.FileManager" Title="One.NET Filemanager" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentModal" Src="~/AdminControls/TextContentModal.ascx" %>


<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        
<one:Notifier runat="server" ID="Notifier1" />
    <div class="adminSection">
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
        <div id="tree"></div>
        <asp:HiddenField runat="server" ID="HiddenSelectedFolderId" ClientIDMode="Static" />
        <asp:HiddenField runat="server" ID="HiddenFieldLanguageId" ClientIDMode="Static" />
    </section>
</div>

<div class="col-md-9">
    <div class="mainEditor ce-it-2">
            <div class="col-sm-offset-3 col-sm-9">
                <div class="col-sm-6 pull-right">
                    <asp:CheckBox ID="CheckBoxConfirm" runat="server" Text="Confirm delete folder and all subfolders"  />    
                </div>
                <div class="col-sm-6 pull-right">
                    
                        <asp:LinkButton OnClick="CmdRecursiveDelete_Click" id="CmdRecursiveDelete" runat="server" Text="<span class='glyphicon glyphicon-trash'></span> Delete folder" CssClass="btn btn-danger" />
                </div>
            </div>

        <table id="files-table" class="table">
            <thead>
                <tr>
                    <th><input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" /></th>
                    <th>ID</th>
                    <th>Preview</th>
                    <th>Size</th>
                    <th>Filename</th>
                    <th></th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>

        <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" ClientIDMode="Static" OnClick="ButtonDelete_Click" />
        <one:TextContentModal runat="server" ID="TextContentModal1" />
    </div>
</div>     
</asp:Content>
