using System;

namespace One.Net.BLL
{
	[Serializable]
    public class BOIntContImage : BOIntCont
    {
        public string Src { get; set; }

        public virtual string Alt { get; set; }

        public int FileID { get; set; }

        public string FileName { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string FullUri { get; set; }

	    public BOIntContImage() { }

        public BOIntContImage(string html) : base(html) { }
    }
}
