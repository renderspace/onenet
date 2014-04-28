using System;

namespace One.Net.BLL
{
    [Serializable]
    public class BORegular : BOInternalContent, ICloneable
    {
        #region Variables

        private int? id;
        private int articleCount = 0;

        #endregion Variables

        #region Properties

        public int? Id
        {
            get { return id; }
            set { id = value; }
        }

        public int ArticleCount
        {
            get { return articleCount; }
            set { articleCount = value; }
        }

        #endregion Properties

        public BORegular() { }
        public BORegular(int? id, int? contentId, int articleCount)
        {
            this.id = id;
            this.ContentId = contentId;
            this.articleCount = articleCount;
        }

        public object Clone()
        {
            BORegular result = new BORegular();
            this.CloneContent(result);
            result.Id = this.Id;
            result.ArticleCount = this.ArticleCount;
            return result;
        }
    }
}
