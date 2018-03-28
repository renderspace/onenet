using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;
using One.Net.BLL.Utility;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleSingle : MModule, IBasicSEOProvider, IImageListProvider
    {
        public const string REQUEST_ARTICLE_ID = "aid";
        private static readonly BArticle articleB = new BArticle();

        [Setting(SettingType.CSInteger, DefaultValue = "0")]
        public int OverrideLanguageId
        {
            get
            {
                return GetIntegerSetting("OverrideLanguageId");
            }
        }

        [Setting(SettingType.CSInteger)]
        public List<int> HiddenTagsList
        {
            get
            {
                return GetIntegerListSetting("HiddenTagsList");
            }
        }

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

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool HideHtml { get { return GetBooleanSetting("HideHtml"); } }

        [Setting(SettingType.String, DefaultValue = "dd.MM.yy")]
        public string DateFormatString { get { return GetStringSetting("DateFormatString"); } }

        [Setting(SettingType.ImageTemplate, DefaultValue = "0")]
        public BOImageTemplate ThumbTemplate { get { return GetImageTemplate("ThumbTemplate"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowCategories { get { return GetBooleanSetting("ShowCategories"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HasHumanReadableUrlParameter)
                return;

            BOArticle article = null;

            if (HasHumanReadableUrlParameter)
            {
                var originalArticle = articleB.GetArticle(HumanReadableUrlParameter, OverrideLanguageId);
                if (originalArticle != null)
                {
                    article = originalArticle.Clone() as BOArticle;
                }
            }

            if (article != null)
            {
                ListImages = new List<BOIntContImage>();

                if (MultiView1 != null)
                    MultiView1.ActiveViewIndex = 1;

                this.Attributes.Add("data-article-id", article.Id.ToString());

                foreach (var img in article.ImagesForGallery.ToList())
                {
                    ListImages.Add(img);
                    article.RemoveImages.Add(img);
                }

                if (DivTeaserImage != null && ThumbTemplate != null && article.ImagesNotForGallery.Count() > 0)
                {
                    var image = article.ImagesNotForGallery.FirstOrDefault();
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
                    HtmlArticle.Attributes.Add("data-article-id", id.ToString());
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

                SectionHtml.Visible = !HideHtml;

                if (DivReadon != null)
                {
                    DivReadon.Visible = !string.IsNullOrWhiteSpace(ArticleListUri);
                }

                if (RepeaterCategories != null && ShowCategories)
                {
                    var regulars = article.Regulars;
                    if (HiddenTagsList != null && HiddenTagsList.Count > 0)
                    {
                        regulars.RemoveAll(r => HiddenTagsList.Contains(r.Id.Value));
                    }

                    RepeaterCategories.DataSource = regulars;
                    RepeaterCategories.DataBind();
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