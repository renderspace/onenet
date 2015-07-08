using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class Menu : MModule
    {
        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool LocalExpand { get { return GetBooleanSetting("ArticleListUri"); } }

        [Setting(SettingType.Int, DefaultValue = "1")]
        public int MinDepth { get { return GetIntegerSetting("MinDepth"); } }

        [Setting(SettingType.Int, DefaultValue = "2")]
        public int MaxDepth { get { return GetIntegerSetting("MaxDepth"); } }

        [Setting(SettingType.Int, DefaultValue = "0")]
        public int Group { get { return GetIntegerSetting("Group"); } }

        [Setting(SettingType.Int, DefaultValue = "2")]
        public int ExpandToLevel { get { return GetIntegerSetting("ExpandToLevel"); } }

        [Setting(SettingType.String, DefaultValue = "sf-menu")]
        public string FirstUlClass { get { return GetStringSetting("FirstUlClass"); } }

        [Setting(SettingType.String, DefaultValue = "navigation")]
        public string CssClass { get { return GetStringSetting("CssClass"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowDescription { get { return GetBooleanSetting("ShowDescription"); } }

        [Setting(SettingType.Int, DefaultValue = "0")]
        public int LeadImageTemplate { get { return GetIntegerSetting("LeadImageTemplate"); } }


        protected void Page_Load(object sender, EventArgs e)
        {
            MenuGroup1.LocalExpand = LocalExpand;
            MenuGroup1.MinDepth = MinDepth;
            MenuGroup1.MaxDepth = MaxDepth;
            MenuGroup1.Group = Group.ToString();
            MenuGroup1.ExpandToLevel = ExpandToLevel;
            MenuGroup1.FirstUlClass = FirstUlClass;
            MenuGroup1.CssClass = CssClass;
            MenuGroup1.ShowDescription = ShowDescription;
            MenuGroup1.LeadImageTemplateId = GetIntegerSetting("LeadImageTemplate");
        }
    }
}