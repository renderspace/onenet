using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Threading;
using One.Net.BLL.Web;
using OneMainWeb.AdminControls;
using One.Net.BLL;
using One.Net.BLL.Utility;
using System.Globalization;
using System.Data.SqlTypes;


namespace OneMainWeb
{
    [Serializable]
    public partial class Articles : OneBasePage
    {
        private static readonly BArticle articleB = new BArticle();
        private static readonly ArticleDataSource articleDS = new ArticleDataSource();

        protected BOArticle SelectedArticle
        {
            get { return ViewState["SelectedArticle"] as BOArticle; }
            set { ViewState["SelectedArticle"] = value; }
        }

        

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;
            AutoPublishWarning.Visible = AutoPublish;
            if (!IsPostBack)
            {
                ddlRegularFilter.DataSource = articleB.ListRegulars(new ListingState(SortDir.Ascending, ""), ShowUntranslated, null, null);
                ddlRegularFilter.DataBind();
            }
            
        }

        private void PrepareEmptyArticle()
        {
            SelectedArticle = new BOArticle();
            SelectedArticle.Title = "";
            SelectedArticle.SubTitle = "";
            SelectedArticle.Teaser = "";
            SelectedArticle.Html = "";
            SelectedArticle.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            SelectedArticle.IsNew = true;
            SelectedArticle.PublishFlag = false;
        }

        protected void cmdShowById_Click(object sender, EventArgs e)
        {
            int id = FormatTool.GetInteger(TextBoxShowById.Text);

            if ( id > -1)
            {
                SelectedArticle = articleB.GetArticle(id, ShowUntranslated);

                if (SelectedArticle != null)
                {
                    Multiview1.ActiveViewIndex = 1;
                }
                else
                {
                    Notifier1.Message = "$article_does_not_exist";
                    Notifier1.Visible = true;
                }
            }
            else
            {
                // text provided was not a number... so do text search
                articleGridView.DataBind();
            }
        }

        protected void cmdAddArticle_Click(object sender, EventArgs e)
        {
            PrepareEmptyArticle();
            Multiview1.ActiveViewIndex = 1;
        }

        protected void ddlRegularFilter_DataBound(object sender, EventArgs e)
        {
            //add an empty item on top of the list
            AddEmptyItem((System.Web.UI.WebControls.DropDownList)sender);
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedArticle = null;
            Multiview1.ActiveViewIndex = 0;
        }

