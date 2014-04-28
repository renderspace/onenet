using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Specialized;
using One.Net.BLL.Utility;
namespace One.Net.BLL
{
    [Serializable]
    public class BOInternalContentAudit : BOInternalContent
    {
        private string auditGuid;

        public string AuditGuid { get { return auditGuid; } set { auditGuid = value; } }
    }

	[Serializable]
	public class BOInternalContent : IContent
	{
        #region Member variables

        private const string NO_TRANSLATION_TAG = "[No translation] ";

        private const string prefix = "/_files";
        // string below is also used (copied) to TByNumberPathProvider. If you change it here, make sure, you change it there, too.
        public const string fileMatcher = prefix + @"/([0-9]{1,6})/([a-zA-Z0-9čćđšž_\-\s\!]{1,255}\.[a-zA-Z0-9_]{2,5})";

        protected const string FILE_ACCESSOR = "_files";
        public const string SETTING_CONTENT_ID = "ContentId";

        protected string teaser = string.Empty;
        protected string html = string.Empty;
        protected string title = string.Empty;
        protected string subtitle = string.Empty;

	    private bool missingTranslation = false;

        protected DateTime? dateModified;
        protected string principalModified;

        protected DateTime dateCreated;
        protected string principalCreated;

		protected List<BOIntContImage> images;
		protected List<BOIntContLink> links;
        protected List<BOIntContFlash> flashObjects;

        #endregion

        # region Simple fields

	    public int Votes { get; set;}
        public double? Score { get; set; }

        public DateTime? LastChanged
        {
            get { return DateModified ?? DateCreated; }
        }

        public string LastChangedBy
        {
            get { return (DateModified == null) ? PrincipalCreated : PrincipalModified; }
        }

        public DateTime? DateModified { get; set;}
        public string PrincipalModified { get; set; }
        public DateTime DateCreated { get; set; }
        public string PrincipalCreated { get; set; }
        public int? ContentId { get; set; }
        public int LanguageId { get; set; }
        public bool RequiredSWFObject { get { return FlashObjects.Count > 0; } }

        public string Title
        {
            get
            {
                return MissingTranslation ? NO_TRANSLATION_TAG + title : title;
            }
            set
            {
                if (value.Length > 4000)
                {
                    throw new InvalidOperationException("Title is limited to 4000 characters");
                }
                else
                {
                    title = value;
                }
            }
        }

        public string SubTitle
        {
            get { return subtitle; }
            set
            {
                if (value == null)
                    subtitle = "";
                else if (value.Length > 255)
                    throw new InvalidOperationException("Subtitle is limited to 255 characters");
                else
                    subtitle = value;
            }
        }

        #endregion
        
        public string Teaser
        {
            get { return teaser; }
            set
            {
                teaser = value;
                BuildInternalStructures(teaser);
            }
        }

        public string ProcessedTeaser
        {
            get
            {
                string processedString = Process(teaser);
                //ProcessPopUp(ref processedString);

                for (int i = 0; i < this.FlashObjects.Count; i++)
                {
                    BOIntContFlash flash = this.FlashObjects[i];
                    if (flash.Width > 0 && flash.Height > 0)
                        processedString = processedString.Replace(flash.WholeHtml, flash.RenderHtml(ContentId, i));
                }

                return processedString;
            }
        }

        public string HtmlText
        {
            get
            {
                var text = Html;

                if (this.Images != null)
                {
                    foreach (BOIntContImage image in this.Images)
                    {
                        text = text.Replace(image.WholeHtml, "");
                    }
                }

                if (this.FlashObjects != null)
                {
                    foreach (BOIntContFlash flash in this.FlashObjects)
                    {
                        text = text.Replace(flash.WholeHtml, "");
                    }
                }

                return text;
            }
        }

        public string TeaserText
        {
            get
            {
                var text = Teaser;

                if (this.Images != null)
                {
                    foreach (BOIntContImage image in this.Images)
                    {
                        text = text.Replace(image.WholeHtml, "");
                    }
                }

                if (this.FlashObjects != null)
                {
                    foreach (BOIntContFlash flash in this.FlashObjects)
                    {
                        text = text.Replace(flash.WholeHtml, "");
                    }
                }

                return text;
            }
        }


        public string Html
        {
            get { return html; }
            set
            {
                html = value;
                BuildInternalStructures(html);
            }
        }

