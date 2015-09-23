using System;
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

        private DateTime? requestedMonth;
        private DateTime? requestedYear;
        private string requestedArticleTextSearch = "";

        private static readonly BArticle articleB = new BArticle();

        #region Settings

        [Setting(SettingType.CSInteger)]
        public List<int> CategoriesList 
        { 
            get 
            {
                List<int> result = GetIntegerListSetting("CategoriesList");

                if (result.Count == 0 && HasHumanReadableUrlParameter)
                {
                    var regular = articleB.GetRegular(HumanReadableUrlParameter);
                    if (regular != null)
                        result.Add(regular.Id.Value);
                }

                return result;
            } 
        }

        [Setting(SettingType.String, DefaultValue = "title", Options = "title,subtitle,teaser,display_date")]
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

            if (ShowModuleTitle)
            {
                H2ModuleTitle.Visible = true;
                H2ModuleTitle.InnerHtml = Translate("article_list_title");
            }

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
            if (Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID] != null)
            {
                PagerArticles.SelectedPage = FormatTool.GetInteger(Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID]);
            }
            ListingState listingState =
                    new ListingState(RecordsPerPage, PagerArticles.FirstRecordIndex, SortDescending ? SortDir.Descending : SortDir.Ascending, 
                    SortByColumn);
                listingState.OffSet = OffSet; 

           var articles = articleB.ListArticles(CategoriesList, false, listingState, requestedArticleTextSearch, requestedMonth, requestedYear);

           RepeaterArticles.DataSource = articles;
           RepeaterArticles.DataBind();

           PagerArticles.TotalRecords = articles.AllRecords;
           PagerArticles.DetermineData();
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
                var H2SubTitle = e.Item.FindControl("H2SubTitle") as HtmlGenericControl;
                var DivReadon = e.Item.FindControl("DivReadon") as HtmlGenericControl;

                var Time1 = e.Item.FindControl("Time1") as HtmlGenericControl;
                var Time2 = e.Item.FindControl("Time2") as HtmlGenericControl;
                var HtmlArticle = e.Item.FindControl("HtmlArticle") as HtmlGenericControl;

                var id = article.Id.Value;
                var TeaserImage1 = e.Item.FindControl("TeaserImage1") as HtmlGenericControl;
                if (article.TeaserImageId > 0 && TeaserImage1 != null && ImageTemplate != null)
                {
                    TeaserImage1.Visible = true;
                    var LiteralTeaserImage = e.Item.FindControl("LiteralTeaserImage") as Literal;
                    LiteralTeaserImage.Text = ImageTemplate.RenderHtml("", article.Images[0].FullUri, "");
                }

                if (H1Title != null)
                    H1Title.Visible = ShowTitle;

                if (H2SubTitle != null)
                    H2SubTitle.Visible = ShowSubTitle;
                if (Header1 != null)
                    Header1.Visible = ShowTitle || ShowSubTitle;
                if (SectionTeaser != null)
                    SectionTeaser.Visible = ShowTeaser;
                if (SectionHtml != null)
                    SectionHtml.Visible = ShowHtml;
                if (DivReadon != null)
                    DivReadon.Visible = ShowMore;
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
            }
        }
    }
}