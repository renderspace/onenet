using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.DAL;

namespace OneMainWeb.adm
{
    public partial class SQLEditor : OneBasePage
    {
        protected void CmdExec_Click(object sender, EventArgs e)
        {
            try
            {
                sqlGrid.DataSource = DbMisc.ExecuteArbitrarySQL(DropDownConnection.SelectedItem.Value, txtSql.Text);
                sqlGrid.DataBind();
            }
            catch (Exception ex)
            {
                sqlGrid.DataBind();
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "$error";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }

            
        }

        protected void Page_Load(object sender, EventArgs e)
        {
			if (!IsPostBack)
			{
				ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
				if (connections.Count != 0)
				{
					foreach (ConnectionStringSettings connection in connections)
					{
						string name = connection.Name;
						string connectionString = connection.ConnectionString;
						DropDownConnection.Items.Add(new ListItem(name, connectionString));
					}
				}
			}
        }
    }
}
