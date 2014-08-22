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
            output.Write("websiteLanguageId = " + System.Threading.Thread.CurrentThread.CurrentCulture.LCID.ToString() + ";");
            output.Write("tracing = " + (publishFlag ? "false" : "true") + ";");
            output.Write("</script>");
        }
    
    }
}