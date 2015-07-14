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
<script>
    $(document).ready(function () {
        $('#<%= CustomClientID %>').magnificPopup({
            delegate: 'ul li figure a',
            type: 'image',
            gallery: { enabled: true },
            image: {
                titleSrc: function (item) {
                    return item.el.parents('li').find('figcaption').html();
                }
            }
        });
    });
</script>
