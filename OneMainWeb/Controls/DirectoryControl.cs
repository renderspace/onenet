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
#warning DEPRECEATED.. check and remove
    [DefaultProperty("")]
    [ToolboxData("<{0}:DirectoryControl runat=server></{0}:DirectoryControl>")]
    public class DirectoryControl : WebControl 
    {
        private readonly ILog log = LogManager.GetLogger("OneMainWeb.Controls.DirectoryControl");

        private int _minDepth;
        private int _maxDepth;

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

        protected override void OnInit(System.EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[3];
            cSThis[0] = cSBase;
            cSThis[1] = _minDepth;
            cSThis[2] = _maxDepth;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            _minDepth = (int)cSThis[1];
            _maxDepth = (int)cSThis[2];
            base.LoadControlState(cSBase);
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        protected override void CreateChildControls()
        {
            SiteMapNodeCollection coll = new SiteMapNodeCollection(SiteMap.RootNode);
            RecursiveCreateChildControls((IHierarchicalEnumerable)coll);
        }

        private void RecursiveCreateChildControls(IHierarchicalEnumerable dataItems)
        {
            bool headerRendered = false;

            int? depth = null;
            foreach (SiteMapNode dataItem in dataItems)
            {
                if (!depth.HasValue)
                {
                    depth = Int32.Parse(dataItem["_absDepth"]);
                }

                if (this._maxDepth < depth)
                {
                    break;
                }
                else
                {
                    string url = dataItem.Url;
                    string title = dataItem.Title;
                    bool dataItemHasChildren = dataItem.HasChildNodes;

                    string cssClass = "";
                    string linkCssClass = "";

                    IHierarchyData data = dataItems.GetHierarchyData(dataItem);

                    if (url == SiteMap.CurrentNode.Url)
                    {
                        cssClass = " sel";
                        linkCssClass = " asel";
                    }

                    if (depth < this._minDepth)
                    {
                        if (dataItemHasChildren)
                        {
                            //  Create any child items
                            RecursiveCreateChildControls(data.GetChildren());
                        }
                    }
                    else
                    {
                        cssClass = cssClass.Trim();
                        linkCssClass = linkCssClass.Trim();

                        if (!headerRendered)
                        {
                            //  Render the header only once <ul>
                            Literal header = new Literal();
                            header.Text = "<ul>";
                            Controls.Add(header);
                            headerRendered = true;
                        }

                        //  Create an item header <li>
                        Literal itemHeader = new Literal();
                        itemHeader.Text = "<li class=\"" + cssClass + "\">";
                        Controls.Add(itemHeader);

                        //  Create the data item
                        Literal item = new Literal();
                        item.Text = "<a href=\"" + url + "\" class=\"" + linkCssClass + "\">" + title + "</a>";
                        Controls.Add(item);

                        if (depth.Value < this._maxDepth && dataItemHasChildren)
                        {
                            //  Create any child items
                            RecursiveCreateChildControls(data.GetChildren());
                        }

                        //  Create the item footer </li>
                        Literal itemFooter = new Literal();
                        itemFooter.Text = "</li>";
                        Controls.Add(itemFooter);
                    }
                }
            }

            //  If we had a header, then render out the footer </ul>
            if (headerRendered)
            {
                Literal footer = new Literal();
                footer.Text = "</ul>";
                Controls.Add(footer);
            }
        }
    }
}

