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
    [ServiceContract]
    public interface IAdminService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContentHistory?contentId={contentId}")]
        [Description("GetContentHistory(int contentId)")]
        IEnumerable<DTOAuditItem> GetContentHistory(int contentId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFolderTree?selectedId={selectedId}")]
        [Description("GetFolderTree")]
        string GetFolderTree(int selectedId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListFiles?folderId={folderId}")]
        [Description("List<DTOFile> ListFiles(int folderId)")]
        List<DTOFile> ListFiles(int folderId);
    }
}
