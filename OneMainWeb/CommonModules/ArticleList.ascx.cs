using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;

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

           RepeaterArticles.DataSource = articles;
           RepeaterArticles.DataBind();
        }
    }
}