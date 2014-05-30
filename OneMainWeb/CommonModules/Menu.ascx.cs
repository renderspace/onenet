using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class Menu : MModule
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MenuGroup1.LocalExpand = GetBooleanSetting("LocalExpand");
            MenuGroup1.MinDepth = GetIntegerSetting("MinDepth");
            MenuGroup1.MaxDepth = GetIntegerSetting("MaxDepth");
            MenuGroup1.Group = GetIntegerSetting("Group").ToString();
            MenuGroup1.ExpandToLevel = GetIntegerSetting("ExpandToLevel");
            MenuGroup1.FirstUlClass = GetStringSetting("FirstUlClass");
            MenuGroup1.CssClass = GetStringSetting("CssClass");
            MenuGroup1.ShowDescription = GetBooleanSetting("ShowDescription");
        }
    }
}