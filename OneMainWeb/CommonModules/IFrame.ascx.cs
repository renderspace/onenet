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
using TwoControlsLibrary;

namespace OneMainWeb.CommonModules
{
    public partial class IFrame : MModule
    {
        protected string Src { 
            get {
                if (PassGetParameters && !string.IsNullOrEmpty(GetStringSetting("Src")))
                {
                    var builder1 = new UrlBuilder(GetStringSetting("Src"));
                    var builder2 = new UrlBuilder(Page);
                    foreach (string key in builder2.QueryString.Keys)
                        builder1.QueryString[key] = builder2.QueryString[key];
                    return builder1.ToString();
                }
                else
                    return GetStringSetting("Src"); 
            } 
        }

        protected int Width { get { return GetIntegerSetting("Width"); } }
        protected int Height { get { return GetIntegerSetting("Height"); } }
        public bool PassGetParameters { get { return GetBooleanSetting("PassGetParameters"); } }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}