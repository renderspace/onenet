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

    #PanelUpload {  height: 120px;  background: url('/adm/images/drag.png') no-repeat center center; padding: 20px; cursor: pointer; border: 2px #ccc dashed; }
    #PanelUpload:hover { background-color: #eee;  }

  </style>
    <script>
        Dropzone.autoDiscover = false;
        $(document).ready(function () {
            var previewNode = document.querySelector("#template");
            previewNode.id = "";
            var previewTemplate = previewNode.parentNode.innerHTML;
            previewNode.parentNode.removeChild(previewNode);

            var myDropzone = new Dropzone("div#PanelUpload", {
                url: "/adm/FileManager.aspx",
                autoProcessQueue: true,
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 20,
                previewTemplate: previewTemplate,
                clickable: ".fileinput-button",
                previewsContainer: "#previews" // Define the container to display the previews
            });

            myDropzone.on("sending", function (file, xhr, formData) {
                var selectedFolderId = $('#HiddenSelectedFolderId').val();
                trace("sending to: " + selectedFolderId);
                formData.append("SelectedFolderId", selectedFolderId);
            });

            myDropzone.on("complete", function (file) {
                trace("complete");
                if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                    var selectedFolderId = $('#HiddenSelectedFolderId').val();
                    trace("complete: " + selectedFolderId);
                    files_databind(selectedFolderId);
                    $(".adminSection").before('<div class="alert alert-success"><p><span>Uploaded files.</span></p></div>');
                    //$(".alert").remove();
                    $("#previews").empty();

                }
            });

            //myDropzone.on("totaluploadprogress", function (progress) {
            //    document.querySelector("#total-progress .progress-bar").style.width = progress + "%";
            //});

        });
    </script>

</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        
<one:Notifier runat="server" ID="Notifier1" />
    <div class="adminSection">
		<div class="row">
		<div class="col-md-4 validationGroup">
            <asp:Label ID="lblSearchMessage" runat="server" CssClass="warning"></asp:Label>
            <asp:TextBox ID="TextBoxSearch" runat="server" placeholder="Search ID" CssClass="digits required"></asp:TextBox>
            <asp:LinkButton ID="ButtonDisplayById" runat="server" Text="Search"  CssClass="btn btn-info causesValidation" OnClick="cmdSearch_Click"  />
		</div>
		<div class="col-md-8">
		<asp:Panel ID="PanelUpload" runat="server" ClientIDMode="static">
           <div class="fallback">
                <input name="file" type="file" multiple />
            </div>
		</asp:Panel>
		</div>
		</div>
    </div>

    <div class="table table-striped files" id="previews">

      <div id="template" class="file-row">
        <!-- This is used as the file preview template -->
        <div>
            <span class="preview"><img data-dz-thumbnail /></span>
        </div>
        <div>
            <p class="name" data-dz-name></p>
            <strong class="error text-danger" data-dz-errormessage></strong>
        </div>
        <div>
            <p class="size" data-dz-size></p>
            <div class="progress progress-striped active" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0">
              <div class="progress-bar progress-bar-success" style="width:0%;" data-dz-uploadprogress></div>
            </div>
        </div>
        <div>
          <button data-dz-remove class="btn btn-warning cancel">
              <i class="glyphicon glyphicon-ban-circle"></i>
              <span>Cancel</span>
          </button>
        </div>
      </div>

    </div>
 

<div class="tabl">
<div class="tabltr">
<div class="s1a">
    <section class="module tall msideb file-stucture-menu">
		<h2>Files structure</h2>
		<div class="msideb-inn">
        <header>
            <asp:Panel runat="server" ID="PanelAddFolder" CssClass="addStuff">
				<div class="form-inline fi-top">
                <div class="form-group">
					<asp:TextBox runat="server" ID="TextBoxFolder" placeholder="Add folder"></asp:TextBox>
				</div>
					<asp:LinkButton ID="ButtonAddFolder" runat="server"  ValidationGroup="AddFolder" OnClick="TreeNodeAdd_Click" text="<span class='glyphicon glyphicon-plus'></span> Add"  CssClass="btn btn-success" />
				</div>
		   </asp:Panel>
        </header>
        <div id="tree"></div>
        <asp:HiddenField runat="server" ID="HiddenSelectedFolderId" ClientIDMode="Static" />
        <asp:HiddenField runat="server" ID="HiddenFieldLanguageId" ClientIDMode="Static" />
		</div>
    </section>
</div>

<div class="s2a">
    <div class="mainEditor ce-it-2">
            <div class="row">
			    <div class="col-sm-3">
				    <span class="btn btn-success fileinput-button">
					    <i class="glyphicon glyphicon-plus"></i>
					    <span>Upload...</span>
				    </span>
			    </div>
                <div class="col-sm-9">
                       <h4><asp:Label runat="server" ID="LabelFolderId" ClientIDMode="Static"></asp:Label></h4>
                </div>
            </div>

            <table id="files-table" class="table">
                <thead style="display: none;">
                    <tr>
                        <th><input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" /></th>
                        <th></th>
                        <th>Preview</th>
                        <th>Size</th>
                        <th>Filename</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
         
            <div class="row fileManagerDeleteButtons" style="display: none;">
                <div class="col-sm-6">
                    <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" ClientIDMode="Static" OnClick="ButtonDelete_Click" />
                </div>
                <div class="col-sm-6">
                    <asp:LinkButton OnClick="CmdRecursiveDelete_Click" id="CmdRecursiveDelete" runat="server" Text="<span class='glyphicon glyphicon-trash'></span> Delete folder" CssClass="btn btn-danger deleteAll pull-right"/>
                    <span style="color: #ff0000; padding: 10px;" class="pull-right">DANGER!!!</span> 
                </div>
            </div>
        
        <one:TextContentModal runat="server" ID="TextContentModal1" EnableHtml="false" Title="File description edit" />
    </div>
</div>   
</div>
</div>  
</asp:Content>
