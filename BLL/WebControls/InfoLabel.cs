using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections.Specialized;

namespace One.Net.BLL.WebControls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:InfoLabel runat=server></{0}:InfoLabel>")]
    public class InfoLabel : Control
    {
        Label lab1 = new Label();
        Label lab2 = new Label();

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true), Description("Label text")]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true), Description("Info text")]
        public string Value
        {
            get
            {
                String s = (String)ViewState["Value"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Value"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ContainerCssClass
        {
            get
            {
                String s = (String)ViewState["ContainerCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["ContainerCssClass"] = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {

            writer.WriteLine();
            writer.WriteBeginTag("div");
            if (string.IsNullOrEmpty(ContainerCssClass))
            {
                writer.WriteAttribute("class", "infolabel");
            }
            else
            {
                writer.WriteAttribute("class", ContainerCssClass);
            }
            writer.WriteAttribute("id", this.ClientID);
            writer.WriteAttribute(HtmlTextWriterAttribute.Name.ToString(), this.UniqueID);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Indent++;
            RenderChildren(writer);
            writer.Indent--;
            writer.WriteLine();
            writer.WriteEndTag("div");
        }

        protected override void CreateChildControls()
        {
            lab1.CssClass = "infolabel";
            lab1.Text = this.Text;
            this.Controls.Add(lab1);
            lab2.CssClass = "info";
            lab2.Text = this.Value;
            this.Controls.Add(lab2);
        }
    }
}