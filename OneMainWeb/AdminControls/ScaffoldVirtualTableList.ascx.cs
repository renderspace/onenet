using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Scaffold;
using System.Configuration;

namespace OneMainWeb.AdminControls
{
    public partial class ScaffoldVirtualTableList : System.Web.UI.UserControl
    {
        protected readonly static BWebsite webSiteB = new BWebsite();

        private static int ReadWebSiteId()
        {
            var websiteId = 0;
            int.TryParse(ConfigurationManager.AppSettings["WebSiteId"], out websiteId);
            return websiteId;
        }

        private static BOWebSite CurrentWebSite
        {
            get;
            set;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PhysicalSchema.CheckDatabaseConfiguration())
                {
                    CurrentWebSite = webSiteB.Get(ReadWebSiteId());
                    DataBind_MenuVirtualTables();
                }
            }
        }

        private void DataBind_MenuVirtualTables()
        {
            var virtualTables = Schema.ListVirtualTables().Where(t => t.ShowOnMenu && t.GroupValues.Contains(CurrentWebSite.WebSiteGroup));
            RepeaterTables.DataSource = virtualTables;
            RepeaterTables.DataBind();
        }
    }
}