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
    [ToolboxData("<{0}:ValidInput runat=\"server\"></{0}:ValidInput>")]
    public class ValidInput : Input
    {
        protected RegularExpressionValidator inputRegexValidator  = new RegularExpressionValidator();

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("")]
        public string ValidationType
        {
            get
            {
                return (ViewState["validationType"] != null ? ViewState["validationType"].ToString() : string.Empty);
            }
            set
            {
                ViewState["validationType"] = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Validation Group the control belongs to")]
        public sealed override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                inputRegexValidator.ValidationGroup = value;
                base.ValidationGroup = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Error message shown by validator")]
        public string ErrorMessage
        {
            get
            {
                return inputRegexValidator.ErrorMessage;
            }
            set
            {
                inputRegexValidator.ErrorMessage = value;
            }
        }

        protected override void CreateChildControls()
        {
            inputRegexValidator.ID = "inputValid" + base.inp.ID;
            inputRegexValidator.ControlToValidate = base.inp.ID;
            inputRegexValidator.Text = "x";
            inputRegexValidator.CssClass = "valid";
            inputRegexValidator.ValidationGroup = this.ValidationGroup;

            bool disableValidation = false;

            switch (ValidationType)
            {
                case "time":
                    inputRegexValidator.ValidationExpression = @"\d{1,2}:\d{1,2}(:\d{1,2}|$)";
                    break;
                case "date":
                    inputRegexValidator.ValidationExpression = @"\d{1,2}\.\d{1,2}\.\d{4}( \d{1,2}:\d{1,2}(:\d{1,2}|$)|$)";
                    break;
                case "alphanumeric":
                    inputRegexValidator.ValidationExpression = @"^[\p{L}\p{Zs}\p{Lu}\p{Ll}]{1,255}$";
                    break;
                case "numeric":
                    inputRegexValidator.ValidationExpression = @"^(\-|\+)?\d+((\.|\,)?\d+)?$";
                    break;
                case "integer":
                    inputRegexValidator.ValidationExpression = @"^(\-|\+)?\d+$"; // optional + or - in front
                    break;
                case "telephone":
                    inputRegexValidator.ValidationExpression = @"^(\+)?[\d\- \(\)]+$"; // optional + in front
                    break;
                case "float":
                    inputRegexValidator.ValidationExpression = @"^(\-|\+)?\d+((\.|\,)?\d+)?$"; // optional + or - in front, following by digits followed by options , or . with digits
                    break;
                case "email":
                    inputRegexValidator.ValidationExpression = @"^(([\w\-\.]+@([A-Za-z0-9]([\w\-])+\.){1,2}([a-zA-Z]([\w\-]){1,3}));*)+";
                    break;
                case "sloVAT":
                    // serverside validation required
                    inputRegexValidator.ValidationExpression = @"^(si)?[0-9]{8}$";
                    break;
                case "TRR":  
                    // serverside validation required
                    inputRegexValidator.ValidationExpression = @"^[0-9]{5}\-[0-9]{10}$";
                    break;
                case "EMSO":
                    // serverside validation required
                    inputRegexValidator.ValidationExpression = @"^[0-9]{13}$";
                    break;
                default:
                    disableValidation = true;
                    break;
            }

            base.CreateChildControls();

            if (!disableValidation)
            {
                this.Controls.Add(inputRegexValidator);
            }
        }
    }
}
