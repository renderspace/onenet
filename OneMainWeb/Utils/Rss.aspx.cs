using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using One.Net.BLL;

namespace OneMainWeb.Utils
{
    public partial class Rss : System.Web.UI.Page
    {
        private static readonly BRssFeed rssFeedB = new BRssFeed();
        private const string REQUEST_RSS_FEED_ID = "id";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request[REQUEST_RSS_FEED_ID] != null)
            {
                try
                {
                    int feedId = FormatTool.GetInteger(Request[REQUEST_RSS_FEED_ID]);
                    XmlDocument doc = rssFeedB.GetRssChannel(feedId);
                    if (doc != null)
                    {
                        Response.Clear();
                        Response.ContentType = "application/rss+xml";
                        Response.ContentEncoding = Encoding.UTF8;
                        Response.Write(doc.OuterXml.ToString());
                    }
                    else
                    {
                        ShowError(rssFeedB.CreateErrorDocument("Failed to load rss feed " + feedId));
                    }
                }
                catch (Exception ex)
                {
                    ShowError(rssFeedB.CreateErrorDocument(ex.Message));
                }
            }
            else
            {
                ShowError(rssFeedB.CreateErrorDocument("Non valid RSS feed URL."));
            }
        }

        private void ShowError(XmlNode doc)
        {
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(doc.OuterXml.ToString());
        }
    }
}
