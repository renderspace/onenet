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
    public partial class ArticleSingle : MModule
    {
        public const string REQUEST_ARTICLE_ID = "aid";
        private static readonly BArticle articleB = new BArticle();

        protected string ArticleListUri { get { return GetStringSetting("ArticleListUri"); } }
        protected bool ShowDate { get { return GetBooleanSetting("ShowDate"); } }
        protected bool ShowDateBelowTitle { get { return GetBooleanSetting("ShowDateBelowTitle"); } }
        protected bool ShowSubTitle { get { return GetBooleanSetting("ShowSubTitle"); } }
        protected bool ShowTeaser { get { return GetBooleanSetting("ShowTeaser"); } }
        protected string DateFormatString { get { return GetStringSetting("DateFormatString"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            int articleId = 0;
            if (Request[REQUEST_ARTICLE_ID] != null)
            {
                int.TryParse(Request[REQUEST_ARTICLE_ID], out articleId);
            }

            var article = articleB.GetArticle(articleId, false);

            if (article != null)
            {

                MultiView1.ActiveViewIndex = 1;

                Time1.Visible = ShowDate;
                Time1.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                Time1.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                Time1.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                Time2.Visible = ShowDateBelowTitle;
                Time2.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                Time2.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                Time2.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                var id = article.Id.Value;
                HtmlArticle.Attributes.Add("class", "hentry a" + id.ToString() + " " + MModule.RenderOrder(id));
                
                H1Title.InnerHtml = article.Title;
                H2SubTitle.Visible = ShowSubTitle;
                H2SubTitle.InnerHtml = article.SubTitle;
                Time2.Visible = false;
                SectionTeaser.Visible = ShowTeaser;
                SectionTeaser.InnerHtml = article.Teaser;
                SectionHtml.InnerHtml = article.Html;
                DivReadon.Visible = !string.IsNullOrWhiteSpace(ArticleListUri);
            }
            else
            {
                MultiView1.ActiveViewIndex = 0;
            }
        }
    }
}