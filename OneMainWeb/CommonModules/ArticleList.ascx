﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleList.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleList" EnableViewState="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<h2 runat="server" id="H2ModuleTitle" noid="True" visible="false"></h2>
<asp:Repeater runat="server" ID="RepeaterArticles" OnItemDataBound="RepeaterArticles_ItemDataBound">
    <ItemTemplate>
		<article id="HtmlArticle" runat="server" noid="True">
			<header runat="server" id="Header1" noid="True">
                <time class="published" id="Time1" runat="server"  noid="True"><%# Eval("DisplayDate") %></time>
			    <h3 class="entry-title" id="H1Title" runat="server" noid="True"><a href="<%# RenderLink(Eval("Id"))  %>"><%# Eval("Title") %></a></h3>
			    <h4 class="entry-subtitle" id="H2SubTitle" runat="server" noid="True"><%# Eval("SubTitle") %></h4>
                <time class="published" id="Time2" runat="server" noid="True"><%# Eval("DisplayDate") %></time>
			</header>
			<div class="entry-summary" runat="server" id="SectionTeaser" noid="True">
			    <%# Eval("Teaser") %>
			</div>	
			<div class="entry-content" runat="server" id="SectionHtml" noid="True">
				<%# Eval("Html") %>
			</div>
			<div class="read-on" id="DivReadon" runat="server" noid="True">
				<a href="<%# RenderLink(Eval("Id"))  %>" class="more"><%= Translate("article_more") %></a>
			</div>
		</article>
    </ItemTemplate>
</asp:Repeater>		
<asp:Panel CssClass="archive" runat="server" ID="PanelArchive" Visible="false" noid="True">
    <asp:HyperLink runat="server" ID="HyperLinkMore"></asp:HyperLink>
</asp:Panel>
<two:Pager id="PagerArticles" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    