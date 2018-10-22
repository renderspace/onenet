using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Threading;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;
using One.Net.BLL.WebControls;

namespace OneMainWeb.CommonModules
{
    public partial class SimpleSearch : MModule
    {
        [Setting(SettingType.String, DefaultValue = "")]
        public string SearchResultsUri { get { return GetStringSetting("SearchResultsUri"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["q"] != null)
            {
                var searchTerm = Request["q"].ToString();
                var uri = "/AdminService/SearchPageContent?keyword=" + searchTerm + "&languageId=" + Thread.CurrentThread.CurrentCulture.LCID;

                var client = new WebClient();
                string response = client.DownloadString(uri);

                Response.Write(response);
                Response.End();
            }
        }
    }
}