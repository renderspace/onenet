using System;

namespace One.Net.BLL
{
	[Serializable]
    public abstract class BOIntCont
    {
        public string WholeHtml { get; set; }

        public virtual string ProcessedHtml
        {
            get { return WholeHtml; }
        }

        public string CssClass { get; set; }

        public bool IsInternal { get; set; }

        public BOIntCont() { }

        public BOIntCont(string html)
        {
            WholeHtml = html;
        }
    }
}
