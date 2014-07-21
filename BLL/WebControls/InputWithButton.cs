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
    [ToolboxData("<{0}:InputWithButton runat=server></{0}:InputWithButton>")]
    public class InputWithButton : CompositeControl, IButtonControl
    {
        protected TextBox inp = new TextBox();
        protected LinkButton linkButton = new LinkButton();
        protected Button button = new Button();
        protected RequiredFieldValidator inputRequired = new RequiredFieldValidator();
        protected RegularExpressionValidator inputRegexValidator = new RegularExpressionValidator();

        public const string EMAIL_REGEX =
            @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3} \.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";

        protected string text;
        protected string commandArgument;
        protected string commandName;
        protected string postBackUrl;
        protected string validationGroup;
        protected bool causesValidation;
        protected string buttonText;
        protected bool isLinkButton;
        protected string validationType = "";
        protected string buttonCssClass;


        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool IsLinkButton
        {
            get { return isLinkButton; }
            set { isLinkButton = value; }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Value
        {
            get { return this.inp.Text; }
            set { this.inp.Text = value.ToString(); }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ValidationType
        {
            get { return validationType; }
            set { validationType = value; }
        }

        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ButtonCssClass
        {
            get { return buttonCssClass; }
            set { buttonCssClass = value; }
        }

        

        protected override void Render(HtmlTextWriter writer)
        {

            writer.WriteLine();
            writer.WriteBeginTag("div");
            writer.WriteAttribute("class", "inputwithbutton");
            writer.WriteAttribute("id", this.ClientID);
            //writer.WriteAttribute("name", this.UniqueID);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Indent++;
            RenderContents(writer);
            writer.Indent--;
            writer.WriteLine();
            writer.WriteEndTag("div");
        }

        protected override void CreateChildControls()
        {
            inp.ID = "intp1";
            inp.CssClass = "text";
            Label lab1 = new Label();
            lab1.Text = Text;
            lab1.AssociatedControlID = inp.ID;
            button.CssClass = buttonCssClass;
            button.Text = buttonText;
            linkButton.Text = buttonText;
            linkButton.CssClass = buttonCssClass;
            
            button.ValidationGroup = ValidationGroup;
            linkButton.ValidationGroup = ValidationGroup;
            inp.ValidationGroup = ValidationGroup;

            button.CommandName = commandName;
            linkButton.CommandName = commandName;

            button.CommandArgument = commandArgument;
            linkButton.CommandArgument = commandArgument;

            button.CausesValidation = causesValidation;
            linkButton.CausesValidation = causesValidation;

            bool disableValidation = true;
            if (ValidationType.Length > 0)
            {
                inputRegexValidator.ID = "inputValid";
                inputRegexValidator.ControlToValidate = inp.ID;
                inputRegexValidator.Text = "x";
                inputRegexValidator.CssClass = "valid";
                inputRegexValidator.ValidationGroup = ValidationGroup;
                disableValidation = false;
                switch (ValidationType)
                {
                    case "email":
                        inputRegexValidator.ValidationExpression = EMAIL_REGEX;
                        break;
                    default:
                        disableValidation = true;
                        break;
                }
            }

            if (inputRequired.ErrorMessage == "")
            {
                string errmsg = "*";
                inputRequired.ErrorMessage = errmsg;
            }

            inputRequired.ID = "inputRequired";
            inputRequired.ControlToValidate = inp.ID;
            inputRequired.Enabled = true;
//            inputRequired.Text = "*";
            inputRequired.CssClass = "required";
            inputRequired.ValidationGroup = ValidationGroup;
            this.Controls.Add(inputRequired);
            this.Controls.Add(lab1);
            this.Controls.Add(inp);
            
            if (IsLinkButton)
            {
                this.Controls.Add(linkButton);
                SetDefaultButton(inp, linkButton);
            }
            else 
            {
                this.Controls.Add(button);
                SetDefaultButton(inp, button);
            }

            if (!disableValidation)
            {
                this.Controls.Add(inputRegexValidator);
            }
        }

        private void InitializeControls()
        {
            button.Click += Click;
            button.Command += Command;
            linkButton.Click += Click;
            linkButton.Command += Command;
        }

        #region Control's State Management

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            InitializeControls();
            base.OnInit(e);
        }


        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[10];
            cSThis[0] = cSBase;
            cSThis[1] = text;
            cSThis[2] = commandArgument;
            cSThis[3] = commandName;
            cSThis[4] = postBackUrl;
            cSThis[5] = validationGroup;
            cSThis[6] = causesValidation;
            cSThis[7] = buttonText;
            cSThis[8] = validationType;
            cSThis[9] = buttonCssClass;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];

            text = (string)cSThis[1];
            commandArgument = (string)cSThis[2];
            commandName = (string)cSThis[3];
            postBackUrl = (string)cSThis[4];
            validationGroup = (string)cSThis[5];
            causesValidation = (bool)cSThis[6];
            buttonText = (string)cSThis[7];
            validationType = (string)cSThis[8];
            buttonCssClass = (string)cSThis[9];
            base.LoadControlState(cSBase);
        }

        #endregion Control's State Management

        #region IButtonControl Members

        public bool CausesValidation
        {
            get { return causesValidation; }
            set { causesValidation = value; }
        }

        public event EventHandler Click;

        public event CommandEventHandler Command;

        public string CommandArgument
        {
            get { return commandArgument; }
            set {
                commandArgument = value;
            }
        }

        public string CommandName
        {
            get { return commandName; }
            set { 
                commandName = value;
            }
        }

        public string PostBackUrl
        {
            get { return postBackUrl; }
            set { postBackUrl = value; }
        }

        public string ValidationGroup
        {
            get { return validationGroup; }
            set
            {
                validationGroup = value;
                this.inp.ValidationGroup = value;
            }
        }

        #endregion

        public void SetDefaultButton(TextBox textControl, Control defaultButton)
        {
            textControl.Attributes.Add("onkeydown", "fnTrapKD('" + defaultButton.ClientID + "',event)");
        }
    }
}
