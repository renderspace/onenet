using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.AdminControls
{
    public partial class ScaffoldVirtualTableList : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PhysicalSchema.CheckDatabaseConfiguration())
                {
                    DataBind_MenuVirtualTables();
                }
            }
        }

        private void DataBind_MenuVirtualTables()
        {
            var virtualTables = Schema.ListVirtualTables().Where(t => t.ShowOnMenu);
            RepeaterTables.DataSource = virtualTables;
            RepeaterTables.DataBind();
        }
    }
}