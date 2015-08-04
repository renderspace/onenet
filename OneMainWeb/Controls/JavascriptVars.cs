using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using System.Threading;

namespace OneMainWeb.Controls 
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:JavascriptVars runat=server></{0}:JavascriptVars>")]
    public class JavascriptVars : WebControl
    {
        protected override void Render(HtmlTextWriter output)
        {
            var publishFlag = PresentBasePage.ReadPublishFlag();

            output.Write("<script>");
            output.Write("websiteLanguageId = " + Thread.CurrentThread.CurrentCulture.LCID.ToString() + ";");
            output.Write("websiteTwoLetterISOLanguageName = '" + Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName + "';");
            output.Write("tracing = " + (publishFlag ? "false" : "true") + ";");
            output.Write(@"
            function trace(msg) {
    if (typeof tracing == 'undefined' || !tracing) return;
    try { console.log(msg); } catch (ex) { }
}
            ");
            output.Write("</script>");
        }
    
    }
}