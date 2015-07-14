using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using One.Net.BLL.Model.Web;

namespace One.Net.BLL.Web
{
    public abstract class MExternalAdmin : System.Web.UI.UserControl
    {
        public event EventHandler SetMessage;
        public event EventHandler SetException;
        public event EventHandler SetWarning;

        protected void OnSetWarning(AdminExternalEventArgs e)
        {
            if (SetWarning != null)
            {
                SetWarning(this, e);
            }
        }

        protected void OnSetMessage(AdminExternalEventArgs e)
        {
            if (SetMessage != null)
            {
                SetMessage(this, e);
            }
        }

        protected void OnSetException(AdminExternalEventArgs e)
        {
            if (SetException != null)
            {
                SetException(this, e);
            }
        }  

        protected static int SelectedLanguageId
        {
            get { return Thread.CurrentThread.CurrentCulture.LCID; }
        }
    }

    public class AdminExternalEventArgs : EventArgs
    {
        private Exception exc = new Exception();
        private string message = "";
        private string warning = "";

        public AdminExternalEventArgs(Exception exc, string message)
        {
            this.exc = exc;
            this.message = message;
        }

        public string Message { get { return message; } set { message = value; } }
        public string Warning { get { return warning; } set { warning = value; } }
        public Exception Exc { get { return exc; } set { exc = value; } }
    }
}
