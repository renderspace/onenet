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
                    Notifier1.Message = ResourceManager.GetString("$article_does_not_exist");
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
                articleGridView.DataBind();
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


        protected void articleGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedArticle = articleB.GetArticle(Int32.Parse(grid.SelectedValue.ToString()), true);
                Multiview1.ActiveViewIndex = 1;
            }
        }

        

        protected void articleGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count >= 4)
            {
                LinkButton cmdPublish = e.Row.FindControl("cmdPublish") as LinkButton;
                LinkButton cmdEdit = e.Row.FindControl("cmdEdit") as LinkButton;
                ImageButton cmdEditButton = e.Row.FindControl("cmdEditButton") as ImageButton;
                LinkButton cmdDelete = e.Row.FindControl("cmdDelete") as LinkButton;
                LinkButton cmdRevertToPublished = e.Row.FindControl("cmdRevertToPublished") as LinkButton;
                LinkButton cmdUnPublish = e.Row.FindControl("cmdUnPublish") as LinkButton;
                BOArticle article = e.Row.DataItem as BOArticle;

                if (article != null &&
                    cmdPublish != null &&
                    cmdEdit != null &&
                    cmdDelete != null &&
                    cmdRevertToPublished != null &&
                    cmdEditButton != null &&
                    cmdUnPublish != null)
                {
                    cmdUnPublish.Visible = !article.IsNew && !article.MarkedForDeletion && Authorization.HasPublishRights;
                    cmdPublish.Visible = (article.MarkedForDeletion || article.IsChanged) && Authorization.HasPublishRights;
                    cmdEdit.Visible = !article.MarkedForDeletion;
                    cmdDelete.Visible = !article.MarkedForDeletion;
                    cmdRevertToPublished.Visible = !article.IsNew && ( article.IsChanged || article.MarkedForDeletion);
                    cmdEditButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.edit.gif");
                    //cmdDelete.OnClientClick = @"return confirm('" + ResourceManager.GetString("$label_confirm_delete") + @"');";
                }
            }
        }

        protected void articleGridView_Deleted(object sender, GridViewDeletedEventArgs e)
        {
            Notifier1.Visible = true;
            if (e.Exception != null)
            {
                Notifier1.Message = e.Exception.Message;
                e.ExceptionHandled = true;
            }
            else
            {
                Notifier1.Message = ResourceManager.GetString("$mark_for_deletion_succeeded");
            }
        }

        protected void articleGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandArgument != null)
            {
                int id = FormatTool.GetInteger(e.CommandArgument);
                if (id > 0)
                {
                    switch (e.CommandName)
                    {
                        case "Publish":
                            {
                                Notifier1.Visible = true;
                                if (articleB.Publish(id))
                                    Notifier1.Message = ResourceManager.GetString("$publish_sucessfull");
                                else
                                    Notifier1.Message = ResourceManager.GetString("$publish_unsucessfull");

                                articleGridView.DataBind();
                            } break;
                        case "RevertToPublished":
                            {
                                articleB.RevertToPublished(id);
                                Notifier1.Visible = true;
                                Notifier1.Message = ResourceManager.GetString("$revert_to_published_succeeded");
                                articleGridView.DataBind();
                            } break;
                        case "UnPublish":
                            {
                                articleB.UnPublish(id);
                                Notifier1.Visible = true;
                                Notifier1.Message = ResourceManager.GetString("$unpublish_succeeded");
                                articleGridView.DataBind();
                                break;
                            }
                    }
                }
            }
        }

        protected void ObjectDataSourceArticleList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["showUntranslated"] = ShowUntranslated;

            int articleId = FormatTool.GetInteger(TextBoxShowById.Text);

            if (articleId == -1)
                e.InputParameters["titleSearch"] = TextBoxShowById.Text;
            else
                e.InputParameters["titleSearch"] = "";              

            if (e.ExecutingSelectCount)
                e.InputParameters.Clear();
        }

       

        private void SaveArticle(bool close)
        {
            try
            {
                if (lbRegularsAssignedToArticle.Items.Count == 0)
                {
                    Notifier1.Warning = ResourceManager.GetString("$minimum_one_regular_has_to_be_assigned_to_article");
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
                            Notifier1.Warning += "<h3>" + ResourceManager.GetString("$errors") + "</h3><ul>";

                        foreach (var validatorError in errors)
                        {
                            Notifier1.Warning += "<li>" + ResourceManager.GetString("$" + validatorError.Error);
                            if (!string.IsNullOrEmpty(validatorError.Tag))
                                Notifier1.Warning += "<span>" + validatorError.Tag + "</span>";
                            Notifier1.Warning += "</li>";
                        }

                        if (hasErrors)
                            Notifier1.Warning += "</ul>";

                        if (hasAmpersands)
                        {
                            Notifier1.Warning += "<h3>" + ResourceManager.GetString("$ampersands") + "</h3><ul>";
                            foreach (int i in ampersands)
                            {
                                Notifier1.Warning += "<li>" + ResourceManager.GetString("$position") + "<span>" + i + "</span></li>";
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
                        Notifier1.Message = ResourceManager.GetString("$article_saved");

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
                Notifier1.ExceptionName = ResourceManager.GetString("$error_saving");
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
            TextContentEditor.TextBoxCssClass = "ckeditor";
            if (AutoPublishWarning != null)
                AutoPublishWarning.Visible = this.AutoPublish && (bool)Context.Items["publish"];
            base.OnPreRender(e);
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
