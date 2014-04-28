using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace One.Net.BLL.WebControls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:LabeledCheckBox runat=\"server\"></{0}:LabeledCheckBox>")]
    public class LabeledCheckBox : CompositeControl
    {
        protected CheckBox chk = new CheckBox();

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
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

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue(false)]
        [Description("Gets or sets a value, indicating whether the control is checked.")]
        public bool Checked
        {

            get { return this.chk.Checked; }
            set { this.chk.Checked = value; }
        }

        public event EventHandler CheckedChanged;

        protected void OnCheckedChanged(object sender, EventArgs e)
        {
            if (CheckedChanged != null)
            {
                CheckedChanged(this, e);
            }
        }

        public bool AutoPostBack
        {
            get { return chk.AutoPostBack; }
            set { chk.AutoPostBack = value; }
        }

        public LabeledCheckBox()
        {
            chk.ID = "chk";
        }

        protected override void Render(HtmlTextWriter writer)
        {

            writer.WriteLine();
            writer.WriteBeginTag("div");
            if (string.IsNullOrEmpty(ContainerCssClass))
            {
                writer.WriteAttribute("class", "checkbox");
            }
            else
            {
                writer.WriteAttribute("class", ContainerCssClass);
            }
            writer.WriteAttribute("id", this.ClientID);
            writer.WriteAttribute(HtmlTextWriterAttribute.Name.ToString(), this.UniqueID);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Indent++;
            RenderContents(writer);
            writer.Indent--;
            writer.WriteLine();
            writer.WriteEndTag("div");
        }

        protected override void CreateChildControls()
        {
            chk.CheckedChanged += new EventHandler(this.OnCheckedChanged);
            Label lab1 = new Label();
            lab1.Text = Text;
            lab1.AssociatedControlID = chk.ID;

            this.Controls.Add(lab1);
            this.Controls.Add(chk);
        }
    }
}
