<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BCarousel.ascx.cs" Inherits="OneMainWeb.CommonModules.BCarousel" EnableViewState="false" %>
<ol class="carousel-indicators">
    <asp:Repeater runat="server" ID="RepeaterButtons" OnItemDataBound="RepeaterButtons_ItemDataBound">
        <ItemTemplate>
                    <asp:Literal runat="server" ID="LiteralNavig"></asp:Literal>
        </ItemTemplate>
    </asp:Repeater>
</ol>
<div class="carousel-inner">
    <asp:Repeater runat="server" ID="RepeaterImages" OnItemDataBound="RepeaterImages_ItemDataBound">
        <ItemTemplate>
                    <asp:Literal runat="server" ID="LiteralImage"></asp:Literal>
        </ItemTemplate>
    </asp:Repeater>
</div>
<a class="left carousel-control" href="#<%: CustomClientID %>" role="button" data-slide="prev">
	<span class="glyphicon glyphicon-chevron-left"></span>
</a>
<a class="right carousel-control" href="#<%: CustomClientID %>" role="button" data-slide="next">
	<span class="glyphicon glyphicon-chevron-right"></span>
</a>