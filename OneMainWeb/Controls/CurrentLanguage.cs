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
    [ToolboxData("<{0}:CurrentLanguage runat=server></{0}:CurrentLanguage>")]
    public class CurrentLanguage : WebControl
    {
        public WebSiteDisplayField DisplayFieldCurrent { get; set; }

        protected readonly static BWebsite webSiteB = new BWebsite();
        private IEnumerable<BOWebSite> WebSiteList;

        public CurrentLanguage()
        {
            base.EnableViewState = false;
        }

        protected override void CreateChildControls()
        {
            WebSiteList = webSiteB.List();

            base.CreateChildControls();
        }

        private BOWebSite CurrentWebSite
        {
            get
            {
                if (WebSiteList != null)
                {
                    foreach (var webSite in WebSiteList)
                    {
                        if (webSite.Language.LCID == Thread.CurrentThread.CurrentCulture.LCID)
                            return webSite;
                    }
                }
                return null;
            }
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("\n");
            writer.Indent = 0;
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(CssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.RenderBeginTag("span");

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "current");
            writer.RenderBeginTag("span");
            
            LanguageSelector.RenderDisplayField(CurrentWebSite, writer, DisplayFieldCurrent);
            writer.RenderEndTag();

            writer.RenderBeginTag("ul");
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

    }
}