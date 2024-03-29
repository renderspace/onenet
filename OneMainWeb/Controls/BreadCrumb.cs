﻿using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;

using System.Diagnostics;
using NLog;

namespace OneMainWeb.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BreadCrumb runat=server></{0}:BreadCrumb>")]
    public class BreadCrumb : WebControl, INamingContainer
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private bool controlsAdded = false;

        //  Template Declarations
        private int _minDepth;
        private int _maxDepth;
        private string _separator;
		private string _group;

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
		
        public string Group
        {
            get { return _group; }
            set { _group = value; }
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
                writer.RenderBeginTag(HtmlTextWriterTag.Ol);
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
            var _menuGroup = node["_menuGroup"];

            if (depth >= MinDepth && depth <= MaxDepth && (string.IsNullOrEmpty(Group) || Group == _menuGroup))
            {
                var item = new Literal();
                var sep = new Literal();

                sep.Text = Separator;

                var cssClass = " l" + depth + " p" + node["_pageID"];

                if (SiteMap.CurrentNode["_pageID"] == node["_pageID"])
                    item.Text = "<li class=\"active\">" + node.Title + "</li>";
                else
                {
                    item.Text = "<li><a href=\"" + node.Url + "\" class=\"" + cssClass + "\">" + node.Title + "</a></li>";
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