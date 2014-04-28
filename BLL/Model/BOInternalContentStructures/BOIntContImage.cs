using System;

namespace One.Net.BLL
{
	[Serializable]
    public class BOIntContImage : BOIntCont
    {
	    string src, fileName;
	    private string fullUri;

        public string Src
        {
            get { return src; }
            set { src = value; }
        }
        string alt;

        public virtual string Alt
        {
            get { return alt; }
            set { alt = value; }
        }

        private int fileId = 0;


        public int FileID
        {
            get { return fileId; }
            set { fileId = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private int width;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

	    public string FullUri
	    {
	        get { return fullUri; }
	        set { fullUri = value; }
	    }

	    public BOIntContImage() { }

        public BOIntContImage(string html) : base(html) { }

        public override string ToString()
        {
            if (src != null && alt != null)
            {
                return "Image\n" + WholeHtml + "\nsrc=" + src + " alt=\"" + alt + "\" FileID=" + FileID.ToString() + " H:" + Height;
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
