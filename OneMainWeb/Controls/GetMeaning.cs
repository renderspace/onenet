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
                return ((s == null) ? "" : s);
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


        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool PlainHtml
        {
            get
            {
                if (ViewState["PlainHtml"] == null)
                    return false;
                return (bool)ViewState["PlainHtml"];
            }

            set
            {
                ViewState["PlainHtml"] = value;
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (PlainHtml)
            {
                var meaning = BContent.GetComplexMeaning(Keyword);
                if (meaning != null)
                {
                    if (meaning.MissingTranslation)
                    {
                        output.Write(Keyword);
                    }
                    else
                    {
                        output.Write(meaning.Html.Trim().Replace("\n", "").Replace("\r", ""));
                    }
                }
                    
                return;
            } 
            
            if (!Complex)
                output.Write(BContent.GetMeaning(Keyword).Trim());
            else
                output.Write(BContent.GetComplexMeaningRendered(Keyword));
        }
    }
}
