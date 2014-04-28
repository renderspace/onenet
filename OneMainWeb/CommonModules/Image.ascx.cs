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
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class Image : MModule
    {
        protected string Href { get { return GetStringSetting("href"); } }
        protected string Src { get { return GetStringSetting("Src"); } }
        protected string Alt { get { return GetStringSetting("Alt"); } }
        protected int Width { get { return GetIntegerSetting("Width"); } }
        protected int Height { get { return GetIntegerSetting("Height"); } }
        protected bool IsLinked { get { return GetStringSetting("href").Length > 0; } }
        protected bool OpenInNewWindow { get { return GetBooleanSetting("OpenInNewWindow"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            LiteralImageOutput.Text = RenderImageTag();
        }

        protected string RenderImageTag()
        {

            string linkTag = "<a href=\"" + Href + "\" ";
            if (OpenInNewWindow && IsLinked)
                linkTag += "onclick=\"window.open(this.href); return false;\"";
            linkTag += ">";

            string imgTag = "<img src=\"" + this.Src + "\"";

            if (Width > 0 && Height > 0)
                imgTag += " width=\"" + Width + "\" height=\"" + Height + "\"";

            imgTag += " alt=\"" + HttpUtility.HtmlEncode(Alt) + "\" ";

            if (OpenInNewWindow && !IsLinked)
                imgTag += "onclick=\"window.open(this.href); return false;\"";

            imgTag += " />";

            if (IsLinked)
                return linkTag + imgTag + "</a>";
            else
                return imgTag;
        }
    }
}