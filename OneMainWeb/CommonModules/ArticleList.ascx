<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleList.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleList" EnableViewState="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<asp:Repeater runat="server" ID="RepeaterArticles" OnItemDataBound="RepeaterArticles_ItemDataBound" >
    <ItemTemplate>
		<article class="hentry odd first a1">
			<header runat="server" id="Header1">
				<hgroup>
					 <h1 class="entry-title" id="H1Title" runat="server"><%# Eval("Title") %></h1>
					 <h2 class="entry-subtitle" id="H2SubTitle" runat="server"><%# Eval("SubTitle") %></h2>
				</hgroup>
			</header>
			<footer class="metadata">
				<time class="published" datetime="2011-11-11" pubdate=""><%# Eval("DisplayDate") %></time>
			</footer>
			<section class="entry-summary" runat="server" id="SectionTeaser">
			    <%# Eval("Teaser") %>
			</section>	
			<section class="entry-content" runat="server" id="SectionHtml">
				<%# Eval("Html") %>
			</section>
			<div class="readon" id="DivReadon" runat="server">
				<a href="" title="" class="more">more &raquo; </a>
			</div>
		</article>
    </ItemTemplate>
</asp:Repeater>		
	
<div class="archive">
	<a href="" title="" class="more">Archive &raquo; </a>
</div>

<two:Pager id="PagerArticles" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    