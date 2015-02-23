using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.AdminControls
{
    public partial class TextContentModal : System.Web.UI.UserControl
    {
        public string SelectedLanguage
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.EnglishName;
            }
        }

        public string SelectedLanguageId 
        { 
            get 
            { 
                return Thread.CurrentThread.CurrentCulture.LCID.ToString(); 
            }
        }

        public bool EnableCkHtml
        {
            get;
            set;
        }

        public string Title { get; set; }
             

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}