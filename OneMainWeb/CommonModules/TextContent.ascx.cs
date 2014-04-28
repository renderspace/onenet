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
        BOInternalContent textContentModel;

        protected bool EnableImagePopup 
		{ 
			get 
			{ 
				return GetIntegerSetting("ImagePopupWidth") > 0 && GetIntegerSetting("ImagePopupHeight") > 0;
			} 
		}
        protected bool EnableImageProvider { get { return GetBooleanSetting("EnableImageProvider"); } }
        public bool EnableCommentProvider { get { return GetBooleanSetting("EnableCommentProvider"); } }
        

        protected int ImagePopupWidth { get { return GetIntegerSetting("ImagePopupWidth"); } }
        protected int ImagePopupHeight { get { return GetIntegerSetting("ImagePopupHeight"); } }

        protected int ImageTemplate { get { return GetIntegerSetting("ImageTemplate"); } }
        

        /// <summary>
        /// Provides a list of all images displayed by this module (if EnableImageProvider is enabled). 
        /// Availible after Load event.
        /// </summary>
        public List<BOIntContImage> ListImages 
        {
            get 
            {
                if (textContentModel != null && EnableImageProvider)
                {
                    return textContentModel.Images;
                }
                else { return null; }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            textContentModel = textContentB.GetTextContent(InstanceId);

            if (null != textContentModel)
            {
                // if images are supposed to be displayed somewhere else, then hide all.
                if (EnableImageProvider)
                {
                    foreach (BOIntContImage image in textContentModel.Images)
                    {
                        textContentModel.RemoveImages.Add(image);
                    }
                }

                if (ImageTemplate > 0)
                {
                    // load template
                    BWebsite websiteB = new BWebsite();
                    object o = websiteB.GetTemplate(ImageTemplate);
                    if (o is BOImageTemplate)
                        textContentModel.ImageTemplate = (BOImageTemplate) o;
                }
            }
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (textContentModel != null && textContentModel.IsComplete)
            {
                if (textContentModel.Title.Length > 0)
                {
                    output.WriteBeginTag("h1");
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContentModel.Title);
                    output.WriteEndTag("h1");
                }

                if (textContentModel.SubTitle.Length > 0)
                {
                    output.WriteBeginTag("h2");
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContentModel.SubTitle);
                    output.WriteEndTag("h2");
                }

                if (textContentModel.Teaser.Length > 0 && GetBooleanSetting("ShowTeaserText"))
                {
                    output.WriteBeginTag("div");
                    output.WriteAttribute("class", "contentTeaser", true);
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContentModel.ProcessedTeaser);
                    output.WriteEndTag("div");
                }

			    if (textContentModel.ProcessedHtml.Length > 0)
			    {
                    output.WriteBeginTag("div");
                    output.WriteAttribute("class", "contentHtml", true);
                    output.Write(HtmlTextWriter.TagRightChar);
				    output.Write(textContentModel.ProcessedHtml);
				    output.WriteEndTag("div");
			    }
            }
        }

        public int? ContentId
        {
            get 
            {
                if (textContentModel == null)
                { 
                    return null; 
                }

                return textContentModel.ContentId;
            }
        }
    }
}