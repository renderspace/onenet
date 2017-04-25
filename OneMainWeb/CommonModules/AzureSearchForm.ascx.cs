using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;
using One.Net.BLL.WebControls;

namespace OneMainWeb.CommonModules
{
    public partial class AzureSearchForm : MModule
    {
        [Setting(SettingType.String, DefaultValue = "")]
        public string SearchResultsUri { get { return GetStringSetting("SearchResultsUri"); } }
    }
}