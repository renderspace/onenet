using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;
using System.Net;
using System.Xml;
using TwoControlsLibrary;
using log4net;

namespace OneMainWeb.CommonModules
{
    public partial class NajdiSearch : MModule
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(NajdiSearch));

        protected string SearchUri { get { return GetStringSetting("SearchUri"); } }
        protected string SearchDomain { get { return GetStringSetting("SearchDomain"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            PlaceHolderResults.Visible = PlaceHolderNoResults.Visible = false;

            if (Request["q"] != null)
            {
                WebClient client = new WebClient();

                var builder = new UrlBuilder(SearchUri);
                builder.QueryString.Clear();
                builder.QueryString["q"] = Request["q"];
                if (Request["o"] != null)
                    builder.QueryString["o"] = Request["o"];
                if (!string.IsNullOrEmpty(SearchDomain))
                {
                    builder.QueryString["location"] = SearchDomain;
                    builder.QueryString["locationmode"] = "include";
                    builder.QueryString["st"] = "adv";
                }

                client.Encoding = System.Text.Encoding.UTF8;
                log.Info("NajdiSearch uri:" + builder.ToString());
                var rawXml = client.DownloadString(builder.ToString());
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(rawXml);

                var resultPath = @"/InterseekXml/SearchOk/Results/Result";
                var pagePath = @"/InterseekXml/SearchOk/Pages/Page";
                var baseUrlPath = @"/InterseekXml/SearchOk/BaseUrl";

                XmlNodeList xmlResultList = null;
                XmlNodeList xmlPageList = null;
                XmlNodeList xmlBaseUrlList = null;

                if (doc.DocumentElement.Attributes["xmlns"] != null)
                {
                    string xmlns = doc.DocumentElement.Attributes["xmlns"].Value;
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("Najdi.si", xmlns);

                    resultPath = @"/Najdi.si:InterseekXml/Najdi.si:SearchOk/Najdi.si:Results/Najdi.si:Result";
                    pagePath = @"/Najdi.si:InterseekXml/Najdi.si:SearchOk/Najdi.si:Pages/Najdi.si:Page";
                    baseUrlPath = @"/Najdi.si:InterseekXml/Najdi.si:SearchOk/Najdi.si:BaseUrl";

                    xmlResultList = doc.SelectNodes(resultPath, nsmgr);
                    xmlPageList = doc.SelectNodes(pagePath, nsmgr);
                    xmlBaseUrlList = doc.SelectNodes(baseUrlPath, nsmgr);
                }
                else
                {
                    xmlResultList = doc.SelectNodes(resultPath);
                    xmlPageList = doc.SelectNodes(pagePath);
                    xmlBaseUrlList = doc.SelectNodes(baseUrlPath);
                }

                log.Info("Xml Document loaded");

                var baseUrl = "";
                if (xmlBaseUrlList != null && xmlBaseUrlList.Count > 0)
                    baseUrl = xmlBaseUrlList[0].InnerText;

                if (xmlResultList.Count > 0)
                {

                    PlaceHolderResults.Visible = true;


                    var results = new List<SearchResult>();
                    foreach (XmlElement node in xmlResultList)
                    {
                        XmlNodeList list = null;

                        var result = new SearchResult();

                        list = node.GetElementsByTagName("Abstract");
                        if (list != null && list.Count > 0)
                            result.Abstract = list[0].InnerText;

                        list = node.GetElementsByTagName("ContentLength");
                        if (list != null && list.Count > 0)
                            result.ContentLength = list[0].InnerText;

                        list = node.GetElementsByTagName("ContentLength");
                        if (list != null && list.Count > 0)
                            result.ContentLengthUnit = list[0].Attributes["unit"].Value;

                        list = node.GetElementsByTagName("ContentType");
                        if (list != null && list.Count > 0)
                            result.ContentType = list[0].InnerText;

                        list = node.GetElementsByTagName("LastModified");
                        if (list != null && list.Count > 0)
                            result.LastModified = list[0].InnerText;

                        list = node.GetElementsByTagName("QuickPreviewAbstract");
                        if (list != null && list.Count > 0)
                            result.QuickPreviewAbstract = list[0].InnerText;

                        list = node.GetElementsByTagName("QuickPreviewTitle");
                        if (list != null && list.Count > 0)
                            result.QuickPreviewTitle = list[0].InnerText;

                        list = node.GetElementsByTagName("Title");
                        if (list != null && list.Count > 0)
                            result.Title = list[0].InnerText;

                        list = node.GetElementsByTagName("Url");
                        if (list != null && list.Count > 0)
                            result.Url = HttpUtility.UrlDecode(list[0].InnerText);

                        list = node.GetElementsByTagName("QuickPreviewUri");
                        if (list != null && list.Count > 0)
                            result.QuickPreviewUri = list[0].InnerText;

                        list = node.GetElementsByTagName("MoreHitsFromThisHostUri");
                        if (list != null && list.Count > 0)
                            result.MoreHitsFromThisHostUri = list[0].InnerText;
                        results.Add(result);
                    }

                    RepeaterResults.DataSource = results;
                    RepeaterResults.DataBind();


                    var pages = new List<SearchPage>();
                    foreach (XmlElement pagerNode in xmlPageList)
                    {
                        var page = new SearchPage();
                        page.Index = pagerNode.Attributes["index"].Value;
                        page.Selected = false;

                        var newPagerBuilder = new UrlBuilder(Page);
                        newPagerBuilder.QueryString.Clear();
                        newPagerBuilder.QueryString["q"] = HttpUtility.UrlDecode(Request["q"]);

                        var externalPagerBuilder = new UrlBuilder(baseUrl + HttpUtility.UrlDecode(pagerNode.InnerText));

                        var requestO = -1;
                        if (Request["o"] != null)
                            requestO = FormatTool.GetInteger(Request["o"]) / 10;

                        if (externalPagerBuilder.QueryString.ContainsKey("o"))
                        {
                            newPagerBuilder.QueryString["o"] = externalPagerBuilder.QueryString["o"];

                            if (Int32.Parse(page.Index) == requestO)
                                page.Selected = true;

                        }
                        else if (Request["o"] != null && requestO == Int32.Parse(page.Index))
                            page.Selected = true;
                        else if (Request["o"] == null)
                            page.Selected = true;

                        page.Url = newPagerBuilder.ToString();

                        pages.Add(page);
                    }

                    RepeaterPages.DataSource = pages;
                    RepeaterPages.DataBind();
                }
                else
                {
                    PlaceHolderNoResults.Visible = true;
                }
            }

        }
    }

    public class SearchResult
    {
        public string Abstract { get; set; }
        public string ContentLength { get; set; }
        public string ContentLengthUnit { get; set; }
        public string ContentType { get; set; }
        public string LastModified { get; set; }
        public string QuickPreviewAbstract { get; set; }
        public string QuickPreviewTitle { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string QuickPreviewUri { get; set; }
        public string MoreHitsFromThisHostUri { get; set; }
    }

    public class SearchPage
    {
        public string Index { get; set; }
        public string Url { get; set; }
        public bool Selected { get; set; }
    }
}