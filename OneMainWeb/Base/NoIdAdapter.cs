using System.Web.UI;
using System.Web.UI.Adapters;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace OneMainWeb.Base
{
    public class NoIdAdapter : ControlAdapter
    {
        protected override void BeginRender(HtmlTextWriter writer)
        {
            if (Control is Table || !(Control is IPostBackDataHandler || Control is IPostBackEventHandler))
            {
                AttributeCollection attr = null;
                if (Control is WebControl)
                    attr = ((WebControl)Control).Attributes;
                else if (Control is HtmlControl)
                    attr = ((HtmlControl)Control).Attributes;
                if (attr != null)
                {
                    string noId = attr["noid"];
                    if (noId == "true" || noId == "True")
                        Control.ID = null;
                    if (!string.IsNullOrEmpty(noId))
                        attr.Remove("noid");
                }
            }
            base.BeginRender(writer);
        }
    }
}