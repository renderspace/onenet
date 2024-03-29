﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.WebControls;
using System.ComponentModel.DataAnnotations;
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleList : MModule, IBasicSEOProvider
    {
        public const string REQUEST_ARTICLE_TEXT_SEARCH = "atsearch";
        public const string REQUEST_DATE = "adfd";
        public const string REQUEST_ARTICLE_REGULAR_ID = "regid";

        private DateTime? requestedMonth;
        private DateTime? requestedYear;
        private string requestedArticleTextSearch = "";

        private static readonly BArticle articleB = new BArticle();

        #region Settings

        [Setting(SettingType.Int, DefaultValue = "0")]
        public int OverrideLanguageId
        {
            get
            {
                return GetIntegerSetting("OverrideLanguageId");
            }
        }

        [Setting(SettingType.CSInteger)]
        public List<int> HiddenTagsList
        {
            get
            {
                return GetIntegerListSetting("HiddenTagsList");
            }
        }

        [Setting(SettingType.CSInteger)]
        public List<int> ExcludeCategoriesList
        {
            get
            {
                return GetIntegerListSetting("ExcludeCategoriesList");
            }
        }

        [Setting(SettingType.CSInteger)]
        public List<int> CategoriesList 
        { 
            get 
            {
                List<int> databaseCategories = GetIntegerListSetting("CategoriesList");
                var result = new List<int>();
                result.AddRange(databaseCategories);
                if (Request[REQUEST_ARTICLE_REGULAR_ID] != null)
                {
                    var categoriesFilter = StringTool.SplitStringToIntegers(Request[REQUEST_ARTICLE_REGULAR_ID]);
                    if (categoriesFilter.Any())
                    {
                        result.Clear();
                    }
                    foreach (var c in categoriesFilter)
                    {
                        if (databaseCategories.Contains(c))
                        {
                            result.Add(c);
                        }
                    }
                }
                else if (result.Count == 0 && HasHumanReadableUrlParameter)
                {
                    var regular = articleB.GetRegular(HumanReadableUrlParameter, OverrideLanguageId);
                    if (regular != null)
                        result.Add(regular.Id.Value);
                }
                return result;
            } 
        }

        [Setting(SettingType.String, DefaultValue = "title", Options = "title,subtitle,teaser,display_date,random")]
        public string SortByColumn { get { return GetStringSetting("SortByColumn"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }

        [Setting(SettingType.Int, DefaultValue = "10")]
        public int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool ShowPager { get { return GetBooleanSetting("ShowPager"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowDate { get { return GetBooleanSetting("ShowDate"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowDateBelowTitle { get { return GetBooleanSetting("ShowDateBelowTitle"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool ShowTitle { get { return GetBooleanSetting("ShowTitle"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowSubTitle { get { return GetBooleanSetting("ShowSubTitle"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowTeaser { get { return GetBooleanSetting("ShowTeaser"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowHtml { get { return GetBooleanSetting("ShowHtml"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowMore { get { return GetBooleanSetting("ShowMore"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowModuleTitle { get { return GetBooleanSetting("ShowModuleTitle"); } }

        [Setting(SettingType.Url)]
        public string SingleArticleUri { get { return GetStringSetting("SingleArticleUri"); } }

        [Setting(SettingType.Url)]
        public string ArticleListUri { get { return GetStringSetting("ArticleListUri"); } }

        [Setting(SettingType.Int, DefaultValue = "0")]
        public int OffSet { get { return GetIntegerSetting("OffSet"); } }

        [Setting(SettingType.String, DefaultValue = "dd.MM.yy")]
        public string DateFormatString { get { return GetStringSetting("DateFormatString"); } }

        [Setting(SettingType.ImageTemplate, DefaultValue="-1")]
        public BOImageTemplate ImageTemplate { get { return GetImageTemplate("ImageTemplate"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowCategories { get { return GetBooleanSetting("ShowCategories"); } }

        #endregion Settings

        public string Description
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string OgImageUrl
        {
            get;
            set;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PagerArticles.Visible = ShowPager;
            PagerArticles.RecordsPerPage = RecordsPerPage;
            PagerArticles.SelectedPage = 1;

            if (!string.IsNullOrWhiteSpace(ArticleListUri))
            {
                PanelArchive.Visible = true;
                HyperLinkMore.NavigateUrl = ArticleListUri;
                HyperLinkMore.Text = Translate("article_list");
            }

            //rid

            if (Request[REQUEST_DATE] != null)
            {
                string reqDate = Request[REQUEST_DATE];
                string[] dateVals = reqDate.Trim().Split('-');
                int reqYear = FormatTool.GetInteger(dateVals[0].Trim());
                int reqMonth = FormatTool.GetInteger(dateVals[1].Trim());
                if (reqYear > -1 && reqMonth > -1)
                    requestedMonth = new DateTime(reqYear, reqMonth, 1);
                else if (reqYear > -1)
                    requestedYear = new DateTime(reqYear, 1, 1);
            }

            // Added June 15 2011 as part of building ArticleDropDownFilter by MP
            if (Request[REQUEST_ARTICLE_TEXT_SEARCH] != null)
            {
                requestedArticleTextSearch = HttpUtility.UrlDecode(Request[REQUEST_ARTICLE_TEXT_SEARCH]);
            }

            if (Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID] != null && ShowPager)
            {
                PagerArticles.SelectedPage = FormatTool.GetInteger(Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID]);
            }

            ListingState listingState =
                    new ListingState(RecordsPerPage, PagerArticles.FirstRecordIndex, SortDescending ? SortDir.Descending : SortDir.Ascending,
                    SortByColumn);
            listingState.OffSet = OffSet;

            var articles = articleB.ListArticles(CategoriesList, listingState, requestedArticleTextSearch, requestedMonth, requestedYear, ExcludeCategoriesList, OverrideLanguageId);

            foreach (BOArticle article in articles)
            {
                var websiteUri = PublishFlag ? CurrentWebsite.ProductionUrl : CurrentWebsite.PreviewUrl;
                article.Permalink = websiteUri.TrimEnd('/')  + SingleArticleUri + "/" + article.HumanReadableUrl;
            }

            RepeaterArticles.DataSource = articles;
            RepeaterArticles.DataBind();

            if (ShowPager)
            {
                PagerArticles.TotalRecords = articles.AllRecords;
                PagerArticles.DetermineData();
            }

            if (ShowModuleTitle && articles.AllRecords > 0)
            {
                H2ModuleTitle.Visible = true;
                H2ModuleTitle.InnerHtml = Translate("article_list_title");
            }
        }

        protected string RenderLink(string humanReadableUrl)
        {
            return SingleArticleUri + "/" + humanReadableUrl;
        }

        protected void RepeaterArticles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                var article = (BOArticle) e.Item.DataItem;
                var SectionTeaser = e.Item.FindControl("SectionTeaser") as HtmlGenericControl;
                var Header1 = e.Item.FindControl("Header1") as HtmlGenericControl;
                var SectionHtml = e.Item.FindControl("SectionHtml") as HtmlGenericControl;
                var H1Title = e.Item.FindControl("H1Title") as HtmlGenericControl;
                var H1NoLinkTitle = e.Item.FindControl("H1NoLinkTitle") as HtmlGenericControl;
                var H2SubTitle = e.Item.FindControl("H2SubTitle") as HtmlGenericControl;
                var DivReadon = e.Item.FindControl("DivReadon") as HtmlGenericControl;

                var Time1 = e.Item.FindControl("Time1") as HtmlGenericControl;
                var Time2 = e.Item.FindControl("Time2") as HtmlGenericControl;
                var HtmlArticle = e.Item.FindControl("HtmlArticle") as HtmlGenericControl;

                var RepeaterCategories = e.Item.FindControl("RepeaterCategories") as Repeater;

                var id = article.Id.Value;
                var TeaserImage1 = e.Item.FindControl("TeaserImage1") as HtmlGenericControl;
                if (article.TeaserImageId > 0 && TeaserImage1 != null && ImageTemplate != null)
                {
                    TeaserImage1.Visible = true;
                    var LiteralTeaserImage = e.Item.FindControl("LiteralTeaserImage") as Literal;
                    LiteralTeaserImage.Text = ImageTemplate.RenderHtml(article.Images[0].Alt, article.Images[0].FullUri, "tii1");
                }

                if (H1Title != null)
                    H1Title.Visible = ShowTitle;
                if (H1NoLinkTitle != null)
                    H1NoLinkTitle.Visible = ShowTitle;

                if (H2SubTitle != null)
                    H2SubTitle.Visible = ShowSubTitle;
                if (Header1 != null)
                    Header1.Visible = ShowTitle || ShowSubTitle;
                if (SectionTeaser != null)
                    SectionTeaser.Visible = ShowTeaser;
                if (SectionHtml != null)
                    SectionHtml.Visible = ShowHtml;
                if (DivReadon != null)
                    DivReadon.Visible = ShowMore && article.HasHtml;
                if (Time1 != null)
                {
                    Time1.Visible = ShowDate;
                    Time1.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                    Time1.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                    Time1.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                }
                if (Time2 != null)
                {
                    Time2.Visible = ShowDateBelowTitle;
                    Time2.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                    Time2.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                    Time2.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                }

                if (HtmlArticle.Attributes["class"] == null)
                    HtmlArticle.Attributes.Add("class", "hentry a" + id.ToString() + " " + MModule.RenderOrder(id));
                else
                    HtmlArticle.Attributes["class"] += " hentry a" + id.ToString() + " " + MModule.RenderOrder(id);

                if (RepeaterCategories != null && ShowCategories)
                {
                    var regulars = article.Regulars;
                    if (HiddenTagsList != null && HiddenTagsList.Count > 0)
                    {
                        regulars.RemoveAll(r => HiddenTagsList.Contains(r.Id.Value));
                    }

                    RepeaterCategories.DataSource = regulars.OrderBy(r => r.Id);
                    RepeaterCategories.DataBind();
                }
            }
        }
    }
}