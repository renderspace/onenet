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
    public interface IAdminService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContentHistory?contentId={contentId}&languageId={languageId}")]
        [Description("GetContentHistory(int contentId)")]
        IEnumerable<DTOAuditItem> GetContentHistory(int contentId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFolderTree?selectedId={selectedId}&languageId={languageId}")]
        [Description("GetFolderTree")]
        string GetFolderTree(int selectedId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFolders?parentId={parentId}&languageId={languageId}")]
        [Description("GetFolders")]
        string GetFolders(int parentId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFolderFiles?folderId={folderId}&languageId={languageId}")]
        [Description("GetFolderFiles")]
        string GetFolderFiles(int folderId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListFiles?folderId={folderId}&languageId={languageId}")]
        [Description("List<DTOFile> ListFiles(int folderId)")]
        List<DTOFile> ListFiles(int folderId, int languageId);

        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeleteFile?fileId={fileId}")]
        [Description("boolean DeleteFile(int fileId)")]
        bool DeleteFile(int fileId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFileForEditing?id={id}&languageId={languageId}")]
        [Description("DTOFile GetFileForEditing(int id, int languageId)")]
        DTOFile GetFileForEditing(int id, int languageId);

        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ChangeContent")]
        [Description("bool ChangeContent(DTOContent content)")]
        bool ChangeContent(DTOContent content);

        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "RevertTextContent")]
        [Description("bool RevertTextContent(DTOContent content)")]
        bool RevertTextContent(DTOContent content);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "IsTextContentPublished?instanceId={instanceId}")]
        [Description("bool IsTextContentPublished(int instanceId)")]
        bool IsTextContentPublished(int instanceId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContent?id={id}&languageId={languageId}")]
        [Description("DTOContent GetContent(int id, int languageId)")]
        DTOContent GetContent(int id, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContentTemplate?instanceId={instanceId}")]
        [Description("DTOContentTemplate GetContentTemplate(int instanceId)")]
        DTOContentTemplate GetContentTemplate(int instanceId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetTemplate?templateId={templateId}")]
        [Description("DTOTemplate GetTemplate(int templateId)")]
        DTOTemplate GetTemplate(int templateId);

        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ChangeContentTemplate")]
        [Description("bool ChangeContentTemplate(DTOContentTemplate contentTemplate)")]
        bool ChangeContentTemplate(DTOContentTemplate contentTemplate);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GenerateArticleParLink?title={title}")]
        [Description("string GenerateArticleParLink(string title)")]
        string GenerateArticleParLink(string title);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GenerateRegularParLink?title={title}")]
        [Description("string GenerateRegularParLink(string title)")]
        string GenerateRegularParLink(string title);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "SearchPageContent?keyword={keyword}&languageId={languageId}")]
        [Description("List<DTOSearchableItem> SearchPageContent(string keyword, int languageId)")]
        List<DTOSearchableItem> SearchPageContent(string keyword, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "SearchDictionary?keyword={keyword}&languageId={languageId}")]
        [Description("List<DTOSearchableItem> SearchDictionary(string keyword, int languageId)")]
        List<DTOSearchableItem> SearchDictionary(string keyword, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "SearchArticles?keyword={keyword}&languageId={languageId}")]
        [Description("List<DTOSearchableItem> SearchArticles(string keyword, int languageId)")]
        List<DTOSearchableItem> SearchArticles(string keyword, int languageId);

        /* -------- */

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "regulars?languageId={languageId}")]
        List<DTORegular> ListRegulars(int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "articles?languageId={languageId}&page={page}")]
        List<DTOArticleSearch> ListArticles(int languageId, int page);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "articles/exists/{humanReadableUrl}?excludeArticleId={articleId}")]
        bool CheckArticleHumanReadableUrl(string humanReadableUrl, int articleId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "articles/{rawId}?languageId={languageId}")]
        DTOArticle GetArticle(string rawId, int languageId);

        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "articles")]
        int SaveArticle(DTOArticle article);

    }
}
