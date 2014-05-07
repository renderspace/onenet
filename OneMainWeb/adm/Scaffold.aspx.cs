using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.adm
{
    public partial class Scaffold : OneBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MultiView1.ActiveViewIndex = PhysicalSchema.CheckDatabaseConfiguration() ? 0 : 1;
            }
        }

        protected void ButtonCreateTables_Click(object sender, EventArgs e)
        {
            List<string> errors;

            PhysicalSchema.CreateConfiguration(out errors);

            if (true)
            {
                Response.Redirect("/adm/ScaffoldConfig.aspx");
            }
        }
    }
}