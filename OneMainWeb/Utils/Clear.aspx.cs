using System;
using One.Net.BLL;
using System.Web.Routing;

namespace OneMainWeb.Utils
{
    public partial class Clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            BWebsite webSiteB = new BWebsite();
            webSiteB.ClearCache();
            OneSiteMapProvider.ReloadSiteMap();
            RouteConfig.ReloadRoutes(RouteTable.Routes);
            Label1.Text = "Cache cleared at: " + DateTime.Now;
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            foreach (var key in Request.Cookies.AllKeys)
                Response.Cookies[key].Expires = DateTime.Now.AddDays(-1);
            Label1.Text = "Cookies cleared at: " + DateTime.Now;
        } 
    }
}
