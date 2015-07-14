using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

using System.Collections.Specialized;

namespace One.Net.BLL.WebControls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DateEntry runat=server></{0}:DateEntry>")]
    public class DateEntry : CompositeControl
    {
        TextBox tb = new TextBox();
        RequiredFieldValidator dateRequired = new RequiredFieldValidator();
        RegularExpressionValidator dateRegex = new RegularExpressionValidator();
        CustomValidator dateParse = new CustomValidator();

        private bool pickWithTime = true;

        private bool showCalendar = false;

        [Bindable(true), Category("Data"), DefaultValue(""), Description("Date value")]
        public bool PickWithTime
        {
            get { return this.pickWithTime; }
            set { this.pickWithTime = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue(""), Description("Date value")]
        public DateTime SelectedDate
        {
            get
            {
                DateTime d;
                try
                {
                    if (!(tb.Text.Length > 0))
                    {
						d = DateTime.MinValue;
                    }
                    else
                    {
						d = DateTime.Parse(tb.Text, Thread.CurrentThread.CurrentUICulture, DateTimeStyles.None);
                    }
                }
                catch
                {
                    d = DateTime.MinValue;
                }
                return d;
            }
            set
            {
				tb.Text = value.ToString("d", Thread.CurrentThread.CurrentUICulture) + " " + value.ToString("HH:mm", Thread.CurrentThread.CurrentUICulture);
            }
        } 

        public bool HasValue
        {
            get { return !SelectedDate.Equals(DateTime.MinValue); }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true), Description("Label text")]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return (s ?? String.Empty);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true), Description("Label text")]
        public bool ShowCalendar
        {
            get
            {
                return showCalendar;
            }

            set
            {
                showCalendar = value;
            }
        }

        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Is this field Required?")]
        public bool Required
        {
            get
            {
                return dateRequired.Enabled;
            }
            set
            {
                dateRequired.Enabled = value;
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
                return dateRequired.ErrorMessage;
            }
            set
            {
                dateRequired.ErrorMessage = value;
            }
        }
        
        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("Date format is incorrect")]
        [Description("Message to display if textbox is left blank")]
        public string FormatMessage
        {
            get
            {
                return dateParse.ErrorMessage;
            }
            set
            {
                dateRegex.ErrorMessage = value;
                dateParse.ErrorMessage = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Validation Group the control belongs to")]
        public string ValidationGroup
        {
            get
            {
                return dateRequired.ValidationGroup;
            }
            set
            {
                dateRequired.ValidationGroup = value;
                dateRegex.ValidationGroup = value;
            }
        }

        #region Control's State Management

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[3];
            cSThis[0] = cSBase;
            cSThis[1] = showCalendar;
            cSThis[2] = pickWithTime;
            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            showCalendar = (bool)cSThis[1];
            pickWithTime = (bool)cSThis[2];
            base.LoadControlState(cSBase);
        }

        #endregion Control's State Management

        public void ClearValue()
        {
            tb.Text = "";
        }


        private void dateParse_ServerValidate(object source, ServerValidateEventArgs e) 
        {
            DateTime d;
            e.IsValid = DateTime.TryParse(e.Value, Thread.CurrentThread.CurrentUICulture, DateTimeStyles.AssumeUniversal, out d);
        }

        protected override void Render(HtmlTextWriter writer)
        {

            writer.WriteLine();
            writer.WriteBeginTag("div");
            writer.WriteAttribute("class", "date");
            writer.WriteAttribute("id", this.ClientID);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Indent++;
            RenderContents(writer);
            writer.Indent--;
            writer.WriteLine();
            writer.WriteEndTag("div");
        }

        protected override void CreateChildControls()
        {
            ImageButton ib = new ImageButton();

            string dateFormat;
            ib.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.ico_calendar.gif");
            ib.Width = new Unit(20);
			ib.Height = new Unit(14);
            tb.ID = "tb1";

            Label lab1 = new Label();
            lab1.Text = Text;
            lab1.AssociatedControlID = tb.ID;
            if (dateRegex.ErrorMessage == string.Empty)
            {
                string errmsg = "Date format is incorrect";
                dateRegex.ErrorMessage = errmsg;
                dateParse.ErrorMessage = errmsg;
            }
            dateRequired.ID = "dateRequired";
            dateRequired.ControlToValidate = tb.ID;
            dateRequired.Text = "*";
            dateRequired.CssClass = "required";
            dateRegex.ID = "dateRegex";
            dateRegex.ControlToValidate = tb.ID;
            dateRegex.Text = "x";
            dateRegex.CssClass = "format";

            switch (Thread.CurrentThread.CurrentUICulture.LCID)
            {
                case 1033:
                {
                    dateFormat = "%m/%d/%Y";
                    dateRegex.ValidationExpression = @"^(([0]?[1-9]|1[0-2])/([0-2]?[0-9]|3[0-1])/[1-2]\d{3})? ?((([0-1]?\d)|(2[0-3])):[0-5]\d)?(:[0-5]\d)? ?(AM|am|PM|pm)?$";
                    break;
                }
                case 1050: case 1060: case 2074: case 5146:
                {
                    dateFormat = "%d.%m.%Y";
                    dateRegex.ValidationExpression = @"\d{1,2}\.\d{1,2}\.\d{4}( \d{1,2}:\d{1,2}(:\d{1,2}|$)|$){0,1}";
                    break;
                }
                default:
                {
                    dateFormat = "%d.%m.%Y";
                    dateRegex.ValidationExpression = @"\d{1,2}\.\d{1,2}\.\d{4}( \d{1,2}:\d{1,2}(:\d{1,2}|$)|$){0,1}";
                    break;
                }
            }
            if (PickWithTime)
                dateFormat += " %H:%M";

            dateParse.ID = "dateParse";
            dateParse.ControlToValidate = tb.ID;
            dateParse.Text = dateRegex.Text;
            dateParse.CssClass = "parse";
            dateParse.ServerValidate += new ServerValidateEventHandler(dateParse_ServerValidate);

            this.Controls.Add(lab1);
            
            if (showCalendar)
            {
                this.Controls.Add(ib);
            }
            this.Controls.Add(tb);
            if (showCalendar)
            {
                ib.Attributes.Add("OnClick", "return showCalendar('" + tb.ClientID + "', '" + dateFormat + "', '24', true, '" + ib.ClientID + "');");
            }
            this.Controls.Add(dateRequired);
            this.Controls.Add(dateRegex);
            this.Controls.Add(dateParse);
        }

        protected override void OnLoad(EventArgs e)
        {
            RegisterCalendarJavascript();
            base.OnLoad(e);
        }

        public void RegisterCalendarJavascript()
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarJs"))
            {
                string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar.js");
                Page.ClientScript.RegisterClientScriptInclude("CalendarJs", scriptUrl); 
            }
            if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarSetup"))
            {
                string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-setup.js");
                Page.ClientScript.RegisterClientScriptInclude("CalendarSetup", scriptUrl);
            }
            switch (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag)
            {
                case "sl-SI":
                    {
                        if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarLang"))
                        {
                            string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-sl.js");
                            Page.ClientScript.RegisterClientScriptInclude("CalendarLang", scriptUrl);
                        }
                        break;
                    }
                case "hr-HR":
                    {
                        if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarLang"))
                        {
                            string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-hr.js");
                            Page.ClientScript.RegisterClientScriptInclude("CalendarLang", scriptUrl);
                        }
                        break;
                    }
                case "sr-latn":
					{
                        if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarLang"))
                        {
                            string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-sr.js");
                            Page.ClientScript.RegisterClientScriptInclude("CalendarLang", scriptUrl);
                        }
                        break;
                    }
				case "bs-Latn-BA":
					{
                        if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarLang"))
                        {
                            string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-ba.js");
                            Page.ClientScript.RegisterClientScriptInclude("CalendarLang", scriptUrl);
                        }
                        break;
                    }
                default:
                    {
                        if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarLang"))
                        {
                            string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "TwoControlsLibrary.Res.calendar-en.js");
                            Page.ClientScript.RegisterClientScriptInclude("CalendarLang", scriptUrl);
                        }
                        break;
                    }
            }
            if (!Page.ClientScript.IsClientScriptBlockRegistered("CalendarFunctions"))
            {
                string theScript = @"<script type=""text/javascript"">
					//<![CDATA[
						function calendarSelected(cal, date) 
						{
							cal.sel.value = date;
							if (cal.dateClicked && (cal.sel.id == 'sel1' || cal.sel.id == 'sel3'))
								cal.callCloseHandler();
						}

						function closeCalendarHandler(cal) 
						{
							cal.hide();
							_dynarch_popupCalendar = null;
						}

						function showCalendar(id, format, showsTime, showsOtherMonths, displayArea) 
						{
							var el = document.getElementById(id);
							if (_dynarch_popupCalendar != null) 
							{
									_dynarch_popupCalendar.hide();
							} else 
							{
									var cal = new Calendar(1, null, calendarSelected, closeCalendarHandler);
									if (typeof showsTime == 'string') 
									{
  										cal.showsTime = true;
  										cal.time24 = (showsTime == '24');
									}
								cal.showsOtherMonths = showsOtherMonths;
									_dynarch_popupCalendar = cal;
									cal.setRange(1900, 2070);
									cal.create();
							}
							_dynarch_popupCalendar.setDateFormat(format);
							_dynarch_popupCalendar.parseDate(el.value);
							_dynarch_popupCalendar.sel = el;
							var displayObj = document.getElementById(displayArea);
							if (displayObj != null)
								_dynarch_popupCalendar.showAtElement(displayObj);
							else
								_dynarch_popupCalendar.showAtElement(el.nextSibling, 'div');
							return false;
						}
						//]]>
					</script>";
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CalendarFunctions", theScript);
            }

        }
    }
}
