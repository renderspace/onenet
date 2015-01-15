<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldDynamicEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldDynamicEditor" %>
<asp:Literal ID="LiteralJQuery" runat="server" EnableViewState="false"></asp:Literal>		

<div class="adminSection form-horizontal validationGroup">
    <asp:Literal ID="LiteralResultsDebug" runat="server" EnableViewState="false"></asp:Literal>
    <asp:PlaceHolder ID="PanelFieldsHolder" runat="server"></asp:PlaceHolder>
    <div class="form-group">
            <div class="col-sm-offset-3 col-sm-9">
                <asp:LinkButton ID="ButtonCancel" runat="server" Text="Cancel" onclick="ButtonCancel_Click" ValidationGroup="Cancel" CausesValidation="false" UseSubmitBehavior="false" CssClass="btn btn-default" />
                <asp:LinkButton ID="ButtonSave" runat="server" Text="Save" onclick="ButtonSave_Click" CssClass="btn btn-success causesValidation" />
                <asp:LinkButton ID="ButtonSaveAndClose" runat="server" onclick="ButtonSaveAndClose_Click" CssClass="btn btn-success causesValidation" />
            </div>
    </div>
</div>