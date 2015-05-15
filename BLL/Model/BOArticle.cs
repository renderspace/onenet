using System;
using System.Collections.Generic;
using One.Net.BLL.Model;

namespace One.Net.BLL
{
    [Serializable]
    public class BOArticle : PublishableInternalContent, IPublishable, ICloneable
    {
        #region Variables

        private List<BORegular> regulars = new List<BORegular>();
        private int? id;
        
        private DateTime displayDate;

        #endregion Variables

        #region Properties

        public int? Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<BORegular> Regulars
        {
            get { return regulars; }
            set { regulars = value; }
        }

        public string RegularsList
        {
            get 
            {
                string answer = "";
                foreach (BORegular regular in regulars)
                {
                    answer += regular.Title + ", ";
                }
                if (regulars.Count > 0)
                {
                    answer = answer.Remove(answer.Length - 2);
                }
                return answer;
            }
        }

        public DateTime DisplayDate
        {
            get { return displayDate; }
            set { displayDate = value; }
        }

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

        #endregion Properties

        #region Methods

        public string RenderRegulars()
        {
            var result = "";
            if (regulars.Count > 0)
                result += "<ul>";
            foreach (var regular in regulars)
            {
                result += "<li>" + regular.Title + "</li>";
            }
            if (regulars.Count > 0)
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

            foreach (BORegular regular in regulars)
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
