using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ComponentModel;
using System.ServiceModel.Web;
using System.Collections;

namespace One.Net.BLL.Service
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IFrontendService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "SearchPageContent?keyword={keyword}&languageId={languageId}")]
        [Description("List<DTOSearchableItem> SearchPageContent(string keyword, int languageId)")]
        List<DTOSearchableItem> SearchPageContent(string keyword, int languageId);
    }
}