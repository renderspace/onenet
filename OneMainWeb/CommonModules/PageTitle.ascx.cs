using One.Net.BLL;
using One.Net.BLL.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.CommonModules
{
    public partial class PageTitle : MModule
    {
        private static BWebsite websiteB = new BWebsite();
        protected void Page_Load(object sender, EventArgs e)
        {
            var page = websiteB.GetPage(PageId);
            LiteralPageTitle.Text = page.Title;
        }
    }
}