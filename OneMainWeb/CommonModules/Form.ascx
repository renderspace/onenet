<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Form.ascx.cs" Inherits="Forms.Form" %>
<%@ Register TagPrefix="two" Namespace="Forms" Assembly="Forms" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Import Namespace="Forms.BLL" %>

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

    window.onload = function (e) {
        if (window.language == 'bs' ||
            window.language == 'hr' ||
            window.language == 'sl' ||
            window.language == 'sr') {

            // Replace the builtin US date validation with bs/hr/sl/sr date validation
            $.validator.addMethod(
                "date",
                function (value, element) {
                    // have to do this stuff because of Firefox date implementation

                    var bits = value.match(/([0-9]+)/gi), str;
                    if (!bits || bits.length < 3)
                        return this.optional(element) || false;
                    var reconstructed = bits[0] + '.' + bits[1] + '.' + bits[2];
                    if (value !== reconstructed)
                        return this.optional(element) || false;

                    var day = parseInt(bits[0], 10);
                    var month = parseInt(bits[1], 10);
                    var year = parseInt(bits[2], 10);

                    if (month < 1 || month > 12 || day < 1 || day > 31 || year < 1 || year > 9999)
                        return this.optional(element) || false;

                    var timestamp = Date.parse(month + '/' + day + '/' + year);
                    if (isNaN(timestamp) == false) {
                        var d = new Date(timestamp);
                        return this.optional(element) || true;
                    }
                    return this.optional(element) || false;
                }
            );
        }
    }

</script>

<div class="validationGroup">
    <asp:PlaceHolder ID="PlaceHolderAcutalForm" runat="server" />
    <asp:PlaceHolder ID="PlaceHolderResults" runat="server">
        <h2 runat="server" id="DivFormTitle"></h2>
        <asp:Panel ID="PanelThankYouNote" runat="server" Visible="false" CssClass="form-thank-you" noid="True">
            <script>
                $(function () {
                    var page = document.location.pathname + '/thank-you-for-form-<%= FormId %>' + location.search + location.hash;
                    if (window.ga) {
                        ga('send', 'pageview', page);
                    } else {
                        console.log("no GA... but if it was here, we would track this: " + page);
                    }
                });
            </script>
            <asp:Literal ID="lblThankYouNote" runat="server"></asp:Literal>
        </asp:Panel>
    </asp:PlaceHolder>

    <asp:Panel runat="server" ID="PanelError" CssClass="form-error">
        <h2 style="color: red;">Form error</h2>
        <p style="color: red;"><asp:Literal runat="server" ID="LiteralErrorMessage"></asp:Literal></p>
    </asp:Panel>

    <asp:Panel ID="PanelCommands" runat="server" CssClass="form-group form-submit" noid="True">
        <asp:Button ID="cmdPrev" runat="server" Text="prev" OnClick="cmdPrev_Click" CssClass="btn btn-prev" />    
        <asp:Button ID="cmdNext" runat="server" Text="next" OnClick="cmdNext_Click" CssClass="btn btn-next causesValidation" />    
        <asp:Button ID="cmdSubmit" runat="server" Text="submit" OnClick="cmdSubmit_Click" CssClass="btn btn-submit causesValidation" />    
    </asp:Panel>
</div>
