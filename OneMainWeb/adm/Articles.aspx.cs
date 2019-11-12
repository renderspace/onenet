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
using System.Data.Sql;
using System.Data.SqlClient;
using MsSqlDBUtility;
using System.Linq;

namespace OneMainWeb
{
    public partial class Articles : OneBasePage
    {
        private static readonly BArticle articleB = new BArticle();

        protected BOArticle SelectedArticle
        {
            get { return ViewState["SelectedArticle"] as BOArticle; }
            set { ViewState["SelectedArticle"] = value; }
        }        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SelectedWebsite == null)
            {
                Notifier1.Warning = "You don't have permissions for any site or there are no websites defined in database.";
                return;
            }

            Notifier1.Visible = true;
            if (!IsPostBack)
            {

                Multiview1.ActiveViewIndex = 0;
                Regulars_DataBind(DropDownListRegularFilter);
                /*
                try
                {
                    Multiview1.ActiveViewIndex = 0;
                    Regulars_DataBind(DropDownListRegularFilter);   
                }
                catch
                {
                    Multiview1.ActiveViewIndex = 2;
                }*/
            }
            if (!IsPostBack && Request["keyword"] != null)
            {
                TextBoxShowById.Text = Request["keyword"];
                cmdShowById_Click(null, null);
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
            SelectedArticle.DisplayDate = DateTime.Today;
        }

        protected void cmdAddArticle_Click(object sender, EventArgs e)
        {
            PrepareEmptyArticle();
            Multiview1.ActiveViewIndex = 1;
        }

        protected void DropDownListRegularFilter_DataBound(object sender, EventArgs e)
        {
            //add an empty item on top of the list
            AddEmptyItem((System.Web.UI.WebControls.DropDownList)sender);
        }

