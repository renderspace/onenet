<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextContentModal.ascx.cs" Inherits="OneMainWeb.AdminControls.TextContentModal" EnableViewState="false" %>
<div class="modal" id="text-content-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" data-language-id="<%: SelectedLanguageId %>">
    
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel">File description edit</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label class="col-sm-3 control-label">Language</label> 
                    <div class="col-sm-3">
                        <span class="j_control_languege"></span>
                    </div>
                    <div class="col-sm-3 j_control_file">
                   
                    </div>
                </div>

                <div class="clearfix"></div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Title</label>
                    <div class="col-sm-9">
                        <input type="text" maxlength="255" class="form-control" id="content-title" />
                    </div>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Subtitle</label>
                    <div class="col-sm-9">
                        <input type="text" maxlength="255" class="form-control" id="content-subtitle" />
                    </div>
                </div>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Teaser</label>
                    <div class="col-sm-9">
                        <textarea  rows="5" class="form-control" id="content-teaser"></textarea>
                    </div>
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