using System;
using System.Collections.Generic;
using One.Net.BLL.Model;

namespace One.Net.BLL
{
    [Serializable]
    public class BOArticle : PublishableInternalContent, IPublishable, ICloneable
    {
        #region Properties

        public int? Id { get; set;  }

        public List<BORegular> Regulars { get; set; }

        public string RegularsList
        {
            get 
            {
                string answer = "";
                foreach (BORegular regular in Regulars)
                {
                    answer += regular.Title + ", ";
                }
                if (Regulars.Count > 0)
                {
                    answer = answer.Remove(answer.Length - 2);
                }
                return answer;
            }
        }

        public DateTime DisplayDate { get; set; }

        /// <summary>
        /// Returns first image id in the content.
        /// </summary>
        public int TeaserImageId
        {
            get 
            { 
                if (base.Images.Count > 0)
                {
                    return base.Images[0].FileID; 
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool NoSingleView { get; set; }

        public string HumanReadableUrl { get; set; }

        public string Permalink { get; set; }

        #endregion Properties

        public BOArticle()
        {
            Regulars = new List<BORegular>();
        }

        #region Methods

        public string RenderRegulars()
        {
            var result = "";
            if (Regulars.Count > 0)
                result += "<ul>";
            foreach (var regular in Regulars)
            {
                result += "<li>" + regular.Title + "</li>";
            }
            if (Regulars.Count > 0)
                result += "</ul>";

            return result;
        }

        /// <summary>
        /// Create a new object in memory and deep copy everything from existing object.
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            BOArticle article = new BOArticle();
            this.CloneContent(article);

            article.Id = this.Id;
            article.IsChanged = this.IsChanged;
            article.MarkedForDeletion = this.MarkedForDeletion;
            article.PublishFlag = this.PublishFlag;
            article.DisplayDate = this.DisplayDate;
            article.HumanReadableUrl = this.HumanReadableUrl;
            article.Permalink = this.Permalink;
            article.NoSingleView = this.NoSingleView;

            foreach (BORegular regular in Regulars)
            {
				article.Regulars.Add((BORegular) regular.Clone());
            }

            return article;
        }

        #endregion Methods
    }

    [Serializable]
    public class BOArticleMonth
    {
        public DateTime Date { get; set; }
        public int ArticleCount { get; set; }
    }

    [Serializable]
    public class BOArticleMonthDay
    {
        public DateTime Date { get; set; }
        public int ArticleCount { get; set; }
    }
}