        protected void InsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveArticle(false);
        }

        protected void InsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveArticle(true);
        }

        private void Regulars_DataBind(ListControl lb)
        {
            if (lb != null)
            {

                var regulars = articleB.ListRegulars(new ListingState(SortDir.Ascending, ""));
                if (regulars.Count() == 0)
                {
                    Notifier1.Warning = "You need to add at least one regular (article category). Use menu on the left.";
                    return;
                }
                if (regulars.Where(r => string.IsNullOrWhiteSpace(r.HumanReadableUrl)).Count() > 0)
                {
                    throw new Exception("regulars human readable URL problem");
                }

                lb.DataSource = regulars; 
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

                GridViewSortExpression = "display_date";
                GridViewSortDirection = SortDir.Descending;
                
                Articles_DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                Regulars_DataBind(ListBoxRegulars);

                if (SelectedArticle != null)
                {
                    ListBoxAssignedToArticle.DataSource = SelectedArticle.Regulars;
                    ListBoxAssignedToArticle.DataTextField = "Title";
                    ListBoxAssignedToArticle.DataValueField = "Id";
                    ListBoxAssignedToArticle.DataBind();
                }

                TextContentEditor.UseCkEditor = true;
                LastChangeAndHistory1.SelectedContentId = 0;
                LastChangeAndHistory1.Text = "";

                if (SelectedArticle != null)
                {

                    if (SelectedArticle.DisplayDate != DateTime.MinValue)
                    {
                        TextBoxDate.Text = SelectedArticle.DisplayDate.ToString("d", Thread.CurrentThread.CurrentUICulture) + " " + SelectedArticle.DisplayDate.ToString("HH:mm", Thread.CurrentThread.CurrentUICulture);
                    }

                    TextBoxHumanReadableUrl.Text = string.IsNullOrEmpty(SelectedArticle.HumanReadableUrl) ? "" : SelectedArticle.HumanReadableUrl;
                    TextContentEditor.Title = SelectedArticle.Title;
                    TextContentEditor.SubTitle = SelectedArticle.SubTitle;
                    TextContentEditor.Teaser = SelectedArticle.Teaser;
                    TextContentEditor.Html = SelectedArticle.Html;

                    if (SelectedArticle.Id.HasValue)
                    {
                        LastChangeAndHistory1.SelectedContentId = SelectedArticle.ContentId.Value;
                        LastChangeAndHistory1.Text = SelectedArticle.DisplayLastChanged;
                        LastChangeAndHistory1.SelectedLanguageId = SelectedArticle.LanguageId;
                        LabelId.Text = SelectedArticle.Id.Value.ToString();
                       
                    }
                    InsertUpdateButton.Text = "Save";
                    InsertUpdateCloseButton.Text = "Save & Close";
                }
            }

            log.Debug("Articles Multiview1_ActiveViewChanged (end)");
        }

        protected void LinkButtonConvert_Click(object sender, EventArgs e)
        {
            var button = sender as LinkButton;
            var commandArgument = button.CommandArgument.ToString();
            if (commandArgument == "Id")
            {
                ConfigureArticles(false);
            }
            else
            {
                ConfigureArticles(true);
            }

            Multiview1.ActiveViewIndex = 0;
        }

        private void ConfigureArticles(bool autoGeneratePartialLinks)
        {
            try
            {
                Regulars_DataBind(DropDownListRegularFilter);

                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;

                GridViewSortExpression = "display_date";
                GridViewSortDirection = SortDir.Descending;

                Articles_DataBind();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("articles human readable URL problem"))
                {
                    var result = articleB.AutoCreateHumanReadableUrlArticles(autoGeneratePartialLinks);
                    result += articleB.AutoCreateHumanReadableUrlRegulars(autoGeneratePartialLinks);
                    Notifier1.Warning = "Updated " + result.ToString() + " items with human readable URLs.";
                    Articles_DataBind();
                }
                else if (ex.Message.Contains("regulars human readable URL problem"))
                {
                    var result = articleB.AutoCreateHumanReadableUrlRegulars(autoGeneratePartialLinks);
                    Notifier1.Warning = "Updated " + result.ToString() + " regulars with human readable URLs.";
                    Articles_DataBind();
                }
                else if (ex.Message.Contains("human_readable_url"))
                {
                    articleB.UpgradeArticles();
                    var result = articleB.AutoCreateHumanReadableUrlArticles(autoGeneratePartialLinks);
                    result += articleB.AutoCreateHumanReadableUrlRegulars(autoGeneratePartialLinks);
                    Notifier1.Warning = "Updated " + result.ToString() + " items with human readable URLs.";
                }
                else
                {
                    throw;
                }
            }

            Regulars_DataBind(DropDownListRegularFilter);  
        }

        protected void GridViewArticles_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedArticle = articleB.GetArticle(Int32.Parse(grid.SelectedValue.ToString()));
                Multiview1.ActiveViewIndex = 1;
            }
        }

        private void SaveArticle(bool close)
        {
            try
            {
                var d = SqlDateTime.MinValue.Value;
                DateTime.TryParse(TextBoxDate.Text, Thread.CurrentThread.CurrentUICulture, DateTimeStyles.None, out d);

                var humanReadableUrl = TextBoxHumanReadableUrl.Text.Trim();


                if (ListBoxAssignedToArticle.Items.Count == 0)
                {
                    Notifier1.Warning = "You need to select at least one category/regular. Use the 'right' botton below.";
                    return;
                }
                else if (d <= SqlDateTime.MinValue.Value)
                {
                    Notifier1.Warning = "Please select article date.";
                    return;
                }
                else if (String.IsNullOrEmpty(humanReadableUrl))
                {
                    Notifier1.Warning = "Human readable url cannot be empty.";
                    return;
                }
                else
                {
                    SelectedArticle.HumanReadableUrl = humanReadableUrl;
                    SelectedArticle.Title = TextContentEditor.Title;
                    SelectedArticle.SubTitle = TextContentEditor.SubTitle;
                    SelectedArticle.Teaser = TextContentEditor.Teaser;
                    SelectedArticle.Html = TextContentEditor.Html;
                    SelectedArticle.DisplayDate = d;
                    SelectedArticle.IsChanged = true;
                    SelectedArticle.MarkedForDeletion = false;
                    SelectedArticle.PublishFlag = false;
                    SelectedArticle.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                        
                    SelectedArticle.Regulars.Clear();
                    foreach (ListItem item in ListBoxAssignedToArticle.Items)
                    {
                        BORegular regular = new BORegular();
                        regular.Id = Int32.Parse(item.Value);
                        regular.Title = item.Text;
                        SelectedArticle.Regulars.Add(regular);
                    }
                    articleB.ChangeArticle(SelectedArticle);
                    Notifier1.Message = "Article saved";

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
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "Error saving";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
        }

        protected void cmdAssignRegularToArticle_Click(object sender, EventArgs e)
        {
            if (ListBoxRegulars.SelectedItem != null)
            {
                int regularId = Int32.Parse(ListBoxRegulars.SelectedValue);
                ListItem item = new ListItem(ListBoxRegulars.SelectedItem.Text, regularId.ToString());
                if (!ListBoxAssignedToArticle.Items.Contains(item) && regularId > 0)
                    ListBoxAssignedToArticle.Items.Add(item);
            }
        }

        protected void cmdRemoveRegularFromArticle_Click(object sender, EventArgs e)
        {
            int regularId = Int32.Parse(ListBoxAssignedToArticle.SelectedItem.Value);
            ListItem item = new ListItem(ListBoxAssignedToArticle.SelectedItem.Text, regularId.ToString());
            ListBoxAssignedToArticle.Items.Remove(item);
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in GridViewArticles.Rows)
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
                Articles_DataBind();
            }
        }

        protected void ButtonPublish_Click(object sender, EventArgs e)
        {
            if (!PublishRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

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
                Articles_DataBind();
            }
        }

        protected void ButtonUnPublish_Click(object sender, EventArgs e)
        {
            if (!PublishRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

            int unPublishCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                if (articleB.UnPublish(i))
                {
                    unPublishCount++;
                }
            }
            if (unPublishCount > 0)
            {
                Notifier1.Title = string.Format("Unpublished {0} articles", unPublishCount);
                Articles_DataBind();
            }
        }

        protected void ButtonRevert_Click(object sender, EventArgs e)
        {
            if (!PublishRights)
            {
                Notifier1.Warning = "You don't have publish rights.";
                Notifier1.Message = "Contact administrator";
                return;
            }

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
                Articles_DataBind();
            }
        }

        private void Articles_DataBind()
        {
            Articles_DataBind("");
        }

        private void Articles_DataBind(string searchBy)
        {
            var regularsFilter = DropDownListRegularFilter.SelectedValue;
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;
            var regularIds = StringTool.SplitStringToIntegers(regularsFilter);
            var excludeRegularIds = new List<int>();
            PagedList<BOArticle> articles = articleB.ListArticles(regularIds, null, null, state, searchBy, excludeRegularIds);

            var missingUrlArticles = articles.Where(ar => string.IsNullOrWhiteSpace(ar.HumanReadableUrl));
            if (missingUrlArticles.Count() > 0)
            {
                throw new Exception("articles human readable URL problem");
            }
            TwoPostbackPager1.TotalRecords = articles.AllRecords;
            TwoPostbackPager1.DetermineData();
            GridViewArticles.DataSource = articles;
            GridViewArticles.DataBind();
            ButtonRevert.Visible = ButtonPublish.Visible = (articles.Count != 0) && PublishRights;
            PanelGridButtons.Visible = (articles.Count != 0);
            TwoPostbackPager1.Visible = (articles.Count != 0);
            GridViewArticles.Visible = (articles.Count != 0);
            PanelNoResults.Visible = (articles.Count == 0);
        }

        protected void cmdFilterArticles_Click(object sender, EventArgs e)
        {
            Articles_DataBind();
        }

        protected void cmdShowById_Click(object sender, EventArgs e)
        {
            int id = FormatTool.GetInteger(TextBoxShowById.Text);

            if (id > -1)
            {
                SelectedArticle = articleB.GetArticle(id);

                if (SelectedArticle != null)
                {
                    Multiview1.ActiveViewIndex = 1;
                }
                else
                {
                    Notifier1.Message = "Article by this ID does not exist";
                    Notifier1.Visible = true;
                }
            }
            else
            {
                Articles_DataBind(TextBoxShowById.Text);
            }
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Articles_DataBind();
        }

        protected void GridViewArticles_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            Articles_DataBind();
        }
    }
}
