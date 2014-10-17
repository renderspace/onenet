using System;

namespace One.Net.BLL
{
	[Serializable]
    public class BOIntContLink : BOIntCont
    {
        public string Link { get; set; }

        public string Target { get; set; }

        public BOIntContLink(string html) : base(html) { }
    }
}
