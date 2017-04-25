using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;
using Newtonsoft.Json.Linq;
using One.Net.BLL.WebControls;

namespace OneMainWeb.CommonModules
{
    public partial class AzureSearchResults : MModule
    {
        [Setting(SettingType.String, DefaultValue = "")]
        public string AzureApiKey { get { return GetStringSetting("AzureApiKey"); } }

        [Setting(SettingType.String, DefaultValue = "")]
        public string SearchDomain { get { return GetStringSetting("SearchDomain"); } }

        [Setting(SettingType.Int, DefaultValue = "10")]
        public int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            PagerResults.RecordsPerPage = RecordsPerPage;
            PagerResults.SelectedPage = 1;
            PagerResults.Visible = false;

            if (Request["q"] != null)
            {
                if (Request[Pager.REQUEST_PAGE_ID + PagerResults.ID] != null)
                    PagerResults.SelectedPage = FormatTool.GetInteger(Request[Pager.REQUEST_PAGE_ID + PagerResults.ID]);

                var q = Request["q"];

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request parameters
                queryString["q"] = q + " site:" + SearchDomain;
                queryString["count"] = RecordsPerPage.ToString();
                queryString["offset"] = ((PagerResults.SelectedPage - 1) * RecordsPerPage).ToString();
                // queryString["mkt"] = "sl-si";
                queryString["safesearch"] = "Moderate";

                var request = (HttpWebRequest)WebRequest.Create("https://api.cognitive.microsoft.com/bing/v5.0/search?" + queryString);

                request.Headers.Add("Ocp-Apim-Subscription-Key", AzureApiKey);

                var response = request.GetResponse();
                var sr = new StreamReader(response.GetResponseStream());

                dynamic result = JObject.Parse(sr.ReadToEnd());

                if (result != null && result.webPages != null && result.webPages.totalEstimatedMatches != null && result.webPages.totalEstimatedMatches > 0 && result.webPages.value != null)
                {
                    RepeaterResults.DataSource = result.webPages.value;
                    RepeaterResults.DataBind();

                    PagerResults.TotalRecords = result.webPages.totalEstimatedMatches;
                    PagerResults.DetermineData();

                    PagerResults.Visible = result.webPages.totalEstimatedMatches > RecordsPerPage;
                }
            }
        }
    }
}