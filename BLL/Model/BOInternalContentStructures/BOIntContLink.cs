using System;

namespace One.Net.BLL
{
	[Serializable]
    public class BOIntContLink : BOIntCont
    {
        private string link;

        public string Link
        {
            get { return link; }
            set { link = value; }
        }
        private string target = "";

        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        public BOIntContLink(string html) : base(html) { }

        public override string ToString()
        {
            if (Link != null)
            {
                return "Link\n" + WholeHtml + "\n" + Link + "/" + Target;
            }
            else
            {
                return base.ToString();
            }
        }


    }
}
