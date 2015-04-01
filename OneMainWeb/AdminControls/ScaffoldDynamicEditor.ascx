<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldDynamicEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldDynamicEditor" %>
<asp:Literal ID="LiteralJQuery" runat="server" EnableViewState="false"></asp:Literal>		

<div class="adminSection form-horizontal validationGroup">
    <asp:Literal ID="LiteralResultsDebug" runat="server" EnableViewState="false"></asp:Literal>
    <asp:PlaceHolder ID="PanelFieldsHolder" runat="server"></asp:PlaceHolder>
    <div class="form-group">
            <div class="col-sm-offset-3 col-sm-9">
                <asp:LinkButton ID="ButtonSave" runat="server" Text="Save" onclick="ButtonSave_Click" CssClass="btn btn-success causesValidation" />
                <asp:LinkButton ID="ButtonSaveAndClose" runat="server" onclick="ButtonSaveAndClose_Click" CssClass="btn btn-success causesValidation" />
            </div>
    </div>
</div>


<div class="modal" id="to-many-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
    <div class="modal-backdrop in"></div>    
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel"></h4>
            </div>
            <div class="modal-body form-horizontal">
                <input type="hidden" class="j_control_language_id" />
                <input type="hidden" class="j_control_content_id" />
                <input type="hidden" class="j_control_file_id" />
                
                <div class="form-group">
                    <label class="col-sm-3 control-label">Modal window for item edit</label> 
                    <div class="col-sm-3">
                        <span class="j_control_language"></span>
                    </div>
                    <div class="col-sm-3 j_control_file">
                   
                    </div>
                </div>
                <div class="clearfix"></div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                <a href="#" Class="btn btn-success">Save & Close</a>
            </div>
        </div>
    </div>
</div>