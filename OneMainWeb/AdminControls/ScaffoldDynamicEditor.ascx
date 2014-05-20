<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldDynamicEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldDynamicEditor" %>
<asp:Literal ID="LiteralJQuery" runat="server" EnableViewState="false"></asp:Literal>		
<div class="DynamicEditor">
    <asp:Literal ID="LiteralResultsDebug" runat="server" EnableViewState="false"></asp:Literal>
    <asp:Panel ID="PanelFieldsHolder" runat="server"></asp:Panel>
    <div class="save">
        <asp:Button ID="ButtonCancel" runat="server" Text="$cancel" onclick="ButtonCancel_Click" ValidationGroup="Cancel" CausesValidation="false" UseSubmitBehavior="false" />
        <asp:Button ID="ButtonSave" runat="server" Text="$save" onclick="ButtonSave_Click" />
        <asp:Button ID="ButtonSaveAndClose" runat="server" onclick="ButtonSaveAndClose_Click" />
    </div>
</div>