using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;
using System.Threading;
using System.Diagnostics;

using NLog;
using One.Net.BLL;

namespace OneMainWeb.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:LanguageSelector runat=server></{0}:LanguageSelector>")]
    public class LanguageSelector : WebControl
    {
        protected readonly static BWebsite webSiteB = new BWebsite();
        protected static Logger log = LogManager.GetCurrentClassLogger();
        private bool controlsRendered = false;

        public int Group { get; set; }

        public LanguageSelector()
        {
            base.EnableViewState = false;
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("\n");
            writer.Indent = 0;
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.RenderBeginTag("nav");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "current");
            writer.RenderBeginTag("span");
            writer.Write(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
            writer.RenderEndTag();
            writer.RenderBeginTag("ul");
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected static bool PublishFlag
        {
            get { return PresentBasePage.ReadPublishFlag(); }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            var list = webSiteB.List();

            foreach (var website in list)
            {
                if (website.WebSiteGroup == Group && Group > -1)
                {
                    var websiteUrl = PublishFlag ? website.ProductionUrl : website.PreviewUrl;

                    if (!string.IsNullOrEmpty(websiteUrl))
                    {
                        var cssClass = "lang-" + website.Languge.TwoLetterISOLanguageName;
                        if (Thread.CurrentThread.CurrentCulture.LCID == website.Languge.LCID)
                            cssClass += " current";

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
                        writer.RenderBeginTag("li");

                        writer.AddAttribute(HtmlTextWriterAttribute.Href, websiteUrl);
                        writer.AddAttribute(HtmlTextWriterAttribute.Title, website.Title);
                        writer.RenderBeginTag("a");
                        writer.Write(website.Title);
                        writer.RenderEndTag();

                        writer.RenderEndTag();
                    }
                }
            }

            base.RenderContents(writer);
        }

    }
}