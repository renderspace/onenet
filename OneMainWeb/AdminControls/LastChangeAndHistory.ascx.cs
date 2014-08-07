using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.AdminControls
{
    public partial class LastChangeAndHistory : System.Web.UI.UserControl
    {

        public string Text
        {
            get { return LabelChanged.Text;  }
            set { LabelChanged.Text = value; }
        }

        public int SelectedContentId
        {
            get { return ViewState["SelectedContentId"] != null ? (int)ViewState["SelectedContentId"] : 0; }
            set { ViewState["SelectedContentId"] = value; }
        }

        public int SelectedLanguageId
        {
            get { return ViewState["SelectedLanguageId"] != null ? (int)ViewState["SelectedLanguageId"] : 0; }
            set { ViewState["SelectedLanguageId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}