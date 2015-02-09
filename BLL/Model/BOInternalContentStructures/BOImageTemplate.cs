using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Collections.Specialized;
using System.Web;
using One.Net.BLL.Utility;

namespace One.Net.BLL
{
    [Serializable]
    public class BOImageTemplate : BOTemplate
    {
        private string bgColor;

        public string BgColor
        {
            get { return bgColor; }
            set { bgColor = value; }
        }

        private int width;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int max;

        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        private int quality;
        public int Quality
        {
            get { return quality; }
            set { quality = value; }
        }

        private int overlayImageId;

        public int OverlayImageId
        {
            get { return overlayImageId; }
            set { overlayImageId = value; }
        }

        private int overlayMode;

        public int OverlayMode
        {
            get { return overlayMode; }
            set { overlayMode = value; }
        }

        private bool aspectRatioSize;

        public bool AspectRatioSize
        {
            get { return aspectRatioSize; }
            set { aspectRatioSize = value; }
        }

        [NonSerialized]
        readonly LoosyFormatter serializer = new LoosyFormatter();

        private static string EncodeQueryString(NameValueCollection col)
        {
            if (!col.HasKeys())
            {
                return "";
            }

            int count = col.Count;

            string[] keys = col.AllKeys;
            string[] values = new string[count];
            col.CopyTo(values, 0);

            string[] pairs = new string[count];
            for (int i = 0; i < count; i++)
            {
                pairs[i] = string.Concat(keys[i], "=", values[i]);
            }

            return "?" + string.Join("&", pairs);
        }

        public string RenderImageLink(string link)
        {
            string linkWithoutQuery = link;

            int querySeparatorOccurance = link.IndexOf("?");
            if (querySeparatorOccurance > -1)
                linkWithoutQuery = link.Substring(0, querySeparatorOccurance);

            NameValueCollection queryString = new NameValueCollection();

            string[] splitLink = link.Split(new char[] { '?' });
            if (splitLink.Length > 1)
            {
                string query = HttpUtility.UrlDecode(splitLink[1]);
                query = query.Replace("&amp;", "&");
                string[] pairs = query.Split(new char[] { '&' });
                foreach (string s in pairs)
                {
                    string[] pair = s.Split(new char[] { '=' });
                    if (pair.Length > 1)
                    {
                        queryString[pair[0]] = pair[1];
                    }
                }
            }
            if (this.Width > 0)
                queryString["w"] = this.Width.ToString();
            if (this.Height > 0)
                queryString["h"] = this.Height.ToString();
            if (this.Max > 0)
                queryString["m"] = this.Max.ToString();
            if (this.AspectRatioSize)
                queryString["ars"] = "1";
            if (!string.IsNullOrEmpty(BgColor))
                queryString["c"] = BgColor;
            if (Quality > 0 && Quality <= 100)
                queryString["q"] = Quality.ToString();

            if (this.OverlayImageId > 0)
            {
                queryString["o"] = OverlayImageId.ToString();
                queryString["om"] = OverlayMode.ToString();
            }
            return linkWithoutQuery + EncodeQueryString(queryString);
        }


        public string RenderHtml(string alt, string link, string cssClass, string rel = "")
        {
            if (string.IsNullOrWhiteSpace(link))
                return "";
            string result = "<img src=\"" + RenderImageLink(link) + "\" " + (!string.IsNullOrWhiteSpace(rel) ? ("rel=\"" + rel + "\"") : "");
            result += (!string.IsNullOrEmpty(alt.Trim())) ?
                            (" alt=\"" + HttpUtility.HtmlEncode(alt) + "\"" +
                            " title=\"" + HttpUtility.HtmlEncode(alt)) + "\"" : "";
            if (string.IsNullOrWhiteSpace(cssClass))
                result += " />";
            else
                result += " class=\"" + cssClass + "\" />";
            return result;
        }

        public override string ToString()
        {
            return 
                "ImageTemplate W:" + Width + " H:" + Height + " O:" + OverlayImageId;
        }
    }
}
