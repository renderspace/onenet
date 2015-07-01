using System;
using One.Net.BLL;
using System.Web.Routing;
using NLog.Internal;

namespace OneMainWeb.Utils
{
    public partial class Clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var selectedWebSiteId = PresentBasePage.ReadWebSiteId();

                var websiteB = new BWebsite();
                var currentWebsite = websiteB.Get(selectedWebSiteId);

                if (!string.IsNullOrWhiteSpace(currentWebsite.PreviewUrl))
                {
                    try
                    {
                        Uri baseUri = new Uri(currentWebsite.PreviewUrl);
                        Uri admUri = new Uri(baseUri, "adm");
                        LiteralCms.Text = "<a href=\"" + admUri.ToString() + "\">Back to CMS..</a>";
                    }
                    catch { }
                }
            }
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
