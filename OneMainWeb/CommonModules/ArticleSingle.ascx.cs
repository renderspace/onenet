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
    public partial class ArticleSingle : MModule, IBasicSEOProvider
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

                if (Time1 != null)
                {
                    Time1.Visible = ShowDate;
                    Time1.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                    Time1.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                    Time1.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                }
                if (Time2 != null)
                {
                    Time2.Visible = ShowDateBelowTitle;
                    Time2.Attributes.Add("datetime", article.DisplayDate.ToString("yyyy-MM-dd"));
                    Time2.Attributes.Add("pubdate", article.DateCreated.ToString("yyyy-MM-dd"));
                    Time2.InnerHtml = article.DisplayDate.ToString(DateFormatString);
                }

                var id = article.Id.Value;
                if (HtmlArticle != null)
                {
                    HtmlArticle.Attributes.Add("class", "hentry a" + id.ToString() + " " + MModule.RenderOrder(id));
                }
                
                Title = article.Title;

                if (H1Title != null)
                {
                    H1Title.InnerHtml = article.Title;
                }
                if (H2SubTitle != null)
                {
                    H2SubTitle.Visible = ShowSubTitle;
                    H2SubTitle.InnerHtml = article.SubTitle;
                }

                if (SectionTeaser != null)
                {
                    SectionTeaser.Visible = ShowTeaser;
                    SectionTeaser.InnerHtml = article.Teaser;
                }

                Description = article.Teaser;

                if (SectionHtml != null)
                {
                    SectionHtml.InnerHtml = article.Html;
                }

                if (DivReadon != null)
                {
                    DivReadon.Visible = !string.IsNullOrWhiteSpace(ArticleListUri);
                }
            }
            else
            {
                MultiView1.ActiveViewIndex = 0;
            }
        }

        public string Description
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string OgImageUrl
        {
            get;
            set;
        }
    }
}