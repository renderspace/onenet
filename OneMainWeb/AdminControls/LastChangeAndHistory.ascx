<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LastChangeAndHistory.ascx.cs" Inherits="OneMainWeb.AdminControls.LastChangeAndHistory" %>
<div class="lastChange"><a href="#" class="popup-audit-history" data-toggle="modal" data-target="#audit-history"><asp:Label runat="server" ID="LabelChanged"></asp:Label></a></div>

<div class="modal" id="audit-history" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" data-content-id="<%: SelectedContentId %>" data-language-id="<%: SelectedLanguageId %>">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                <h4 class="modal-title" id="myModalLabel">Audit history</h4>
            </div>
            <div class="modal-body audit-history-table">
                <table id="audit-history-table" class="table table-condensed">
                    <thead>
                        <tr>
                            <th>Last change</th>
                            <th>Title</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
                <div class="clearfix"></div>
            </div>
            <div class="modal-footer">
            <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>