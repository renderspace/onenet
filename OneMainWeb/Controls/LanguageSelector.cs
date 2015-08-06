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
        public enum WebSiteDisplayField
        {
            TwoLetterCode,
            ThreeLetterCode,
            NativeName,
            EnglishName
        }

        public WebSiteDisplayField DisplayField { get; set; }
        public int Group { get; set; }

        protected readonly static BWebsite webSiteB = new BWebsite();

        public LanguageSelector()
        {
            base.EnableViewState = false;
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("\n");
            writer.Indent = 0;
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(CssClass))
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
                if (website.WebSiteGroup == Group && Group > 0)
                {
                    var websiteUri = PublishFlag ? website.ProductionUrl : website.PreviewUrl;

                    if (!string.IsNullOrEmpty(websiteUri))
                    {
                        var cssClass = "lang-" + website.Language.TwoLetterISOLanguageName;
                        if (Thread.CurrentThread.CurrentCulture.LCID == website.Language.LCID)
                            cssClass += " current";

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
                        writer.RenderBeginTag("li");

                        writer.AddAttribute(HtmlTextWriterAttribute.Href, websiteUri);
                        writer.AddAttribute(HtmlTextWriterAttribute.Title, website.Title);
                        writer.RenderBeginTag("a");

                        switch (DisplayField)
                        {
                            case WebSiteDisplayField.TwoLetterCode :
                                writer.Write(website.Language.TwoLetterISOLanguageName);
                                break;
                            case WebSiteDisplayField.ThreeLetterCode:
                                writer.Write(website.Language.ThreeLetterISOLanguageName);
                                break;
                            case WebSiteDisplayField.NativeName:
                                writer.Write(website.Language.NativeName);
                                break;
                            case WebSiteDisplayField.EnglishName:
                                writer.Write(website.Language.EnglishName);
                                break;
                            default:
                                writer.Write(website.Language.TwoLetterISOLanguageName);
                                break;
                        }
                        
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                    }
                }
            }

            base.RenderContents(writer);
        }

    }
}