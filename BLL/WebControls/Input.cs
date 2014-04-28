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
    [ToolboxData("<{0}:Input runat=\"server\"></{0}:Input>")]
    public class Input : CompositeControl
    {
        protected TextBox inp = new TextBox();
        protected RequiredFieldValidator inputRequired = new RequiredFieldValidator();

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
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TextBoxCssClass
        {
            get
            {
                String s = (String)ViewState["TextBoxCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["TextBoxCssClass"] = value;
            }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Value
        {
            get { return this.inp.Text;}
            set { this.inp.Text = value.ToString(); }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public override short TabIndex
        {
            get { return this.inp.TabIndex; }
            set { this.inp.TabIndex = value; }
        }

        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Gets or sets the maximum number of characters in textbox")]
        public int MaxLength
        {

            get { return this.inp.MaxLength; }
            set { this.inp.MaxLength = value; }
        }


        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(TextBoxMode.SingleLine)]
        [Description("Is this field MutliLine?")]
        public TextBoxMode TextMode
        {
            
            get { return this.inp.TextMode;}
            set { this.inp.TextMode = value; }
        }

        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Is this field ReadOnly?")]
        public bool ReadOnly
        {

            get { return this.inp.ReadOnly; }
            set { this.inp.ReadOnly = value; }
        }

        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(1)]
        [Description("If multiline, how many rows?")]
        public int Rows
        {
            get { return this.inp.Rows; }
            set { this.inp.Rows = value; }
        }

        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Is this field Required?")]
        public bool Required
        {
            get
            {
                return inputRequired.Enabled;
            }
            set
            {
                inputRequired.Enabled = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("Date is required")]
        [Description("Message to display if textbox is left blank")]
        public string RequiredMessage
        {
            get
            {
                return inputRequired.ErrorMessage;
            }
            set
            {
                inputRequired.ErrorMessage = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Validation Group the control belongs to")]
        public virtual string ValidationGroup
        {
            get
            {
                return inputRequired.ValidationGroup;
            }
            set
            {
                inputRequired.ValidationGroup = value;
            }
        }

        public Input()
        {
            inp.ID = "inp";
        }

        protected override void Render(HtmlTextWriter writer)
        {
            inp.CssClass = TextBoxCssClass;

            writer.WriteLine();
            writer.WriteBeginTag("div");
            if (string.IsNullOrEmpty(ContainerCssClass))
            {
                writer.WriteAttribute("class", "input");
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
            
            Label lab1 = new Label();
            lab1.Text = Text;
            lab1.AssociatedControlID = inp.ID;

            if (inputRequired.ErrorMessage == string.Empty)
            {
                string errmsg = "Field is required";
                inputRequired.ErrorMessage = errmsg;
            }

            inputRequired.ID = "inputRequired";
            inputRequired.ControlToValidate = inp.ID;
            inputRequired.Text = "*";
            inputRequired.CssClass = "required";

            
            
            this.Controls.Add(lab1);
            this.Controls.Add(inp);
            this.Controls.Add(inputRequired);
        }
    }
}
