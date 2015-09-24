<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MagnificGallery.ascx.cs" Inherits="OneMainWeb.CommonModules.MagnificGallery" EnableViewState="false" %>
<asp:Repeater runat="server" ID="RepeaterImages" OnItemDataBound="RepeaterImages_ItemDataBound">
    <HeaderTemplate>
        <ul>
    </HeaderTemplate>
    <FooterTemplate>
        </ul>
    </FooterTemplate>
    <ItemTemplate>
        <li>
            <figure>
                <asp:Literal runat="server" ID="LiteralImageTag"></asp:Literal>
                <figcaption>
                    <asp:Literal runat="server" ID="LiteralTitle"></asp:Literal>
                    <asp:Literal runat="server" ID="LiteralCaption"></asp:Literal>
                </figcaption>                                   
            </figure>
        </li>
    </ItemTemplate>
</asp:Repeater>
<asp:Literal runat="server" ID="LiteralMessage"></asp:Literal>