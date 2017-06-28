<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AzureSearchResults.ascx.cs" Inherits="OneMainWeb.CommonModules.AzureSearchResults" %>

<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<asp:Repeater runat="server" ID="RepeaterResults">
    <ItemTemplate>
        <div class="item">
            <a target="_blank" href="<%# Eval("url") %>"><%# Eval("name") %></a>
            <p><%# Eval("snippet") %></p>
        </div>
    </ItemTemplate>
</asp:Repeater>

<two:Pager id="PagerResults" runat="server" MaxColsPerRow="11" NumPagesShown="10" />

<asp:Panel runat="server" ID="PanelError">
    <%=TranslateComplex("azure_search_error") %>
</asp:Panel>