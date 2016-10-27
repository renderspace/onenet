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
using System.IO;
using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb
{
    public partial class External : OneBasePage
    {
        protected const string R_MODULE = "module";

        protected override void SavePageStateToPersistenceMedium(object viewState)
        {
            base.SavePageStateToPersistenceMedium(viewState);

            LosFormatter format = new LosFormatter();

            StringWriter writer = new StringWriter();

            format.Serialize(writer, viewState);

            int viewStateLength = writer.ToString().Length;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request[R_MODULE] != null)
            {
                ContentPlaceHolder contentPlaceHolder = (ContentPlaceHolder) Master.FindControl("MainContent");

                string controlName = Request[R_MODULE].ToString();
                Control control = null;
                string relPath = "~/AdminExtensions/" + controlName + ".ascx";

                if (!string.IsNullOrEmpty(relPath) && File.Exists(HttpContext.Current.Server.MapPath(relPath)))
                {
                    try
                    {
                        control = LoadControl(relPath);

                        if (control is MExternalAdmin)
                        {
                            ((MExternalAdmin)control).SetMessage += new EventHandler(External_SetMessage);
                            ((MExternalAdmin)control).SetException += new EventHandler(External_SetException);
                            ((MExternalAdmin)control).SetWarning += new EventHandler(External_SetWarning);
                        }
                    }
                    catch (Exception ex)
                    {
                        Notifier1.ExceptionName = ex.GetType().Name;
                        Notifier1.ExceptionMessage = ex.Message;
                    }

                    if (control != null)
                    {
                        contentPlaceHolder.Controls.Add(control);
                    }
                    else
                    {
                        Notifier1.Warning = "Failed to load module";
                    }
                }
                else
                {
                    Notifier1.Warning = "Module does not exist";
                }
            }
            else
            {
                Notifier1.Warning = "Missing module parameter";
            }
        }

        void External_SetException(object sender, EventArgs e)
        {
            AdminExternalEventArgs ae = e as AdminExternalEventArgs;
            if (ae != null)
            {
                Notifier1.ExceptionName = "$error";
                Notifier1.ExceptionMessage = ae.Exc.Message;
            }
        }

        void External_SetMessage(object sender, EventArgs e)
        {
            AdminExternalEventArgs ae = e as AdminExternalEventArgs;
            if (ae != null)
            {
                Notifier1.Message = ae.Message;
            }
        }

        void External_SetWarning(object sender, EventArgs e)
        {
            AdminExternalEventArgs ae = e as AdminExternalEventArgs;
            if (ae != null)
            {
                Notifier1.Warning = ae.Message;
            }
        }
    }
}
