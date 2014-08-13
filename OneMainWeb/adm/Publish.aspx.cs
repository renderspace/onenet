using System;
using System.Web;
using System.Web.UI.WebControls;
using One.Net.BLL;
using System.Threading;
using One.Net.BLL.WebControls;


namespace OneMainWeb
{
    public partial class Publish : OneBasePage
    {
        private static readonly BArticle articleB = new BArticle();
        private static readonly BWebsite webSiteB = new BWebsite();

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;

            if (!IsPostBack)
            {
                Multiview1.ActiveViewIndex = 0;
            }
        }

        protected void tabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;
                GridViewSortExpression = "";
                Articles_DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                TwoPostbackPager2.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager2.SelectedPage = 1;
                GridViewSortExpression = "";
                Pages_DataBind();
            }
        }

        private void Articles_DataBind()
        {
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;
            PagedList<BOArticle> articles = articleB.ListUnpublishedArticles(state);
            TwoPostbackPager1.TotalRecords = articles.AllRecords;
            TwoPostbackPager1.DetermineData();
            GridViewArticles.DataSource = articles;
            GridViewArticles.DataBind();
        }

        private void Pages_DataBind()
        {
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager2.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;
            PagedList<BOPage> pages = webSiteB.ListUnpublishedPages(SelectedWebSiteId, state);
            TwoPostbackPager2.TotalRecords = pages.AllRecords;
            TwoPostbackPager2.DetermineData();
            GridViewPages.DataSource = pages;
            GridViewPages.DataBind();
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Articles_DataBind();
        }

        public void TwoPostbackPager2_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager2.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Pages_DataBind();
        }

        protected void GridViewArticles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Schedule":
                    var ei = Int32.Parse(e.CommandArgument.ToString());
                    GridViewArticles.EditIndex = ei;
                    break;
                case "Save":
                    var arg = Int32.Parse(e.CommandArgument.ToString());
                    break;
                case "Cancel":
                    GridViewArticles.EditIndex = -1;
                    break;
            }
        }

        protected void GridViewPages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string command = e.CommandName;
            switch (command)
            {
                case "Schedule":
                    int ei = Int32.Parse(e.CommandArgument.ToString());
                    GridViewPages.EditIndex = ei;
                    break;
                case "Save":
                    break;
                case "Cancel":
                    GridViewPages.EditIndex = -1;
                    break;
            }
        }

        protected void PublishArticlesButton_Click(object sender, EventArgs e)
        {
            int publishCount = 0;
            int failedCount = 0;

            foreach (GridViewRow row in GridViewArticles.Rows)
            {
                CheckBox chkForPublish = row.FindControl("chkForPublish") as CheckBox;
                Literal litArticleId = row.FindControl("litArticleId") as Literal;

                if (litArticleId != null && chkForPublish != null && chkForPublish.Checked )
                {
                    int articleId = FormatTool.GetInteger(litArticleId.Text);
                    if ( articleId > -1 )
                    {
                        if (articleB.Publish(articleId))
                            publishCount++;
                        else
                            failedCount++;
                    }
                }
            }

            if (publishCount > 0)
            {
                // note, resource file has to contain "Uspjesno objavio {0} clanaka"
                Notifier1.Message = string.Format("$successfully_published_n_articles", publishCount);
                GridViewArticles.DataBind();
            }

            if (failedCount > 0)
            {
                // note, resource file has to contain "Nije objavio {0} clanaka"
                Notifier1.Warning = string.Format("$failed_to_publish_n_articles", failedCount);
            }           

            if ( publishCount == 0 && failedCount == 0)
            {
                Notifier1.Warning = "$nothing_to_publish";
            }
        }

        protected void PublishPagesButton_Click(object sender, EventArgs e)
        {
            int publishedCount = 0;
            int failedCount = 0;

            foreach (GridViewRow row in GridViewPages.Rows)
            {
                CheckBox chkForPublish = row.FindControl("chkForPublish") as CheckBox;
                Literal litPageId = row.FindControl("litPageId") as Literal;

                if (litPageId != null && chkForPublish != null && chkForPublish.Checked)
                {
                    int pageId = FormatTool.GetInteger(litPageId.Text);
                    if (pageId > -1)
                    {
                        if (webSiteB.PublishPage(pageId))
                            publishedCount++;
                        else
                            failedCount++;
                    }
                }
            }

            if (publishedCount > 0)
            {
                // note, resource file has to contain "Uspjesno objavio {0} stranica"
                Notifier1.Message = string.Format("$successfully_published_n_pages", publishedCount);
                GridViewPages.DataBind();
            }
            
            if ( failedCount > 0 )
            {
                // note, resource file has to contain "Nije objavio {0} stranice"
                Notifier1.Warning = string.Format("Failed_to_publish_n_pages", failedCount);
            }

            if (failedCount == 0 && publishedCount == 0)
            {
                Notifier1.Warning = "Nothing to publish";
            }
        }

        protected void LinkButtonArticles_Click(object sender, EventArgs e)
        {
            Multiview1.ActiveViewIndex = 0;
        }

        protected void LinkButtonPages_Click(object sender, EventArgs e)
        {
            Multiview1.ActiveViewIndex = 1;
        }

        protected void GridViewArticles_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            Articles_DataBind();
        }

        protected void GridViewPages_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            Pages_DataBind();
        }
    }
}
