using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.Utils
{
    public partial class OneClear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var selectedWebSiteId = PresentBasePage.ReadWebSiteId();

            try
            {
                var websiteB = new BWebsite();
                var currentWebsite = websiteB.Get(selectedWebSiteId);

                var publishFlag = PresentBasePage.ReadPublishFlag();

                if (!publishFlag && !string.IsNullOrWhiteSpace(currentWebsite.PreviewUrl))
                {
                    Button1.Text = "Clear cache on preview";
                }
                if (publishFlag && !string.IsNullOrWhiteSpace(currentWebsite.ProductionUrl))
                {
                    Button1.Text = "Clear cache on production";
                }
            }
            catch
            {
                Label1.Text = "Error occured, please check website id and language";
            }
            
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            BWebsite webSiteB = new BWebsite();
            webSiteB.ClearCache();
            OneSiteMapProvider.ReloadSiteMap();
            RouteConfig.ReloadRoutes(RouteTable.Routes);
            Label1.Text = DateTime.Now.ToString();
        }
    }
}