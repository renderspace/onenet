using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.IO;
using System.Net;



namespace OneMainWeb.AdminControls
{
    public partial class TextContentControl : System.Web.UI.UserControl
    {
        private bool useCkEditor;

        public TextBoxMode TitleTextMode { get { return TextBoxTitle.TextMode; } set { TextBoxTitle.TextMode = value; } }
        public int TitleTextRows { get { return TextBoxTitle.Rows; } set { TextBoxTitle.Rows = value; } }

        public Unit HtmlHeight { get { return TextBoxHtml.Height; } set { TextBoxHtml.Height = value; } }
        public int HtmlRows { get { return TextBoxHtml.Rows; } set { TextBoxHtml.Rows = value; } }

        public bool UseCkEditor
        {
            get { return useCkEditor; }
            set {
                useCkEditor = value;
                TextBoxHtml.CssClass = useCkEditor ? "form-control ckeditor4" : "form-control";
            }
        }

        public bool SubTitleVisible { get { return TextBoxSubTitle.Visible; } set { TextBoxSubTitle.Visible = value; } }
        public bool TitleVisible { get { return TextBoxTitle.Visible; } set { TextBoxTitle.Visible = value; } }
        public bool TeaserVisible { get { return TextBoxTeaser.Visible; } set { TextBoxTeaser.Visible = value; } }
        public bool HtmlVisible
        {
            get { return TextBoxHtml.Visible; }
            set
            {
                TextBoxHtml.Visible = value;
            }
        }
        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Title { get { return TextBoxTitle.Text; } set { TextBoxTitle.Text = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string SubTitle { get { return TextBoxSubTitle.Text; } set { TextBoxSubTitle.Text = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Teaser { get { return TextBoxTeaser.Text; } set { TextBoxTeaser.Text = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Html
        {
            get { return TextBoxHtml.Text; }
            set { TextBoxHtml.Text = value; }
            /*
            get { return (useFckEditor ? fckHtml.Value : TextBoxHtml.Value); } 
            set { fckHtml.Value = TextBoxHtml.Value = value; }*/
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HtmlHeight == Unit.Pixel(0))
            {
                HtmlHeight = Unit.Pixel(400);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine();
            writer.WriteBeginTag(HtmlTextWriterTag.Div.ToString());
            writer.WriteAttribute(HtmlTextWriterAttribute.Class.ToString(), "full");
            writer.WriteAttribute(HtmlTextWriterAttribute.Id.ToString(), this.ClientID);
            writer.WriteAttribute(HtmlTextWriterAttribute.Name.ToString(), this.UniqueID);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Indent++;
            this.RenderChildren(writer);
            writer.Indent--;
            writer.WriteLine();
            writer.WriteEndTag(HtmlTextWriterTag.Div.ToString());
        }

        #region State methods

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object[] cSThis = new object[2];
            object cSBase = base.SaveControlState();
            cSThis[0] = cSBase;
            cSThis[1] = useCkEditor;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            useCkEditor = (bool)cSThis[1];
            base.LoadControlState(cSBase);
        }

        #endregion State methods

    }
}