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
using System.Web.Optimization;

namespace OneMainWeb.Controls
{
    public class BundleRender : WebControl
    {
        [Bindable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Path
        {
            get
            {
                var s = ViewState["Path"] as String;
                return ((s == null) ? "" : s);
            }

            set
            {
                ViewState["Path"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool IsScript
        {
            get
            {
                if (ViewState["IsScript"] == null)
                    return false;
                return (bool)ViewState["IsScript"];
            }

            set
            {
                ViewState["IsScript"] = value;
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (IsScript)
                output.Write(Scripts.Render(Path));
            else
                output.Write(Styles.Render(Path));
        }
    }
}