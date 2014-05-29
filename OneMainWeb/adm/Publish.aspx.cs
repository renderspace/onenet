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
                tabMultiview.Views[0].Selectable = true;
                tabMultiview.Views[1].Selectable = true;
                tabMultiview.Views[2].Selectable = true;
                
                tabMultiview.SetActiveIndex(0);
            }
        }

        protected void tabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                articleGridView.DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                pageGridView.DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 2)
            {
                articleGridView.DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 3)
            {
                publisherGridview.DataBind();
            }
        }

        protected void articleGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Schedule":
                    var ei = Int32.Parse(e.CommandArgument.ToString());
                    articleGridView.EditIndex = ei;
                    break;
                case "Save":
                    var arg = Int32.Parse(e.CommandArgument.ToString());
                    break;
                case "Cancel":
                    articleGridView.EditIndex = -1;
                    break;
            }
        }







        protected void pageGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string command = e.CommandName;
            switch (command)
            {
                case "Schedule":
                    int ei = Int32.Parse(e.CommandArgument.ToString());
                    pageGridView.EditIndex = ei;
                    break;
                case "Save":
                    break;
                case "Cancel":
                    pageGridView.EditIndex = -1;
                    break;
            }
        }

        protected void ObjectDataSourceArticleList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void ObjectDataSourceCommentList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void ObjectDataSourceFaqList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void ObjectDataSourceEventList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void ObjectDataSourcePublisherList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["published"] = chkPublished.Checked;

            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void ObjectDataSourcePageList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["webSiteId"] = SelectedWebSiteId;

            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void PublishArticlesButton_Click(object sender, EventArgs e)
        {
            int publishCount = 0;
            int failedCount = 0;

            foreach (GridViewRow row in articleGridView.Rows)
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
                Notifier1.Message = string.Format(ResourceManager.GetString("$successfully_published_n_articles"), publishCount);
                articleGridView.DataBind();
            }

            if (failedCount > 0)
            {
                // note, resource file has to contain "Nije objavio {0} clanaka"
                Notifier1.Warning = string.Format(ResourceManager.GetString("$failed_to_publish_n_articles"), failedCount);
            }           

            if ( publishCount == 0 && failedCount == 0)
            {
                Notifier1.Warning = ResourceManager.GetString("$nothing_to_publish");
            }
        }

        protected void PublishPagesButton_Click(object sender, EventArgs e)
        {
            int publishedCount = 0;
            int failedCount = 0;

            foreach (GridViewRow row in pageGridView.Rows)
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
                Notifier1.Message = string.Format(ResourceManager.GetString("$successfully_published_n_pages"), publishedCount);
                pageGridView.DataBind();
            }
            
            if ( failedCount > 0 )
            {
                // note, resource file has to contain "Nije objavio {0} stranice"
                Notifier1.Warning = string.Format(ResourceManager.GetString("$failed_to_publish_n_pages"), failedCount);
            }

            if (failedCount == 0 && publishedCount == 0)
            {
                Notifier1.Warning = ResourceManager.GetString("$nothing_to_publish");
            }
        }

        protected void cmdDisplay_Click(object sender, EventArgs e)
        {
            publisherGridview.DataBind();
        }
    }

    


    [Serializable]
    public class UnpublishedArticleDataSource
    {
        private static readonly BArticle articleB = new BArticle();

        public int SelectArticleCount()
        {
            return (int)HttpContext.Current.Items["articleRowCount"];
        }

        public PagedList<BOArticle> SelectArticles(int recordsPerPage, int firstRecordIndex, string sortBy)
        {
            PagedList<BOArticle> articles = articleB.ListUnpublishedArticles(new ListingState(recordsPerPage, firstRecordIndex, (sortBy.Contains("ASC") || !sortBy.Contains("DESC") ? SortDir.Ascending : SortDir.Descending), sortBy.Replace("DESC", "").Replace("ASC", "")));
            HttpContext.Current.Items["articleRowCount"] = articles.AllRecords;
            return articles;
        }
    }

    [Serializable]
    public class UnpublishedPageDataSource
    {
        private static readonly BWebsite webSiteB = new BWebsite();

        public int SelectPageCount()
        {
            return (int)HttpContext.Current.Items["pageRowCount"];
        }

        public PagedList<BOPage> SelectPages(int webSiteId, int recordsPerPage, int firstRecordIndex, string sortBy)
        {
            PagedList<BOPage> pages = webSiteB.ListUnpublishedPages(webSiteId, new ListingState(recordsPerPage, firstRecordIndex, (sortBy.Contains("ASC") || !sortBy.Contains("DESC") ? SortDir.Ascending : SortDir.Descending), sortBy.Replace("DESC", "").Replace("ASC", "")));
            HttpContext.Current.Items["pageRowCount"] = pages.AllRecords;
            return pages;
        }
    }
}
