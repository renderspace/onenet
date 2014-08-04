using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using One.Net.BLL;

namespace OneMainWeb.AdminControls
{
    public partial class Notifier : System.Web.UI.UserControl
    {
        string title = "", message = "", code = "", exceptionName = "", exceptionMessage = "", warning = "";

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string ExceptionName
        {
            get { return exceptionName; }
            set { exceptionName = value; }
        }

        public string ExceptionMessage
        {
            get { return exceptionMessage; }
            set { exceptionMessage = value; }
        }

        public string Warning
        {
            get { return warning; }
            set { warning = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!string.IsNullOrEmpty(ExceptionName))
            {
                PanelNotifierError.Visible = true;
                Label5.Text = ExceptionName;
                Label6.Text = ExceptionMessage;
            }
            if (!string.IsNullOrEmpty(Title))
            {
                PanelNotifierSuccess.Visible = true;
                Label1.Text = Title;
                Label2.Text = Message;
            }
            if (!string.IsNullOrEmpty(Warning))
            {
                PanelNotifierWarning.Visible = true;
                Label3.Text = Warning;
                Label4.Text = Message;
            }
            base.OnPreRender(e);
        }
    }
}