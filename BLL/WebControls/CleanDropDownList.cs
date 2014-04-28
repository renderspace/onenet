using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace One.Net.BLL.WebControls
{
    /// <summary>
    /// Summary description for OneDropDownList.
    /// </summary>
    public class CleanDropDownList : System.Web.UI.WebControls.DropDownList
    {
        override protected void Render(HtmlTextWriter writer)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            System.IO.StringWriter stringWriter = new System.IO.StringWriter(stringBuilder);
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);

            // write the contents of the base class (RadioButton),
            // to this custom htmlwriter
            base.Render(htmlWriter);

            // replace what we need, dont need from it
            stringBuilder.Replace("<span class=\"" + this.CssClass + "\">", "");
            stringBuilder.Replace("</span>", "");

            string test = stringBuilder.ToString();
            // then output that as output of this control
            writer.Write(stringBuilder.ToString());

        }
    }
}
