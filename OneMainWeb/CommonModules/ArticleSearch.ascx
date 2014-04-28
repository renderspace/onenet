<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleSearch.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleSearch" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<h1><%= Translate("articlesearch_module_title") %></h1>

<asp:Label CssClass="keywords" AssociatedControlID="TextBoxKeywords" ID="LabelKeywords" runat="server" EnableViewState="false" />

<asp:TextBox ID="TextBoxKeywords" runat="server"></asp:TextBox>

<p class="fromTo"><%= Translate("articlesearch_fromto") %></p>
<two:DateEntry ID="DateEntryBeginDate" runat="server" Text="$begin_date_full" Required="false" ShowCalendar="true" />
<two:DateEntry ID="DateEntryEndDate" runat="server" Text="$begin_end_full" Required="false" ShowCalendar="true" />							        
<p class="info"><%= Translate("articlesearch_info") %></p>
<asp:LinkButton CssClass="searchButton" ID="LinkButtonSearch" runat="server" onclick="LinkButtonSearch_Click" EnableViewState="false" />
<p class="info2"><%= Translate("articlesearch_info2") %></p>

<asp:Label runat="server" CssClass="noResults" Visible="false" ID="LabelNoResults" />
<asp:Label runat="server" CssClass="zeroResults" Visible="false" ID="LabelZeroResults" />

<asp:Repeater ID="RepeaterArticleList" runat="server" OnItemDataBound="RepeaterArticleList_ItemDataBound">
    <ItemTemplate>
        <asp:Panel ID="ListItem" runat="server" CssClass="ListItem">
             <div class="Date"><%# ((DateTime)Eval("DisplayDate")).ToShortDateString() %></div>
            <%# "<div class=\"ATitle\">" + "<a href=\"" + SingleArticleUri + "?" + OneMainWeb.CommonModules.Article.REQUEST_ARTICLE_ID + "=" + Eval("Id") + "\">" + Eval("Title") + "</a></div>" %>
            <div class="ASubTitle"><%# Eval("SubTitle") %></div>
            <div class="ATeaser"><%# Eval("Teaser") %></div>
        </asp:Panel>
    </ItemTemplate>
</asp:Repeater>

<two:Pager id="TwoPager1" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    
