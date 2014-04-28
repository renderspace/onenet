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

namespace OneMainWeb.CommonModules
{
    public partial class ArticleList : MModule
    {
        public const string REQUEST_ARTICLE_TEXT_SEARCH = "atsearch";
        public const string REQUEST_DATE = "adfd";

        private DateTime? requestedMonth;
        private DateTime? requestedYear;
        private string requestedArticleTextSearch = "";

        private static readonly BArticle articleB = new BArticle();

        #region Settings
        protected string RegularsList { get { return GetStringSetting("RegularsList"); } }
        protected string SortByColumn { get { return GetStringSetting("SortByColumn"); } }
        protected bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }
        protected int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }
        protected bool ShowPager { get { return GetBooleanSetting("ShowPager"); } }
        protected bool ShowDate { get { return GetBooleanSetting("ShowDate"); } }
        protected bool ShowTitle { get { return GetBooleanSetting("ShowTitle"); } }
        protected bool ShowSubTitle { get { return GetBooleanSetting("ShowSubTitle"); } }
        protected bool ShowTeaser { get { return GetBooleanSetting("ShowTeaser"); } }
        protected bool ShowHtml { get { return GetBooleanSetting("ShowHtml"); } }
        protected string SingleArticleUri { get { return GetStringSetting("SingleArticleUri"); } }
        protected int OffSet { get { return GetIntegerSetting("OffSet"); } }


        #endregion Settings

        protected void Page_Load(object sender, EventArgs e)
        {
            PagerArticles.Visible = ShowPager;
            PagerArticles.RecordsPerPage = RecordsPerPage;
            PagerArticles.SelectedPage = 1;

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

            
            ListingState listingState =
                    new ListingState(RecordsPerPage, PagerArticles.FirstRecordIndex, SortDescending ? SortDir.Descending : SortDir.Ascending, 
                    SortByColumn);
                listingState.OffSet = OffSet; 

           var regulars = RegularsList;

           var articles = articleB.ListArticles(regulars, false, listingState, requestedArticleTextSearch, requestedMonth, requestedYear);

           if (Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID] != null)
           {
               PagerArticles.SelectedPage = FormatTool.GetInteger(Request[Pager.REQUEST_PAGE_ID + PagerArticles.ID]);
           }

           RepeaterArticles.DataSource = articles;
           RepeaterArticles.DataBind();

           PagerArticles.TotalRecords = articles.AllRecords;
           PagerArticles.DetermineData();
        }

        protected void RepeaterArticles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                var SectionTeaser = e.Item.FindControl("SectionTeaser") as HtmlGenericControl;
                var Header1 = e.Item.FindControl("Header1") as HtmlGenericControl;
                var SectionHtml = e.Item.FindControl("SectionHtml") as HtmlGenericControl;
                var H1Title = e.Item.FindControl("H1Title") as HtmlGenericControl;
                var H2SubTitle = e.Item.FindControl("H2SubTitle") as HtmlGenericControl;
                var DivReadon = e.Item.FindControl("DivReadon") as HtmlGenericControl;

                H1Title.Visible = ShowTitle;
                H2SubTitle.Visible = ShowSubTitle;
                Header1.Visible = ShowTitle || ShowSubTitle;
                SectionTeaser.Visible = ShowTeaser;
                SectionHtml.Visible = ShowHtml;
                //DivReadon.Visible = Show
            }
        }
    }
}