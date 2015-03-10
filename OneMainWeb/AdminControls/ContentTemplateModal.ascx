<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentTemplateModal.ascx.cs" Inherits="OneMainWeb.AdminControls.ContentTemplateModal" EnableViewState="false" %>
<div class="modal fade" id="content-template-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel2" aria-hidden="true">
    
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel2"><%: Title %></h4>
            </div>
            <div class="modal-body form-horizontal">
                <input type="hidden" class="j_control_content_template_id" />
                <input type="hidden" class="j_control_template_id" />

                <div class="form-group">
                    <label class="col-sm-3 control-label">Date Created</label> 
                    <div class="col-sm-9">
                        <span class="j_control_date_created"></span>
                    </div>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Principal Created</label> 
                    <div class="col-sm-9">
                        <span class="j_control_principal_created"></span>
                    </div>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Date Modified</label> 
                    <div class="col-sm-9">
                        <span class="j_control_date_modified"></span>
                    </div>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Principal Modified</label> 
                    <div class="col-sm-9">
                        <span class="j_control_principal_modified"></span>
                    </div>
                </div>

                <div class="clearfix"></div>
                <div class="content-fields">

                </div>

                <div class="clearfix"></div>

            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                <a href="#" Class="btn btn-success">Save</a>
            </div>
        </div>
    </div>
</div>