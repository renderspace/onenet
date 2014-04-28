using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace OneMainWeb.Base
{
    public class Section : Panel
    {
        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.AddAttribute("class", this.CssClass);
            writer.RenderBeginTag("section");
        }
    }
}