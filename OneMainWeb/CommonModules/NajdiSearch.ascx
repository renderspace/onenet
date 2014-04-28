<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NajdiSearch.ascx.cs" Inherits="OneMainWeb.CommonModules.NajdiSearch" %>

<asp:PlaceHolder ID="PlaceHolderResults" runat="server">
    <asp:Repeater runat="server" ID="RepeaterResults">
        
        <HeaderTemplate><div class="results"></HeaderTemplate>
        <ItemTemplate>
            <div class="result">
                <h2><a href='<%# Eval("Url")%>'><%# Eval("Title")%></a></h2>
                <p><%# Eval("Abstract")%></p>
                <p class="links">
                    <span class="url"><%# Eval("Url")%></span>
                    <span class="length"><%# Eval("ContentLength")%> <%# Eval("ContentLengthUnit")%></span>
                    <a href='<%# Eval("Url")%>'><%# Translate("najdi_search_more")%></a>
                </p>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    
    </asp:Repeater>

    <asp:Repeater ID="RepeaterPages" runat="server">
        <HeaderTemplate><ul class="pages"></HeaderTemplate>
        <ItemTemplate><li class='<%# (bool)Eval("Selected") ? "sel" : "" %>'><a href='<%# Eval("Url")%>'><%# Int32.Parse((string)Eval("Index")) + 1 %></a></li></ItemTemplate>
        <FooterTemplate></ul></FooterTemplate>
    </asp:Repeater>

</asp:PlaceHolder>

<asp:PlaceHolder ID="PlaceHolderNoResults" runat="server">

    <%=Translate("najdi_search_no_results") %>

</asp:PlaceHolder>