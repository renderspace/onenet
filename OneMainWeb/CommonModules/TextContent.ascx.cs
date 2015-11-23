using System;
using System.Data;
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
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class TextContent : MModule
    {
        private static readonly BTextContent textContentB = new BTextContent();
        BOInternalContent textContent;

        [Setting(SettingType.ImageTemplate, DefaultValue = "-1")]
        public BOImageTemplate ImageTemplate { get { return GetImageTemplate("ImageTemplate"); } }

        [Setting(SettingType.Int, DefaultValue="-1", Visibility=SettingVisibility.SPECIAL )]
        public int ContentId
        {
            get
            {
                if (textContent == null || !textContent.ContentId.HasValue)
                {
                    return -1;
                }

                return textContent.ContentId.Value;
            }
        }
        
        protected override void OnLoad(EventArgs e)
        {
            textContent = textContentB.GetTextContent(InstanceId);

            if (null != textContent)
            {
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
                    output.WriteBeginTag("h2");
                    output.WriteAttribute("class", "st");
                    output.Write(HtmlTextWriter.TagRightChar);
                    output.Write(textContent.Title);
                    output.WriteEndTag("h1");
                }

                if (textContent.SubTitle.Length > 0)
                {
                    output.WriteBeginTag("h3");
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
    }
}