        public string ProcessedHtml
        {
            get
            {
                string processedString = Process(html);

                foreach (BOIntContImage image in this.RemoveImages)
                {
                    processedString = processedString.Replace(image.WholeHtml, "");
                    this.Images.Remove(image);
                }

                if (ImageTemplate != null)
                {
                    foreach (BOIntContImage image in this.Images)
                    {
                        string newHtml = ImageTemplate.RenderHtml(image.Width, image.Height, 0, image.Alt,
                            image.FullUri, image.CssClass);
                        processedString = processedString.Replace(image.WholeHtml, newHtml);
                    }
                }

                for (int i = 0; i < this.FlashObjects.Count; i++)
                {
                    BOIntContFlash flash = this.FlashObjects[i];
                    if ( flash.Width > 0 && flash.Height > 0)
                        processedString = processedString.Replace(flash.WholeHtml, flash.RenderHtml(ContentId, i));
                }

                //ProcessPopUp(ref processedString);
                
                return processedString;
            }
        }

	    public bool HasTranslationInCurrentLanguage
        {
            get { return IsComplete; }
        }

        public bool IsComplete
        {
            get { return (!MissingTranslation && title != null && subtitle != null && teaser != null && html != null && LanguageId > 0); }
        }

	    public bool IsRated
	    {
	        get { return Score.HasValue; }
	    }

        public List<BOIntContFlash> FlashObjects
        {
            get { return flashObjects; }
        }

        public BOImageTemplate ImageTemplate { get; set;}
        
        public List<BOIntContImage> Images
        {
			get { return images; }
        }

        public List<BOIntContImage> RemoveImages { get; set; }
        
        public List<BOIntContLink> Links
        {
			get { return links; }
        }

	    public bool MissingTranslation
	    {
	        get { return missingTranslation; }
	        set { missingTranslation = value; }
	    }

	    public BOInternalContent()
	    {
	        images = new List<BOIntContImage>();
            RemoveImages = new List<BOIntContImage>();
		    links = new List<BOIntContLink>();
            flashObjects = new List<BOIntContFlash>();
	    }

        public BOInternalContent(BOInternalContent content)
        {
            ContentId = content.ContentId;
            Title = content.Title;
            SubTitle = content.SubTitle;
            Teaser = content.Teaser;
            Html = content.Html;
            LanguageId = content.LanguageId;
            PrincipalCreated = content.PrincipalCreated;
            DateCreated = content.DateCreated;
            PrincipalModified = content.PrincipalModified;
            DateModified = content.DateModified;
        }

        public BOInternalContent(int id, int languageId, string title, string subtitle, string teaser, string html, string createdBy, DateTime created)
        {
            ContentId = id;
            Title = title;
            SubTitle = subtitle;
            Teaser = teaser;
            Html = html;
            LanguageId = languageId;
            PrincipalCreated = createdBy;
            DateCreated = created;
        }

        public void LoadContent(BOInternalContent content)
        {
            if (content != null)
            {
                ContentId = content.ContentId;
                Title = content.Title.Replace(NO_TRANSLATION_TAG, "");
                SubTitle = content.SubTitle;
                Teaser = content.Teaser;
                Html = content.Html;
                LanguageId = content.LanguageId;
                PrincipalCreated = content.PrincipalCreated;
                DateCreated = content.DateCreated;
                PrincipalModified = content.PrincipalModified;
                DateModified = content.DateModified;
                MissingTranslation = content.MissingTranslation;
            }
        }

        public void CloneContent(BOInternalContent clone)
        {
            if (null == clone)
                clone = new BOInternalContent();

            clone.ContentId = this.ContentId;
            clone.Title = this.Title;
            clone.SubTitle = this.SubTitle;
            clone.LanguageId = this.LanguageId;
            clone.PrincipalCreated = this.PrincipalCreated;
            clone.DateCreated = this.DateCreated;
            clone.DateModified = this.DateModified;
            clone.PrincipalModified = this.PrincipalModified;
            // these two do processing
            if (Teaser != null && Html != null)
            {
                clone.Teaser = this.Teaser;
                clone.Html = this.Html;
            }
        }

	    private static string Process(string input)
        {
            return input.Replace("target=\"_blank\"", "onclick=\"window.open(this.href); return false;\"");
        }