        protected void InsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveArticle(false);
        }

        protected void InsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveArticle(true);
        }
        
        protected void cmdFilterArticles_Click(object sender, EventArgs e)
        {
            articleGridView.DataBind();
        }

        private void Regulars_DataBind(ListControl lb)
        {
            if (lb != null)
            {
                lb.DataSource = articleB.ListRegulars(new ListingState(), ShowUntranslated, null, null);
                lb.DataTextField = "Title";
                lb.DataValueField = "Id";
                lb.DataBind();
            }
        }

        protected void Multiview1_ActiveViewChanged(object sender, EventArgs e)
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
                Regulars_DataBind(lbRegulars);

                if (SelectedArticle != null)
                {
                    lbRegularsAssignedToArticle.DataSource = SelectedArticle.Regulars;
                    lbRegularsAssignedToArticle.DataTextField = "Title";
                    lbRegularsAssignedToArticle.DataValueField = "Id";
                    lbRegularsAssignedToArticle.DataBind();
                }

                AutoPublishWarning.Visible = this.AutoPublish && (bool)Context.Items["publish"];
                TextContentEditor.UseCkEditor = true;
                LastChangeAndHistory1.SelectedContentId = 0;
                LastChangeAndHistory1.Text = "";

                if (SelectedArticle != null)
                {

                    if (SelectedArticle.DisplayDate != DateTime.MinValue)
                    {
                        TextBoxDate.Text = SelectedArticle.DisplayDate.ToString("d", Thread.CurrentThread.CurrentUICulture) + " " + SelectedArticle.DisplayDate.ToString("HH:mm", Thread.CurrentThread.CurrentUICulture);
                    }

                    TextContentEditor.Title = SelectedArticle.Title;
                    TextContentEditor.SubTitle = SelectedArticle.SubTitle;
                    TextContentEditor.Teaser = SelectedArticle.Teaser;
                    TextContentEditor.Html = SelectedArticle.Html;

                    if (SelectedArticle.Id.HasValue)
                    {
                        LastChangeAndHistory1.SelectedContentId = SelectedArticle.ContentId.Value;
                        LastChangeAndHistory1.Text = SelectedArticle.DisplayLastChanged;
                        LabelId.Text = SelectedArticle.Id.Value.ToString();
                       
                    }
                    InsertUpdateButton.Text = "Save";
                    InsertUpdateCloseButton.Text = "Save & Close";
                }
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
            articleGridView.DataSource = articles;
            articleGridView.DataBind();
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Articles_DataBind();
        }

        protected void articleGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedArticle = articleB.GetArticle(Int32.Parse(grid.SelectedValue.ToString()), true);
                Multiview1.ActiveViewIndex = 1;
            }
        }

        private void SaveArticle(bool close)
        {
            try
            {
                if (lbRegularsAssignedToArticle.Items.Count == 0)
                {
                    Notifier1.Warning = "$minimum_one_regular_has_to_be_assigned_to_article";
                    return;
                }
                else
                {
                    var ampersands = Validator.CheckForAmpersand(TextContentEditor.Html);
                    var errors = new List<ValidatorError>();

                    Validator.CheckHtml(TextContentEditor.Html, ref errors);
                    var hasErrors = errors.Count > 0;
                    var hasAmpersands = ampersands.Count > 0;

                    if (!hasErrors && !hasAmpersands)
                    {
                        ampersands = Validator.CheckForAmpersand(TextContentEditor.Teaser);

                        Validator.CheckHtml(TextContentEditor.Teaser, ref errors);
                        hasErrors = errors.Count > 0;
                        hasAmpersands = ampersands.Count > 0;
                    }

                    if (EnableXHTMLValidator && (hasErrors || hasAmpersands))
                    {
                        if (hasErrors)
                            Notifier1.Warning += "<h3>" + "$errors" + "</h3><ul>";

                        foreach (var validatorError in errors)
                        {
                            Notifier1.Warning += "<li>" + "$" + validatorError.Error;
                            if (!string.IsNullOrEmpty(validatorError.Tag))
                                Notifier1.Warning += "<span>" + validatorError.Tag + "</span>";
                            Notifier1.Warning += "</li>";
                        }

                        if (hasErrors)
                            Notifier1.Warning += "</ul>";

                        if (hasAmpersands)
                        {
                            Notifier1.Warning += "<h3>" + "$ampersands" + "</h3><ul>";
                            foreach (int i in ampersands)
                            {
                                Notifier1.Warning += "<li>" + "$position" + "<span>" + i + "</span></li>";
                            }
                            Notifier1.Warning += "</ul>";
                        }
                    }
                    else
                    {
                        SelectedArticle.Title = TextContentEditor.Title;
                        SelectedArticle.SubTitle = TextContentEditor.SubTitle;
                        SelectedArticle.Teaser = TextContentEditor.Teaser;
                        SelectedArticle.Html = TextContentEditor.Html;

                        var d = SqlDateTime.MinValue.Value;
                        DateTime.TryParse(TextBoxDate.Text, Thread.CurrentThread.CurrentUICulture, DateTimeStyles.None, out d);
                        SelectedArticle.DisplayDate = d;
                        SelectedArticle.IsChanged = true;
                        SelectedArticle.MarkedForDeletion = false;
                        SelectedArticle.PublishFlag = false;
                        SelectedArticle.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                        
                        SelectedArticle.Regulars.Clear();
                        foreach (ListItem item in lbRegularsAssignedToArticle.Items)
                        {
                            BORegular regular = new BORegular();
                            regular.Id = Int32.Parse(item.Value);
                            regular.Title = item.Text;
                            SelectedArticle.Regulars.Add(regular);
                        }
                        articleB.ChangeArticle(SelectedArticle);

                        if (AutoPublish)
                            articleB.Publish(SelectedArticle.Id.Value);
                        Notifier1.Message = "$article_saved";

                        if (close)
                        {
                            SelectedArticle = null;
                            Multiview1.ActiveViewIndex = 0;
                        }
                        else
                        {
                            Multiview1.ActiveViewIndex = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "$error_saving";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
        }
        
       

        protected void cmdAssignRegularToArticle_Click(object sender, EventArgs e)
        {
            if (lbRegulars.SelectedItem != null)
            {
                int regularId = Int32.Parse(lbRegulars.SelectedValue);
                ListItem item = new ListItem(lbRegulars.SelectedItem.Text, regularId.ToString());
                if (!lbRegularsAssignedToArticle.Items.Contains(item) && regularId > 0)
                    lbRegularsAssignedToArticle.Items.Add(item);
            }
        }

        protected void cmdRemoveRegularFromArticle_Click(object sender, EventArgs e)
        {
            int regularId = Int32.Parse(lbRegularsAssignedToArticle.SelectedItem.Value);
            ListItem item = new ListItem(lbRegularsAssignedToArticle.SelectedItem.Text, regularId.ToString());
            lbRegularsAssignedToArticle.Items.Remove(item);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (AutoPublishWarning != null)
                AutoPublishWarning.Visible = this.AutoPublish && (bool)Context.Items["publish"];
            base.OnPreRender(e);
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in articleGridView.Rows)
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
                if (articleB.MarkForDeletion(i))
                {
                    deletedCount++;
                }
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Marked {0} articles for delete", deletedCount);
                articleGridView.DataBind();
            }
        }

        protected void ButtonPublish_Click(object sender, EventArgs e)
        {
            int publishCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                if (articleB.Publish(i))
                {
                    publishCount++;
                }
            }
            if (publishCount > 0)
            {
                Notifier1.Title = string.Format("Published {0} articles", publishCount);
                articleGridView.DataBind();
            }
        }

        

        protected void ButtonRevert_Click(object sender, EventArgs e)
        {
            int revertCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                if (articleB.RevertToPublished(i))
                {
                    revertCount++;
                }
            }
            if (revertCount > 0)
            {
                Notifier1.Title = string.Format("Reverted to published {0} articles", revertCount);
                articleGridView.DataBind();
            }
        }
    }

    [Serializable]
    public class ArticleDataSource
    {
        private readonly static BArticle articleB = new BArticle();

        public ArticleDataSource()
        { }

        public int SelectArticleCount()
        {
            return (int)HttpContext.Current.Items["rowCount"];
        }

        public BOArticle GetArticle(int id)
        {
            return articleB.GetArticle(id, false);
        }

        public PagedList<BOArticle> SelectArticles(int recordsPerPage, int firstRecordIndex, string sortBy, string strRegularId, bool showUntranslated, string titleSearch)
        {
            int regularId = FormatTool.GetInteger(strRegularId);
            string regularFilter = "";
            if (regularId > 0)
            {
                regularFilter = regularId.ToString();
            }
            
            PagedList<BOArticle> articles = articleB.ListArticles(regularFilter, showUntranslated,
                new ListingState(recordsPerPage, firstRecordIndex, (sortBy.Contains("ASC") || !sortBy.Contains("DESC") ? SortDir.Ascending : SortDir.Descending), sortBy.Replace("DESC", "").Replace("ASC", "")), titleSearch, null, null);

            HttpContext.Current.Items["rowCount"] = articles.AllRecords;
            return articles;
        }

        public void ChangeArticle(BOArticle article, out BOArticle articleOut)
        {
            articleB.ChangeArticle(article);
            articleOut = article;
        }

        public void ChangeArticle(BOArticle article)
        {
            articleB.ChangeArticle(article);
        }

        public void ChangeRegular(BORegular regular)
        {
            articleB.ChangeRegular(regular);
        }

        public void MarkForDeletion(int id)
        {
            articleB.MarkForDeletion(id);
        }
    }

}
