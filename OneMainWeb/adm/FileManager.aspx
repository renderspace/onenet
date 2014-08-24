<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="FileManager.aspx.cs" Inherits="OneMainWeb.FileManager" Title="One.NET Filemanager" ValidateRequest="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="TextContentModal" Src="~/AdminControls/TextContentModal.ascx" %>


<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
     <style>
    html, body {
      height: 100%;
    }
    #actions {
      margin: 2em 0;
    }


    /* Mimic table appearance */
    div.table {
      display: table;
    }
    div.table .file-row {
      display: table-row;
    }
    div.table .file-row > div {
      display: table-cell;
      vertical-align: top;
      border-top: 1px solid #ddd;
      padding: 8px;
    }
    div.table .file-row:nth-child(odd) {
      background: #f9f9f9;
    }



    /* The total progress gets shown by event listeners */
    #total-progress {
      opacity: 0;
      transition: opacity 0.3s linear;
    }

    /* Hide the progress bar when finished */
    #previews .file-row.dz-success .progress {
      opacity: 0;
      transition: opacity 0.3s linear;
    }

    /* Hide the delete button initially */
    #previews .file-row .delete {
      display: none;
    }

    /* Hide the start and cancel buttons and show the delete button */

    #previews .file-row.dz-success .start,
    #previews .file-row.dz-success .cancel {
      display: none;
    }
    #previews .file-row.dz-success .delete {
      display: block;
    }

    //http://www.dropzonejs.com/bootstrap.html
  </style>

</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        
<one:Notifier runat="server" ID="Notifier1" />
    <div class="adminSection">
		<asp:Panel ID="PanelUpload" runat="server" CssClass="col-md-4 ">

           <div class="fallback">
                <input name="file" type="file" multiple />
            </div>

             <div id="actions" class="row">

                <div class="col-lg-7">
                <!-- The fileinput-button span is used to style the file input field as button -->
                <span class="btn btn-success fileinput-button">
                    <i class="glyphicon glyphicon-plus"></i>
                    <span>Add files...</span>
                </span>
                <button type="submit" class="btn btn-primary start">
                    <i class="glyphicon glyphicon-upload"></i>
                    <span>Start upload</span>
                </button>
                <button type="reset" class="btn btn-warning cancel">
                    <i class="glyphicon glyphicon-ban-circle"></i>
                    <span>Cancel upload</span>
                </button>
                </div>

                <div class="col-lg-5">
                <!-- The global file processing state -->
                <span class="fileupload-process">
                    <div id="total-progress" class="progress progress-striped active" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0">
                    <div class="progress-bar progress-bar-success" style="width:0%;" data-dz-uploadprogress></div>
                    </div>
                </span>
                </div>

            </div>
            
                        
		</asp:Panel>
		<div class="col-md-4 validationGroup">
            <asp:Label ID="lblSearchMessage" runat="server" CssClass="warning"></asp:Label>
            <asp:TextBox ID="TextBoxSearch" runat="server" placeholder="Search ID" CssClass="digits required"></asp:TextBox>
            <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Search"  CssClass="btn btn-info causesValidation" OnClick="cmdSearch_Click"  />
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
                        <asp:Label runat="server" ID="LabelFolderId" ClientIDMode="Static"></asp:Label>
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
