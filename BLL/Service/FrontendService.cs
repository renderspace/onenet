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

        public string Ping()
        {
            return "FrontendService:" + Thread.CurrentPrincipal.Identity.Name;
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
    }
}
