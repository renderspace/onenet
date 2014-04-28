using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;

using System.Diagnostics;
using log4net;

namespace OneMainWeb.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BreadCrumb runat=server></{0}:BreadCrumb>")]
    public class BreadCrumb : WebControl, INamingContainer
    {
        private readonly ILog log = LogManager.GetLogger("OneMainWeb.Controls.MenuGroup");

        private bool controlsAdded = false;

        //  Template Declarations
        private int _minDepth;
        private int _maxDepth;
        private string _separator;

        public int MaxDepth
        {
            get { return _maxDepth; }
            set { _maxDepth = value; }
        }

        public int MinDepth
        {
            get { return _minDepth; }
            set { _minDepth = value; }
        }

        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }
        
        public BreadCrumb()
        {
            base.EnableViewState = false;
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            if (controlsAdded)
            {
                writer.Write("\n");
                writer.Indent = 0;
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (controlsAdded)
            {
                base.RenderEndTag(writer);
            }
        }

        protected override void CreateChildControls()
        {
            var _selectedNode = SiteMap.CurrentNode;

            if (_selectedNode != null)
            {
                int preControlsCount = this.Controls.Count;
                RecursiveCreateChildControls(SiteMap.CurrentNode);
                int postControlsCount = this.Controls.Count;

                if (postControlsCount > preControlsCount)
                    controlsAdded = true;
            }
            Enabled = Visible = controlsAdded;
        }

        private void RecursiveCreateChildControls(SiteMapNode node)
        {
            var depth = Int32.Parse(node["_absDepth"]);

            if (depth >= MinDepth && depth <= MaxDepth)
            {
                var item = new Literal();
                var sep = new Literal();

                sep.Text = Separator;

                var cssClass = " l" + depth + " p" + node["_pageID"];

                if (SiteMap.CurrentNode["_pageID"] == node["_pageID"])
                    item.Text = node.Title;
                else
                {
                    item.Text = "<a href=\"" + node.Url + "\" class=\"" + cssClass + "\">" + node.Title + "</a>";
                    Controls.AddAt(0, sep);
                }
                Controls.AddAt(0, item);

            }

            if (SiteMap.RootNode["_pageID"] == node["_pageID"])
                return;
            else
                RecursiveCreateChildControls(node.ParentNode);
        }
    }
}