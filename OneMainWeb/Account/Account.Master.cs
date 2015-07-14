using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.Account
{
    public partial class Account : System.Web.UI.MasterPage
    {
        protected Version version;

        protected string AppVersion
        {
            get
            {
                if (version == null)
                    version = Page.GetType().BaseType.Assembly.GetName().Version;
                return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision; //+ version.
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}