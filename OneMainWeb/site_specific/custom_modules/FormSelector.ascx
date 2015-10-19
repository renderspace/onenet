<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormSelector.ascx.cs" Inherits="Forms.FormSelector" %>

<div class="formpart">
    <asp:Label ID="lblForms" runat="server" AssociatedControlID="ddlForms" />
    <asp:DropDownList CssClass="listitem" AutoPostBack="true" OnSelectedIndexChanged="ddlForms_SelectedIndexChanged" runat="server" ID="ddlForms" />
</div>

<asp:Panel ID="PlaceHolderAcutalForm" runat="server" cssclass="forms" />
