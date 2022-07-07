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

        /* ---- */
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListArticles?page={page}&recordsPerPage={recordsPerPage}&year={year}&regids={regids}&sortBy={sortBy}&and={and}")]
        List<DTOArticleSearch> ListArticles(int page, int recordsPerPage, int year, string regids, string sortBy, bool and);
    }
}