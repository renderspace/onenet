<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldDynamicEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldDynamicEditor" %>

    <script type="text/javascript" src="/JavaScript/jquery-1.3.2.min.js"></script>
    <script type="text/javascript" src="/JavaScript/jquery.bgiframe.min.js"></script>
    <script type="text/javascript" src="/JavaScript/jquery.autocomplete.min.js"></script>
    <script type="text/javascript" src="/JavaScript/jquery.validate.min.js"></script> 
    <script type="text/javascript" src="/JavaScript/additional-methods.js"></script> 

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