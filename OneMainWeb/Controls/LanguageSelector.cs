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
    /// <summary>
    /// ****** IMPORTANT NOTE!!! ******
    /// What is INamingContainer?
    /// INamingContainer is a marker interface, meaning it has no methods to implement. 
    /// A control "implements" this interface to let the framework know that it plans on giving it's child controls really specific IDs. 
    /// It's important to the framework, because if two instances of the same control are on the same page, 
    /// and the control gives its child controls some specific ID, there'd end up being multiple controls with the same ID on the page, 
    /// which is going to cause problems. 
    /// So when a Control is a naming container, the UniqueID for all controls within it will have the parent's ID as a prefix. 
    /// This scopes it to that control. So while a child control might have ID "foo", its UniqueID will be "parentID$foo" (where parentID = the ID of the parent). 
    /// Now even if this control exists twice on the page, everyone will still have a UniqueID.
    /// INamingContainer also has the property that any controls within it that do not have a specific ID will have its ID automatically determined based on a counter that is scoped to it. 
    /// So if there were two naming containers, foo and bar, they might have child controls with UniqueIDs foo$ctl01 and bar$ctl01. 
    /// Each naming container gets its own little counter.
    /// ****** END IMPORTANT NOTE ******
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:LanguageSelector runat=server></{0}:LanguageSelector>")]
    public class LanguageSelector : WebControl, INamingContainer // HierarchicalDataBoundControl
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
            if (controlsRendered)
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
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (controlsRendered)
            {
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        protected static bool PublishFlag
        {
            get { return PresentBasePage.ReadPublishFlag(); }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var list = webSiteB.List();

            foreach (var website in list)
            {
                if (website.WebSiteGroup == Group && Group > -1)
                {
                    var websiteUrl = "";
                    if (PublishFlag)
                        websiteUrl = website.ProductionUrl;
                    else
                        websiteUrl = website.PreviewUrl;

                    if (string.IsNullOrEmpty(websiteUrl))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "lang-" + website.Languge.TwoLetterISOLanguageName);
                        writer.RenderBeginTag("li");

                        writer.AddAttribute(HtmlTextWriterAttribute.Href, websiteUrl);
                        writer.AddAttribute(HtmlTextWriterAttribute.Title, website.Title);
                        writer.RenderBeginTag("a");
                        writer.Write(website.Title);
                        writer.RenderEndTag();

                        writer.RenderEndTag();

                        controlsRendered = true;
                    }
                }
            }
            
            base.Render(writer);
        }

        private static string CreateIndentTabs(int indentLevel)
        {
            return new string('\t', indentLevel);
        }
    }
}