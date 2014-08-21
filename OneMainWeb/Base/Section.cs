using System;
using System.Collections;
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
            if (!string.IsNullOrWhiteSpace(CustomClientID))
            {
                writer.AddAttribute("id", CustomClientID);
            }
            writer.AddAttribute("class", this.CssClass);
            IEnumerator keys = Attributes.Keys.GetEnumerator();

            while (keys.MoveNext())
            {
                String key = (String)keys.Current;
                writer.AddAttribute(key, Attributes[key]);
            }
            writer.RenderBeginTag("section");
        }

        public string CustomClientID
        {
            get;
            set;
        }
    }
}