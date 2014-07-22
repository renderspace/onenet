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

        public string TitleLabel { get { return txtTitle.Text; } set { txtTitle.Text = value; } }
        public string SubTitleLabel { get { return txtSubTitle.Text; } set { txtSubTitle.Text = value; } }
        public string TeaserLabel { get { return txtTeaser.Text; } set { txtTeaser.Text = value; } }
        public string HtmlLabel { get { return txtHtml.Text; } set { txtHtml.Text = value; } }

        public TextBoxMode TitleTextMode { get { return txtTitle.TextMode; } set { txtTitle.TextMode = value; } }
        public int TitleTextRows { get { return txtTitle.Rows; } set { txtTitle.Rows = value; } }
        public string TitleContainerCssClass { get { return txtTitle.ContainerCssClass; } set { txtTitle.ContainerCssClass = value; } }

        public Unit HtmlHeight { get { return txtHtml.Height; } set { txtHtml.Height = value; } }
        public int HtmlRows { get { return txtHtml.Rows; } set { txtHtml.Rows = value; } }

        public string TextBoxCssClass
        {
            get { return txtHtml.TextBoxCssClass; }
            set { txtHtml.TextBoxCssClass = value; }
        }

        public bool UseCkEditor
        {
            get { return useCkEditor; }
            set {
                useCkEditor = value;
                txtHtml.TextBoxCssClass = useCkEditor ? "ckeditor" : "" ;
            }
        }

        public bool SubTitleVisible { get { return txtSubTitle.Visible; } set { txtSubTitle.Visible = value; } }
        public bool TitleVisible { get { return txtTitle.Visible; } set { txtTitle.Visible = value; } }
        public bool TeaserVisible { get { return txtTeaser.Visible; } set { txtTeaser.Visible = value; } }
        public bool HtmlVisible
        {
            get { return txtHtml.Visible; }
            set
            {
                txtHtml.Visible = value;
            }
        }
        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Title { get { return txtTitle.Value; } set { txtTitle.Value = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string SubTitle { get { return txtSubTitle.Value; } set { txtSubTitle.Value = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Teaser { get { return txtTeaser.Value; } set { txtTeaser.Value = value; } }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Html
        {
            get { return txtHtml.Value; }
            set { txtHtml.Value = value; }
            /*
            get { return (useFckEditor ? fckHtml.Value : txtHtml.Value); } 
            set { fckHtml.Value = txtHtml.Value = value; }*/
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
            writer.WriteAttribute(HtmlTextWriterAttribute.Class.ToString(), "textcontentcontrol");
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