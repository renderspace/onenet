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
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1060);
                Button1.Text = BContent.GetMeaning("magic_button");
                Button2.Text = BContent.GetMeaning("magic_button2");
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
