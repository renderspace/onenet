<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldDynamicEditor.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldDynamicEditor" %>
<asp:Literal ID="LiteralJQuery" runat="server" EnableViewState="false"></asp:Literal>		

<div class="page-and-module-settings">
    <section class="module">
        <asp:Literal ID="LiteralResultsDebug" runat="server" EnableViewState="false"></asp:Literal>
        <asp:PlaceHolder ID="PanelFieldsHolder" runat="server"></asp:PlaceHolder>
        <div class="save">
            <asp:Button ID="ButtonCancel" runat="server" Text="$cancel" onclick="ButtonCancel_Click" ValidationGroup="Cancel" CausesValidation="false" UseSubmitBehavior="false" CssClass="btn btn-default" />
            <asp:Button ID="ButtonSave" runat="server" Text="Save" onclick="ButtonSave_Click" CssClass="btn btn-success" />
            <asp:Button ID="ButtonSaveAndClose" runat="server" onclick="ButtonSaveAndClose_Click" CssClass="btn btn-success" />
        </div>
     </section>
    </div>