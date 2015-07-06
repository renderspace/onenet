using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using One.Net.BLL.Web;
using One.Net.BLL.Utility;
using One.Net.BLL.Model.Attributes;


namespace OneMainWeb.CommonModules
{
    public partial class IFrame : MModule
    {
        [Setting(SettingType.String)]
        public string Src
        { 
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

        [Setting(SettingType.Int, DefaultValue="320")]
        public int Width { get { return GetIntegerSetting("Width"); } }
        [Setting(SettingType.Int, DefaultValue = "240")]
        public int Height { get { return GetIntegerSetting("Height"); } }
        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool PassGetParameters { get { return GetBooleanSetting("PassGetParameters"); } }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}