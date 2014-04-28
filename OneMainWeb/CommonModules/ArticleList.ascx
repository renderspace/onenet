<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleList.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleList" EnableViewState="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>

<asp:Repeater runat="server" ID="RepeaterArticles">
    <ItemTemplate>
		<article class="hentry odd first a1">
			<header>
				<hgroup>
					 <h1 class="entry-title"><%# Eval("Title") %></h1>
					 <h2 class="entry-subtitle"><%# Eval("SubTitle") %></h2>
				</hgroup>
			</header>
			<footer class="metadata">
				<time class="published" datetime="2011-11-11" pubdate="">11.11.2011</time>
			</footer>
			<section class="entry-summary" runat="server" id="SectionTeaser">
			    <%# Eval("Teaser") %>
			</section>	
			<section class="entry-content" runat="server" id="SectionHtml">
				<%# Eval("Html") %>
			</section>
			<div class="readon">
				<a href="" title="" class="more">more &raquo; </a>
			</div>
		</article>
    </ItemTemplate>
</asp:Repeater>		
	
<div class="archive">
	<a href="" title="" class="more">Archive &raquo; </a>
</div>

<two:Pager id="PagerArticles" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    
		
<section class="pagination">
	<ul>
		<li class="first"><span>&laquo;</span></li>	
		<li class="previous"><span>&lsaquo;</span></li>						
		<li><a href="" title="">1</a></li>
		<li class="current"><em>2</em></li>
		<li><a href="" title="">3</a></li>
		<li><a href="" title="">4</a></li>		
		<li><a href="" title="">5</a></li>		
		<li class="next"><a href="" title="">&rsaquo;</a></li>	
		<li class="last"><a href="" title="">&raquo;</a></li>							
	</ul>	
</section>