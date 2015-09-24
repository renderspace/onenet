using System;

namespace One.Net.BLL
{
    [Serializable]
    public class BORegular : BOInternalContent, ICloneable
    {

        #region Properties

        public int? Id { get; set;}

        public int ArticleCount { get; set; }

        public string HumanReadableUrl { get; set; }

        #endregion Properties

        public BORegular() { }
        public BORegular(int? id, int? contentId, int articleCount)
        {
            Id = id;
            this.ContentId = contentId;
            ArticleCount = articleCount;
        }

        public object Clone()
        {
            BORegular result = new BORegular();
            this.CloneContent(result);
            result.Id = this.Id;
            result.ArticleCount = this.ArticleCount;
            result.HumanReadableUrl = this.HumanReadableUrl;
            return result;
        }
    }
}
