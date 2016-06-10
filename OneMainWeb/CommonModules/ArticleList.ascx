<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleList.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleList" EnableViewState="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<h2 runat="server" id="H2ModuleTitle" noid="True" visible="false"></h2>
<asp:Repeater runat="server" ID="RepeaterArticles" OnItemDataBound="RepeaterArticles_ItemDataBound">
    <ItemTemplate>
		<article id="HtmlArticle" runat="server" noid="True">
            <div class="teaser-image" id="TeaserImage1" runat="server" noid="True" visible="false">
                <a href="<%# RenderLink((string)Eval("HumanReadableUrl"))  %>"><asp:Literal runat="server" ID="LiteralTeaserImage"></asp:Literal></a>
            </div>
			<header runat="server" id="Header1" noid="True">
                <time class="published" id="Time1" runat="server"  noid="True"><%# Eval("DisplayDate") %></time>
			    <h3 class="entry-title" id="H1Title" runat="server" noid="True"><a href="<%# RenderLink((string)Eval("HumanReadableUrl"))  %>"><%# Eval("Title") %></a></h3>
			    <h4 class="entry-subtitle" id="H2SubTitle" runat="server" noid="True"><%# Eval("SubTitle") %></h4>
                <time class="published" id="Time2" runat="server" noid="True"><%# Eval("DisplayDate") %></time>
			</header>

            <asp:Repeater runat="server" ID="RepeaterCategories">
                <HeaderTemplate><ul class="tags"></HeaderTemplate>
                <FooterTemplate></ul></FooterTemplate>
                <ItemTemplate>
                    <li><a onclick="return false"><%# Eval("Title") %></a></li>
                </ItemTemplate>
            </asp:Repeater>

			<div class="entry-summary" runat="server" id="SectionTeaser" noid="True">
			    <%# Eval("ProcessedTeaser") %>
			</div>	
			<div class="entry-content" runat="server" id="SectionHtml" noid="True">
				<%# Eval("ProcessedHtml") %>
			</div>
			<div class="read-on" id="DivReadon" runat="server" noid="True">
				<a href="<%# RenderLink((string)Eval("HumanReadableUrl"))  %>" class="more"><%= Translate("article_more") %></a>
			</div>
		</article>
    </ItemTemplate>
</asp:Repeater>		
<asp:Panel CssClass="archive" runat="server" ID="PanelArchive" Visible="false" noid="True">
    <asp:HyperLink runat="server" ID="HyperLinkMore"></asp:HyperLink>
</asp:Panel>
<two:Pager id="PagerArticles" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    