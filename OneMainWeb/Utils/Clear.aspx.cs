using System;
using One.Net.BLL;

namespace OneMainWeb.Utils
{
    public partial class Clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Button1.Text = "Clear cache";
                Button2.Text = "Clear cookies";
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            OCache.Clear();
            OneSiteMapProvider.ReloadSiteMap();
            Label1.Text = "Cache cleared at: " + DateTime.Now;
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            foreach (var key in Request.Cookies.AllKeys)
                Response.Cookies[key].Expires = DateTime.Now.AddDays(-1);
            OneSiteMapProvider.ReloadSiteMap();
            Label1.Text = "Cookies cleared at: " + DateTime.Now;
        } 
    }
}
