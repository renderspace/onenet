using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using System.IO;
using Newtonsoft.Json.Converters;
using System.ServiceModel.Web;
using One.Net.BLL.Utility;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public partial class FrontendService : IFrontendService
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected static BArticle articleB = new BArticle();

        public string Ping()
        {
            return "FrontendService 1:" + Thread.CurrentPrincipal.Identity.Name;
        }

        public List<DTOSearchableItem> SearchPageContent(string keyword, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);

            var result = new List<DTOSearchableItem>();

            var webSiteB = new BWebsite();

            var dict = webSiteB.FindPages(keyword);
            foreach (var page in dict)
            {
                var pageObj = webSiteB.GetPage(page.Key);
                var item = new DTOSearchableItem() { Id = page.Key.ToString(), Title = page.Value, Url = pageObj.URI };
                result.Add(item);
            }

            return result;
        }

        public List<DTOArticleSearch> ListArticles(int page, int recordsPerPage, int year, string regids, string sortBy, bool and)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            var result = new List<DTOArticleSearch>();
            var state = new ListingState();
            state.RecordsPerPage = recordsPerPage;
            state.SortDirection = SortDir.Descending;
            int firstRecordIndex = (page * state.RecordsPerPage.Value) - state.RecordsPerPage.Value;
            state.FirstRecordIndex = firstRecordIndex < 0 ? 0 : firstRecordIndex;
            state.SortField = string.IsNullOrWhiteSpace(sortBy) || sortBy.Length > 30 ? "id" : sortBy;
            state.SortDirection = SortDir.Descending;
            var categoriesFilter = StringTool.SplitStringToIntegers(regids);
            var articles = articleB.ListArticles(categoriesFilter, state, "", null, year > 1900 ? (DateTime?)new DateTime(year, 1, 1) : null, new List<int>());

            var filteredArtices = articles.Where(a => {
                var regulars = a.Regulars.Select(r => r.Id);
                foreach(var c in categoriesFilter)
                {
                    if (!a.Regulars.Where(r => r.Id.HasValue && r.Id.Value == c).Any())
                    {
                        return false;
                    }
                }
                return true;
            });

            result = (and ? filteredArtices : articles).Select(a => new DTOArticleSearch()
            {
                Id = a.Id.Value.ToString(),
                Title = a.Title,
                SubTitle = a.SubTitle,
                Teaser = a.ProcessedTeaser,
                Html = a.Html,
                HumanReadableUrl = a.HumanReadableUrl,
                DisplayDate = a.DisplayDate,
                FormattedDate = a.DisplayDate.ToShortDateString(),
                Regulars = a.Regulars.Select(r => r.Title).ToArray()
            }).ToList();

            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-And", and.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-CurrentCulture", Thread.CurrentThread.CurrentCulture.LCID.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-AllRecords", articles.AllRecords.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-CurrentPage", articles.CurrentPage.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-RecordsPerPage", state.RecordsPerPage.ToString());
            return result;
        }
    }
}
