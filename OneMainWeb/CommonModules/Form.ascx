<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Form.ascx.cs" Inherits="OneMainWeb.CommonModules.Form" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Import Namespace="One.Net.Forms" %>

<script>

    <% if (Thread.CurrentThread.CurrentCulture.LCID == 1033) { %>
    window.language = 'en';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 1031) { %>
    window.language = 'de';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 1040) { %>
    window.language = 'it';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 1050) { %>
    window.language = 'hr';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 1060) { %>
    window.language = 'sl';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 2074) { %>
    window.language = 'sr';
    <% } else if (Thread.CurrentThread.CurrentCulture.LCID == 5146) { %>
    window.language = 'bs';
    <% } %>

    window.FormId = <%= FormId %>;

</script>

<div class="validationGroup">
    <asp:Panel runat="server" CssClass="liner" ID="PanelProgress" EnableViewState="false">
        <ul>
            <asp:Literal runat="server" ID="LiteralProgressSteps" EnableViewState="false"/>
        </ul>
        
        <div class="colorliner"><asp:Literal runat="server" ID="LiteralProgress" EnableViewState="false"></asp:Literal></div>
    </asp:Panel>
    <asp:PlaceHolder ID="PlaceHolderAcutalForm" runat="server" />
    <asp:PlaceHolder ID="PlaceHolderResults" runat="server">
        <h2 runat="server" id="DivFormTitle"></h2>
        <asp:Panel ID="PanelThankYouNote" runat="server" Visible="false" CssClass="form-thank-you" noid="True">
            <asp:Literal ID="lblThankYouNote" runat="server"></asp:Literal>
        </asp:Panel>
    </asp:PlaceHolder>

    <asp:Panel runat="server" ID="PanelError" CssClass="form-error">
        <h2 style="color: red;">Form error</h2>
        <p style="color: red;"><asp:Literal runat="server" ID="LiteralErrorMessage"></asp:Literal></p>
    </asp:Panel>

    <asp:Panel ID="PanelCommands" runat="server" CssClass="form-group form-submit" noid="True">
        <asp:Button ID="cmdPrev" runat="server" Text="prev" OnClick="cmdPrev_Click" CssClass="btn btn-prev" />    
        <asp:Button ID="ButtonNext" runat="server" Text="next" OnClick="ButtonNext_Click" CssClass="btn btn-next causesValidation" />    
        <asp:Button ID="cmdSubmit" runat="server" Text="submit" OnClick="cmdSubmit_Click" CssClass="btn btn-submit causesValidation" />    
    </asp:Panel>
</div>
