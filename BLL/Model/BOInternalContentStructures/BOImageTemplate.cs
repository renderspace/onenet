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
        private int? imagePopupWidth;
        private int? imagePopupHeight;

        private int? width;
        private int? max;

        private bool isLightBox, enableCaption;
        private bool enableImageLink;
        private string bgColor;

        public string BgColor 
        {
            get { return bgColor; }
            set { bgColor = value; }
        }

        public int? Width
        {
            get { return width; }
            set { width = value; }
        }

        public int? Max
        {
            get { return max; }
            set { max = value; }
        }

        private int? height;

        public int? Height
        {
            get { return height; }
            set { height = value; }
        }

        private int? quality;
        public int? Quality 
        {
            get { return quality; }
            set { quality = value; }
        }

        private int? overlayImageId;

        public int? OverlayImageId
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


        public int? ImagePopupWidth
        {
            get { return imagePopupWidth; }
            set { imagePopupWidth = value; }
        }

        public int? ImagePopupHeight
        {
            get { return imagePopupHeight; }
            set { imagePopupHeight = value; }
        }

        public bool EnableImagePopup
        {
            get { return !IsLightBox && ImagePopupHeight.HasValue && ImagePopupHeight.Value > 0 && ImagePopupWidth.HasValue && ImagePopupWidth.Value > 0; }
        }

        public bool IsLightBox
        {
            get
            {
                return isLightBox /*&& ImagePopupHeight.HasValue && 
                    ImagePopupHeight.Value > 0 && ImagePopupWidth.HasValue && ImagePopupWidth.Value > 0*/;
            }
            set { isLightBox = value; }
        }

        public bool EnableImageLink
        {
            get
            {
                return enableImageLink;
            }
            set { enableImageLink = value; }
        }

        public bool EnableCaption 
        {
            get
            {
                return enableCaption;
            }
            set { enableCaption = value; }
        }

        private bool aspectRatioSize = false;
        public bool AspectRatioSize { get { return aspectRatioSize; } set { aspectRatioSize = value;  } }

        [NonSerialized]
        readonly LoosyFormatter serializer = new LoosyFormatter();

        private static string EncodeQueryString(NameValueCollection col)
        {
            if (!col.HasKeys())
            {
                return string.Empty;
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

        public string RenderHtml(int w, int h, string alt, string link, string cssClass)
        {
            return RenderHtml(w, h, 0, alt, link, cssClass, "");
        }

        public string RenderHtml(int w, int h, int max, string alt, string link, string cssClass)
        { 
            return RenderHtml(w, h, max, alt, link, cssClass, "");
        }

        public string RenderHtml(int w, int h, int max, string alt, string link, string cssClass, string rel)
        {
            string extraAttributes = "";
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

            if (w > 0)
                queryString["w"] = w.ToString();
            if (h > 0)
                queryString["h"] = h.ToString();
            if (max > 0)
                queryString["m"] = max.ToString();

            if (this.Width.HasValue && this.Width > 0)
                queryString["w"] = this.Width.ToString();
            if (this.Height.HasValue && this.Height > 0)
                queryString["h"] = this.Height.ToString();
            if (this.Max.HasValue && this.Max > 0)
                queryString["m"] = this.Max.ToString();

            if (this.AspectRatioSize)
                queryString["ars"] = "1";

            if (!string.IsNullOrEmpty(BgColor))
                queryString["c"] = BgColor;

            if (Quality.HasValue && Quality.Value > 0 && Quality.Value <= 100)
                queryString["q"] = Quality.Value.ToString();


            if (FormatTool.GetInteger(queryString.Get("w")) > 0 && !AspectRatioSize)
                extraAttributes += " width=\"" + queryString["w"] + "\"";
            if (FormatTool.GetInteger(queryString.Get("h")) > 0 && !AspectRatioSize)
                extraAttributes += " height=\"" + queryString["h"] + "\"";

            if (this.OverlayImageId.HasValue && this.OverlayImageId > 0)
            {
                queryString["o"] = OverlayImageId.ToString();
                queryString["om"] = OverlayMode.ToString();
            }

            string result = "<img src=\"" + linkWithoutQuery + EncodeQueryString(queryString) + "\" " + extraAttributes;
            result += (!string.IsNullOrEmpty(alt) && !string.IsNullOrEmpty(alt.Trim())) ?
                            (" alt=\"" + HttpUtility.HtmlEncode(alt) + "\"" +
                            " title=\"" + HttpUtility.HtmlEncode(alt)) + "\"" : "";
            result += " class=\"" + cssClass + "\" />";

            if (IsLightBox)
            {
                NameValueCollection lightBoxQueryString = new NameValueCollection();
                if (ImagePopupWidth > 0)
                    lightBoxQueryString["w"] = ImagePopupWidth.ToString();

                result = "<a rel=\"lightbox\" href = \"" + linkWithoutQuery + EncodeQueryString(lightBoxQueryString) +
                        "\" title=\"" + HttpUtility.HtmlEncode(alt) + "\">" + result + "</a>";
            }
            else if (EnableImagePopup)
            {
                NameValueCollection popUpQueryString = new NameValueCollection();
                popUpQueryString["w"] = (ImagePopupWidth - 20).ToString();

                result = "<a href = \"javascript:void window.open('" + linkWithoutQuery + EncodeQueryString(popUpQueryString) +
                        "', 'popup', 'width=" + ImagePopupWidth + ",height=" + ImagePopupHeight +
                        ",resizable,scrollbars=0,status=0');\" class=\"imagePopUp\">"
                        + result + "</a>";
            }
            else if (EnableImageLink)
            {
                result = "<a class=\"p\" rel=\"" + rel + "\" href=\"" + linkWithoutQuery + "\" title=\"" + alt + "\">" + result + "</a>";
            }

            if (EnableCaption && !string.IsNullOrEmpty(alt) && !string.IsNullOrEmpty(alt.Trim()))
            {
                result += "<span>" + alt + "</span>";
            }


            return result;
        }

        public override string ToString()
        {
            return 
                "BOIntContImageTemplate Width:" + (Width ?? -1) + " Height:" + (Height ??
                -1) + " Overlay:" + (OverlayImageId ?? -1);
        }
    }
}
