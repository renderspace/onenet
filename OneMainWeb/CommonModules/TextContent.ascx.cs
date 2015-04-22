using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using One.Net.BLL;

using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class TextContent : MModule, IImageListProvider
    {
        private static readonly BTextContent textContentB = new BTextContent();
        BOInternalContent textContent;

        protected bool EnableImageProvider { get { return GetBooleanSetting("EnableImageProvider"); } }
        public bool EnableCommentProvider { get { return GetBooleanSetting("EnableCommentProvider"); } }

        protected BOImageTemplate ImageTemplate { get { return GetImageTemplate("ImageTemplate"); } }
        

        /// <summary>
        /// Provides a list of all images displayed by this module (if EnableImageProvider is enabled). 
        /// Availible after Load event.
        /// </summary>
        public List<BOIntContImage> ListImages 
        {
            get 
            {
                if (textContent != null && EnableImageProvider)
                {
                    return textContent.Images;
                }
                else { return null; }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            textContent = textContentB.GetTextContent(InstanceId);

            if (null != textContent)
            {
                if (EnableImageProvider)
                {
                    foreach (BOIntContImage image in textContent.Images)
                    {
                        textContent.RemoveImages.Add(image);
                    }
                }

                if (ImageTemplate != null)
                    textContent.ImageTemplate = ImageTemplate;
            }
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (textContent != null && textContent.IsComplete)
            {
                if (textContent.Title.Length > 0)
                {
                    output.WriteBeginTag("h1");
                    output.WriteAttribute("class", "st");
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContent.Title);
                    output.WriteEndTag("h1");
                }

                if (textContent.SubTitle.Length > 0)
                {
                    output.WriteBeginTag("h2");
                    output.WriteAttribute("class", "st");
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContent.SubTitle);
                    output.WriteEndTag("h2");
                }

                if (textContent.Teaser.Length > 0)
                {
                    output.WriteBeginTag("div");
                    output.WriteAttribute("class", "contentTeaser", true);
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContent.ProcessedTeaser);
                    output.WriteEndTag("div");
                }

			    if (textContent.ProcessedHtml.Length > 0)
			    {
                    output.WriteBeginTag("div");
                    output.WriteAttribute("class", "contentHtml", true);
                    output.Write(HtmlTextWriter.TagRightChar);
				    output.Write(textContent.ProcessedHtml);
				    output.WriteEndTag("div");
			    }
            }
        }

        public int? ContentId
        {
            get 
            {
                if (textContent == null)
                { 
                    return null; 
                }

                return textContent.ContentId;
            }
        }
    }
}