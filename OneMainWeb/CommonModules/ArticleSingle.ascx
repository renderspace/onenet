﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleSingle.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleSingle" EnableViewState="false" %>
<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
    <asp:View ID="View1" runat="server">
        <%= TranslateComplex("article_single_no_article") %>
    </asp:View>
    <asp:View ID="View2" runat="server">
        <article id="HtmlArticle" runat="server" noid="True">
	        <header runat="server" id="Header1" noid="True">
                <time class="published" id="Time1" runat="server"  noid="True"></time>
		        <h1 class="entry-title" id="H1Title" runat="server" noid="True"></h1>
		        <h4 class="entry-subtitle" id="H2SubTitle" runat="server" noid="True"></h4>
                <time class="published" id="Time2" runat="server" noid="True"></time>
	        </header>
            <asp:Repeater runat="server" ID="RepeaterCategories">
                <HeaderTemplate><ul class="tags"></HeaderTemplate>
                <FooterTemplate></ul></FooterTemplate>
                <ItemTemplate>
                    <li><a onclick="return false"><%# Eval("Title") %></a></li>
                </ItemTemplate>
            </asp:Repeater>
	        <div class="entry-summary" runat="server" id="SectionTeaser" noid="True"></div>	
            <div class="entry-teaser-image" runat="server" id="DivTeaserImage" noid="True"></div>	
	        <div class="entry-content" runat="server" id="SectionHtml" noid="True">
		        <%# Eval("Html") %>
	        </div>
	        <div class="read-on" id="DivReadon" runat="server" noid="True">
		        <a href="<%= ArticleListUri %>" class="more"><%= Translate("article_list") %></a>
	        </div>
        </article>
    </asp:View>
</asp:MultiView>
