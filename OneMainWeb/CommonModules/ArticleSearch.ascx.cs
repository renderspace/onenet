using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleSearch : MModule
    {
        protected string RegularsList { get { return GetStringSetting("RegularsList"); } }
        protected string SingleArticleUri { get { return GetStringSetting("SingleArticleUri"); } }
        protected string SortByColumn { get { return GetStringSetting("SortByColumn"); } }
        protected int LimitNoArticles { get { return GetIntegerSetting("LimitNoArticles"); } }
        protected bool ShowPager { get { return GetBooleanSetting("ShowPager"); } }
        protected bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }

        private static readonly BArticle articleB = new BArticle();

        protected void Page_Load(object sender, EventArgs e)
        {
            LabelKeywords.Text = Translate("articlesearch_keywords");
            LinkButtonSearch.Text = Translate("articlesearch_button");
            TwoPager1.Visible = ShowPager;
            TwoPager1.RecordsPerPage = LimitNoArticles;
            TwoPager1.SelectedPage = 1;
            DateEntryBeginDate.Text = Translate("as_begin_date");
            DateEntryEndDate.Text = Translate("as_end_date");
            if (!IsPostBack)
            {
                LabelZeroResults.Text = Translate("articlesearch_zero_results");
            }
        }

        protected void RepeaterArticleList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Panel ArticleList = e.Item.FindControl("ListItem") as Panel;
                if (ArticleList != null)
                    ArticleList.CssClass += " " + RenderOrder(e.Item.ItemIndex);
            }
        }

        protected void LinkButtonSearch_Click(object sender, EventArgs e)
        {
            ListingState listingState =
                    new ListingState(LimitNoArticles, TwoPager1.FirstRecordIndex, SortDescending ? SortDir.Descending : SortDir.Ascending,
                    SortByColumn);

            PagedList<BOArticle> articles = articleB.ListArticles(RegularsList,DateEntryBeginDate.SelectedDate, DateEntryEndDate.SelectedDate, listingState, TextBoxKeywords.Text);

            if (articles.Count > 0)
            {
                LabelNoResults.Visible = true;
                LabelZeroResults.Visible = false;
                LabelNoResults.Text = Translate("articlesearch_no_results") + articles.Count;
            }
            else
            {
                LabelNoResults.Visible = false;
                LabelZeroResults.Visible = true;
            }

            RepeaterArticleList.DataSource = articles;
            RepeaterArticleList.DataBind();
            TwoPager1.TotalRecords = articles.AllRecords;
            TwoPager1.DetermineData();
        }
    }
}