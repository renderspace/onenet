using System;

namespace One.Net.BLL
{
	[Serializable]
    public abstract class BOIntCont
    {
        string wholeHtml;

        public string WholeHtml
        {
            get { return wholeHtml; }
            set { wholeHtml = value; }
        }

        public virtual string ProcessedHtml
        {
            get { return wholeHtml; }
        }

        private string cssClass = "";

        public string CssClass
        {
            get { return cssClass; }
            set { cssClass = value; }
        }

        bool isInternal;

        public bool IsInternal
        {
            get { return isInternal; }
            set { isInternal = value; }
        }

        public BOIntCont() { }

        public BOIntCont(string html)
        {
            WholeHtml = html;
        }
    }
}
