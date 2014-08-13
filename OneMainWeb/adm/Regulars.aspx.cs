using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.adm
{
    public partial class Regulars : OneBasePage
    {
        private static readonly BArticle articleB = new BArticle();

        protected BORegular SelectedRegular
        {
            get { return ViewState["SelectedRegular"] as BORegular; }
            set { ViewState["SelectedRegular"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GridViewSortExpression = "id";
                GridViewSortDirection = SortDir.Ascending;
                RegularDataBind();
            }
        }

        protected void RegularDataBind()
        {
            ListingState state = new ListingState();
            state.SortDirection = GridViewSortDirection;
            state.SortField = GridViewSortExpression;

            GridViewRegular.DataSource = articleB.ListRegulars(state, ShowUntranslated, null, null);
            GridViewRegular.DataBind();
        }

        protected void Multiview1_ActiveViewChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 1)
            {
                TxtRegularContent.UseCkEditor = true;

                if (SelectedRegular != null)
                {
                    TxtRegularContent.Title = SelectedRegular.Title;
                    TxtRegularContent.SubTitle = SelectedRegular.SubTitle;
                    TxtRegularContent.Teaser = SelectedRegular.Teaser;
                    TxtRegularContent.Html = SelectedRegular.Html;
                }
            }
        }

        protected void cmdAddRegular_Click(object sender, EventArgs e)
        {
            BORegular regular = new BORegular();

            regular.Title = TextBoxRegular.Text;
            regular.SubTitle = regular.Teaser = regular.Html = "";
            regular.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            regular.ContentId = null;
            articleB.ChangeRegular(regular);
            RegularDataBind();
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in GridViewRegular.Rows)
            {
                CheckBox chkForPublish = row.FindControl("chkFor") as CheckBox;
                Literal litArticleId = row.FindControl("litId") as Literal;

                if (litArticleId != null && chkForPublish != null && chkForPublish.Checked)
                {
                    int articleId = FormatTool.GetInteger(litArticleId.Text);
                    if (articleId > 0)
                    {
                        result.Add(articleId);
                    }
                }
            }
            return result;
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            int deletedCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                if (articleB.DeleteRegular(i))
                {
                    deletedCount++;
                }
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} categories", deletedCount);
                RegularDataBind();
            }
            if (deletedCount != list.Count())
            {
                Notifier1.ExceptionName = "Some of the regulars weren't deleted. Check if they still contain articles";
            }
        }

        protected void GridViewRegular_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedRegular = articleB.GetRegular(Int32.Parse(grid.SelectedValue.ToString()));
                Multiview1.ActiveViewIndex = 1;
            }
        }

        private void SaveRegular(bool close)
        {
            try
            {
                SelectedRegular.Html = TxtRegularContent.Html;
                SelectedRegular.Title = TxtRegularContent.Title;
                SelectedRegular.Teaser = TxtRegularContent.Teaser;
                SelectedRegular.SubTitle = TxtRegularContent.SubTitle;
                SelectedRegular.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;

                articleB.ChangeRegular(SelectedRegular);

                Notifier1.Message = "Saved";

                if (close)
                {
                    SelectedRegular = null;
                    Multiview1.ActiveViewIndex = 0;
                }
                else
                {
                    Multiview1.ActiveViewIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "Error saving";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
            GridViewRegular.DataBind();
        }

        protected void RegularCancelButton_Click(object sender, EventArgs e)
        {
            Multiview1.ActiveViewIndex = 0;
        }


        protected void RegularInsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveRegular(false);
        }

        protected void RegularInsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveRegular(true);
        }

        protected void GridViewRegular_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            RegularDataBind();
        } 
    }
}