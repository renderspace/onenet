<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentModal.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentModal" EnableViewState="false" %>

<div class="modal" id="text-content-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" data-language-id="<%: SelectedLanguageId %>" data-language="<%: SelectedLanguage %>">
    <div class="modal-backdrop in"></div>    
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel"><%: Title %></h4>
            </div>
            <div class="modal-body form-horizontal">
                <input type="hidden" class="j_control_language_id" />
                <input type="hidden" class="j_control_content_id" />
                <input type="hidden" class="j_control_file_id" />
                
                <div class="form-group sessionTimeoutError" style="display:none">
                    <p class="alert alert-danger col-sm-10 col-sm-offset-1">
                        Your session has timed out. Please copy your text to a texteditor and relogin to one.net.
                    </p>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Language</label> 
                    <div class="col-sm-3">
                        <span class="j_control_language"></span>
                    </div>
                    <div class="col-sm-3 j_control_file">
                   
                    </div>
                </div>

                <div class="clearfix"></div>

                <div class="form-group" id="form-title">
                    <label class="col-sm-3 control-label">Title</label>
                    <div class="col-sm-9">
                        <input type="text" maxlength="255" class="form-control" id="content-title" />
                    </div>
                </div>

                <div class="form-group" id="form-subtitle">
                    <label class="col-sm-3 control-label">Subtitle</label>
                    <div class="col-sm-9">
                        <input type="text" maxlength="255" class="form-control" id="content-subtitle" />
                    </div>
                </div>

                <div class="form-group" id="form-teaser">
                    <label class="col-sm-3 control-label">Teaser</label>
                    <div class="col-sm-9">
                        <textarea  rows="5" class="form-control" id="content-teaser"></textarea>
                    </div>
                </div>

                <div class="form-group ckbox">
                </div>
                
                <div class="clearfix"></div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                <a href="#" Class="btn btn-success modal-save">Save</a>
                <a href="#" Class="btn btn-success modal-save-close">Save and close</a>
            </div>
            <div class="loading"></div>
        </div>
    </div>
</div>