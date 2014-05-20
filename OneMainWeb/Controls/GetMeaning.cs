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
    [ToolboxData("<{0}:GetMeaning runat=server></{0}:GetMeaning>")]
    public class GetMeaning : WebControl
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Keyword
        {
            get
            {
                String s = (String)ViewState["Keyword"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Keyword"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool Complex
        {
            get
            {
                if (ViewState["Complex"] == null)
                    return false;
                return  (bool)ViewState["Complex"];
            }

            set
            {
                ViewState["Complex"] = value;
            }
        }
        /*
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(Text);
        }*/

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter output)
        {
            if (!Complex)
                output.Write(BContent.GetMeaning(Keyword));
            else
                output.Write(BContent.GetComplexMeaningRendered(Keyword));
        }
    }
}