        private void BuildInternalStructures(string input)
        {
            string tagRegex = "<{1,1}(\\w+\\b)(([ ]*(\\w+)[ ]*=[ ]*\\\"[^<>\\\"]*?\\\")+)[ ]*/*>{1,1}";
            string attrRegex = "(\\w+)[ ]*=[ ]*\\\"([^<>\\\"]+?)\\\"[ ]*";

            Regex tagFinder = new Regex(tagRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex attrFinder = new Regex(attrRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            MatchCollection matches = tagFinder.Matches(input);
            foreach (Match match in matches)
            {
                string origString = match.Groups[0].Value;
                string tagName = match.Groups[1].ToString().Trim();
                string attributes = match.Groups[2].ToString();
                MatchCollection attrMatches = attrFinder.Matches(attributes);

                switch (tagName.ToLower())
                {
                    case "img":
                        BOIntContImage image = new BOIntContImage(origString);
                        foreach (Match attrMatch in attrMatches)
                        {
                            string attrName = attrMatch.Groups[1].Value;
                            string attrValue = attrMatch.Groups[2].Value;
                            int temp = 0;
                            switch (attrName.ToLower())
                            {
                                case "src":
                                    image.FullUri = attrValue;
#warning Do we really need these below?
                                    if (attrValue.Contains(FILE_ACCESSOR))
                                    {
                                        image.FileID = GetFileId(attrValue);
                                        image.Src = "";
                                        image.IsInternal = true;
                                        image.FileName = GetFileName(attrValue);
                                    }
                                    else
                                    {
                                        image.Src = attrValue;
                                        image.IsInternal = false;
                                    }
                                    break;
                                case "alt":
                                    image.Alt = attrValue;
                                    break;
                                case "class":
                                    image.CssClass = attrValue;
                                    break;
                                case "height":
                                    int.TryParse(attrValue, out temp);
                                    image.Height = temp;
                                    break;
                                case "width":
                                    int.TryParse(attrValue, out temp);
                                    image.Width = temp;
                                    break;
                            }
                        }
                        images.Add(image);
                        break;
                    case "a":
                        BOIntContLink link = new BOIntContLink(origString);
                        foreach (Match attrMatch in attrMatches)
                        {
                            string attrName = attrMatch.Groups[1].Value;
                            string attrValue = attrMatch.Groups[2].Value;
                            switch (attrName.ToLower())
                            {
                                case "href":
                                    if (attrValue.Contains(FILE_ACCESSOR))
                                    {
                                        link.Link = GetFileId(attrValue).ToString();
                                        link.IsInternal = true;
                                    }
                                    else
                                    {
                                        link.Link = attrValue;
                                        link.IsInternal = false;
                                    }
                                    break;
                                case "target":
                                    link.Target = attrValue;
                                    break;
                                case "onclick":
                                    if (attrValue.Contains("window.open(this.href)"))
                                    {
                                        link.Target = "_blank";
                                    }
                                    break;
                                case "class":
                                    link.CssClass = attrValue;
                                    break;
                            }
                        }
                        links.Add(link);
                        break;
                }

                var list = FindFlashObjects(input);
                foreach (BOIntContFlash fl in list)
                    flashObjects.Add(fl);
            }
        }

        private static List<BOIntContFlash> FindFlashObjects(string html)
        {
            List<BOIntContFlash> list = new List<BOIntContFlash>();

            var objectTags = new Stack<string>();

            BOIntContFlash currentFlash = null;
            int startIndex = 0;

            // Traverse entire HTML
            for (int i = 0; i < html.Length; i++)
            {
                bool isClose;
                bool isSolo;

//                int endIndex = 0;

                char c = html[i];
                if (c == '<')
                {
                    string attributes;

                    // Look ahead at this tag
                    string tag = Validator.LookAhead(html, i, out isClose, out isSolo, out attributes);

                    if (tag.ToLower() == "object")
                    {
                        if (!isSolo)
                        {
                            if (isClose)
                            {
                                // Remove the start tag from the stack
                                objectTags.Pop();

                                if (objectTags.Count == 0 && currentFlash != null)
                                {
                                    if (currentFlash != null)
                                    {
                                        currentFlash.WholeHtml = html.Substring(startIndex, (i + tag.Length + 3) - startIndex); // the 3 is for the </ and > of a closing tag
                                        list.Add(currentFlash);
                                    }
                                    currentFlash = null;
                                }
                            }
                            else
                            {
                                currentFlash = new BOIntContFlash();
                                startIndex = i;

                                string strW = StringTool.GetHtmlAttributeValue(attributes, "width");

                                if (strW.Contains("%"))
                                {
                                    currentFlash.UnitIsPixel = false;
                                    strW = strW.Replace("%", "");
                                }
                                else if (strW.Contains("px"))
                                {
                                    currentFlash.UnitIsPixel = true;
                                    strW = strW.Replace("px", "");
                                }
                                else
                                    currentFlash.UnitIsPixel = true;

                                int w = 0;
                                Int32.TryParse(strW, out w);

                                currentFlash.Width = w;

                                string strH = StringTool.GetHtmlAttributeValue(attributes, "height");

                                if (strH.Contains("%"))
                                    strH = strH.Replace("%", "");
                                else if (strH.Contains("px"))
                                    strH = strH.Replace("px", "");

                                int h = 0;
                                Int32.TryParse(strH, out h);   
                                
                                currentFlash.Height = h;

                                currentFlash.Id = StringTool.GetHtmlAttributeValue(attributes, "id");
                                objectTags.Push(tag);
                            }
                        }
                    }
                    else if (tag.ToLower() == "param" && objectTags.Count > 0 && currentFlash != null)
                    {
                        string name = StringTool.GetHtmlAttributeValue(attributes, "name");
                        string value = StringTool.GetHtmlAttributeValue(attributes, "value");

                        switch (name.ToLower())
                        {
                            case "flashvars":
                                currentFlash.FlashVars = new NameValueCollection();
                                List<string> keyvalues = StringTool.SplitString(System.Web.HttpUtility.HtmlDecode(value.Trim()), '&');
                                foreach (string keyvalue in keyvalues)
                                {
                                    if (!string.IsNullOrEmpty(keyvalue.Trim()))
                                        currentFlash.FlashVars.Add(BOIntContFlash.GetFlashVarKey(keyvalue), BOIntContFlash.GetFlashVarValue(keyvalue));
                                }
                                break;
                            case "movie":
                                currentFlash.Movie = value;
                                break;
                            default:
                                if (currentFlash.Parameters == null)
                                    currentFlash.Parameters = new NameValueCollection();
                                currentFlash.Parameters.Add(name, value);
                                break;
                        }
                    }
                    else if (tag.ToLower() == "embed")
                    {
                        if (isClose)
                        {
                            if (objectTags.Count == 0)
                            {
                                if (currentFlash != null)
                                {
                                    currentFlash.WholeHtml = html.Substring(startIndex, (i + tag.Length + 3) - startIndex);
                                    list.Add(currentFlash);
                                }
                                currentFlash = null;
                            }
                        }
                        else
                        {
                            if (currentFlash == null)
                            {
                                currentFlash = new BOIntContFlash();
                                startIndex = i - tag.Length;
                                if (startIndex < 0)
                                    startIndex = 0;
                            }

                            string flashvars = StringTool.GetHtmlAttributeValue(attributes, "flashvars");

                            currentFlash.FlashVars = new NameValueCollection();
                            List<string> keyvalues = StringTool.SplitString(System.Web.HttpUtility.HtmlDecode(flashvars.Trim()), '&');
                            foreach (string keyvalue in keyvalues)
                            {
                                if (!string.IsNullOrEmpty(keyvalue.Trim()))
                                    currentFlash.FlashVars.Add(BOIntContFlash.GetFlashVarKey(keyvalue), BOIntContFlash.GetFlashVarValue(keyvalue));
                            }

                            if (string.IsNullOrEmpty(currentFlash.Movie))
                                currentFlash.Movie = StringTool.GetHtmlAttributeValue(attributes, "src");

                            string strW = StringTool.GetHtmlAttributeValue(attributes, "width");

                            if (strW.Contains("%"))
                            {
                                currentFlash.UnitIsPixel = false;
                                strW = strW.Replace("%", "");
                            }
                            else if (strW.Contains("px"))
                            {
                                currentFlash.UnitIsPixel = true;
                                strW = strW.Replace("px", "");
                            }
                            else
                                currentFlash.UnitIsPixel = true;

                            int w = 0;
                            Int32.TryParse(strW, out w);

                            currentFlash.Width = w;

                            string strH = StringTool.GetHtmlAttributeValue(attributes, "height");

                            if (strH.Contains("%"))
                                strH = strH.Replace("%", "");
                            else if (strH.Contains("px"))
                                strH = strH.Replace("px", "");

                            int h = 0;
                            Int32.TryParse(strH, out h);

                            currentFlash.Height = h;
                        }
                    }
                }
            }

            return list;
        }

        private static int GetFileId(string attributeValue)
        {
            int fileId = -1;
            try
            {
                Regex reg = new Regex(fileMatcher);
                Match matched = reg.Match(attributeValue);
                fileId = int.Parse(matched.Groups[1].Value);
            }
            catch
            {
            }
            return fileId;
        }

        private static string GetFileName(string attributeValue)
        {
            string fileName = string.Empty;
            try
            {
                Regex reg = new Regex(fileMatcher);
                Match matched = reg.Match(attributeValue);
                fileName = matched.Groups[2].Value;
            }
            catch
            {
            }
            return fileName;
        }
	
    }
}

