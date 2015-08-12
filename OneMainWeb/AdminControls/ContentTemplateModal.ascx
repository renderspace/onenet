<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentTemplateModal.ascx.cs" Inherits="OneMainWeb.AdminControls.ContentTemplateModal" EnableViewState="false" %>
<%@ Import Namespace="System.Threading" %>

<div class="modal fade" id="content-template-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel2" aria-hidden="true">
    
    <div class="modal-dialog modal-lg">
        
        <div class="modal-content validationGroup">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel2"><%= Title %></h4>
            </div>
            <div class="modal-body form-horizontal">
                <input type="hidden" class="j_control_content_template_instance_id" />
                <input type="hidden" class="j_control_template_id" />
                <input type="hidden" class="j_control_principal" value="<%=Thread.CurrentPrincipal.Identity.Name %>" />
                <div class="lastChange"><span class="j_control_principal"></span></div>
                <div class="clearfix"></div>
                <div class="content-fields"></div>
                <div class="clearfix"></div>

            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                <a href="#" Class="btn btn-success causesValidation">Save</a>
                <div class="loading"></div>
            </div>
        </div>

    </div>

</div>