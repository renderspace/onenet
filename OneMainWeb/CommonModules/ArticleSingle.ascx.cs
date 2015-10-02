using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleSingle : MModule, IBasicSEOProvider, IImageListProvider
    {
        public const string REQUEST_ARTICLE_ID = "aid";
        private static readonly BArticle articleB = new BArticle();
        protected BOArticle RequestedArticle;

        [Setting(SettingType.Url)]
        public string ArticleListUri { get { return GetStringSetting("ArticleListUri"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowDate { get { return GetBooleanSetting("ShowDate"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowDateBelowTitle { get { return GetBooleanSetting("ShowDateBelowTitle"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool ShowSubTitle { get { return GetBooleanSetting("ShowSubTitle"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowTeaser { get { return GetBooleanSetting("ShowTeaser"); } }

        [Setting(SettingType.String, DefaultValue = "dd.MM.yy")]
        public string DateFormatString { get { return GetStringSetting("DateFormatString"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool HideImages { get { return GetBooleanSetting("HideImages"); } }

        [Setting(SettingType.ImageTemplate, DefaultValue = "0")]
        public BOImageTemplate ThumbTemplate { get { return GetImageTemplate("ThumbTemplate"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HasHumanReadableUrlParameter)
                return;

            int articleId = 0;

            BOArticle originalArticle = null;

            if (Request[REQUEST_ARTICLE_ID] != null)
            {
                int.TryParse(Request[REQUEST_ARTICLE_ID], out articleId);
                originalArticle = articleB.GetArticle(articleId);
            } 
            else if (HasHumanReadableUrlParameter)
            {
                originalArticle = articleB.GetArticle(HumanReadableUrlParameter);
            }

            if (originalArticle != null)
            {
                if (MultiView1 != null)
                    MultiView1.ActiveViewIndex = 1;

                ListImages = originalArticle.Images;

                var article = (BOArticle)originalArticle.Clone();
                RequestedArticle = article;

                if (HideImages)
                {
                    foreach (BOIntContImage image in article.Images)
                    {
                        article.RemoveImages.Add(image);
                    }
                }

                if (DivTeaserImage != null && ThumbTemplate != null && ListImages.Count > 0)
                {
                    var image = ListImages.FirstOrDefault();
                    article.RemoveImages.Add(image);

                    DivTeaserImage.Visible = true;
                    DivTeaserImage.InnerHtml = ThumbTemplate.RenderHtml(image.Alt, image.FullUri, image.CssClass);
                    DivTeaserImage.Attributes.Add("data-full-size-image-uri", image.FullUri);
                }
                else if (DivTeaserImage != null)
                {
                    DivTeaserImage.Visible = false;
                }

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
                    H2SubTitle.Visible = ShowSubTitle && !string.IsNullOrWhiteSpace(article.SubTitle);
                    H2SubTitle.InnerHtml = article.SubTitle;
                }

                if (SectionTeaser != null)
                {
                    SectionTeaser.Visible = ShowTeaser && !string.IsNullOrWhiteSpace(article.ProcessedTeaser);
                    SectionTeaser.InnerHtml = article.ProcessedTeaser;
                }

                Description = article.Teaser;

                if (SectionHtml != null)
                {
                    SectionHtml.InnerHtml = article.ProcessedHtml;
                }

                if (DivReadon != null)
                {
                    DivReadon.Visible = !string.IsNullOrWhiteSpace(ArticleListUri);
                }
            }
            else
            {
                MultiView1.ActiveViewIndex = 0;
                ListImages = new List<BOIntContImage>();
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

        public List<BOIntContImage> ListImages
        {
            get;
            set;
        }
    }
}