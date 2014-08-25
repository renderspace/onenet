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
                TwoPostbackPager2.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager2.SelectedPage = 1;
                GridViewSortExpression = "";
                Pages_DataBind();
            }
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

        public void TwoPostbackPager2_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager2.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Pages_DataBind();
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

        protected void GridViewPages_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            Pages_DataBind();
        }
    }
}
