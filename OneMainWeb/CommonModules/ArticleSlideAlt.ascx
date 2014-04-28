<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleSlide.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleSlide" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Import namespace="One.Net.BLL"%>


<script type="text/javascript">
    //<![CDATA[
  
    $(document).ready(function() {

        $('.rscarousel_hor').rscarousel({ firstItemIndex:0, moveCount: 1, autoscroll: 1, canAnimate: true, animation: true, speed: 1000, scrollDirection: 'width', circular: true, autoscroll: 6500 });

    });
    //]]>
</script>

<script runat="server">
    
    protected string RenderArticleUri(int id)
    {
        UrlBuilder builder = new UrlBuilder(Page);
        builder.QueryString.Clear();
        builder.Path = SingleArticleUri;
        builder.QueryString["aid"] = id.ToString();
        return builder.ToString();
    }

    protected string RenderArticleImage(BOArticle article)
    {
        if (ShowTeaserImage)
        {
            if (article.TeaserImageId > 0)
            {
                BFileSystem fileSystem = new BFileSystem();
                BOFile file = fileSystem.Get(article.TeaserImageId);
                if (file != null)
                {
                    return "<img src='" + "/_files/" + file.Id + "/" + file.Name + "?w=" + ThumbnailWidth + "' alt='" + article.Title + "' />";
                }
            }
            return "<img src='" + DefaultImageUri + "' alt='" + article.Title + "' />";
        }
        return "";           
    }
    
</script>

<h1><%=Translate("article_slide_module_title") %></h1>
<asp:Panel ID="PanelArticles" runat="server">
    <div class="rscarousel_hor">
        <div class="rscarousel_main">
            <a href="#back" class="nav prev"></a>
            <div class="list_holder">
                <asp:Repeater runat="server" ID="RepeaterArticles">
                    <HeaderTemplate><ul></HeaderTemplate>
                    <ItemTemplate>
                        <li id='li<%# Container.ItemIndex %>'>
							<div class="inner">
								<h2><a href='<%# RenderArticleUri((int)Eval("Id")) %>'><%# Eval("Title") %></a></h2>
								<%# (ShowTeaser ? "<div class=\"image\">" + Eval("ProcessedTeaser") + "</div>" : string.Empty)%> 
								<%# (ShowSubTitle ? "<div class=\"desc\">" + Eval("SubTitle") + "</div>" : string.Empty)%>                                   
								<%# (ShowMoreLink ? BuildSingleArticleLink((int?)Eval("Id")) : "") %>    
							</div>
						</li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>
            </div>
            <a href="#fwd" class="nav next"></a>
        </div>
    </div>
</asp:Panel>
<p class="allArticlesLink"><a href='<%=ArticleListUri %>'><%=Translate("lumpi_article_slide_all_articles") %></a></p>
