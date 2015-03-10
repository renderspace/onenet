﻿using System;
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

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContentHistory?contentId={contentId}&languageId={languageId}")]
        [Description("GetContentHistory(int contentId)")]
        IEnumerable<DTOAuditItem> GetContentHistory(int contentId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFolderTree?selectedId={selectedId}&languageId={languageId}")]
        [Description("GetFolderTree")]
        string GetFolderTree(int selectedId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListFiles?folderId={folderId}&languageId={languageId}")]
        [Description("List<DTOFile> ListFiles(int folderId)")]
        List<DTOFile> ListFiles(int folderId, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetFileForEditing?id={id}&languageId={languageId}")]
        [Description("DTOFile GetFileForEditing(int id, int languageId)")]
        DTOFile GetFileForEditing(int id, int languageId);

        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ChangeContent")]
        [Description("bool ChangeContent(DTOContent content)")]
        bool ChangeContent(DTOContent content);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContent?id={id}&languageId={languageId}")]
        [Description("DTOContent GetContent(int id, int languageId)")]
        DTOContent GetContent(int id, int languageId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetContentTemplate?instanceId={instanceId}")]
        [Description("DTOContentTemplate GetContentTemplate(int instanceId)")]
        DTOContentTemplate GetContentTemplate(int instanceId);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetTemplate?templateId={templateId}")]
        [Description("DTOTemplate GetTemplate(int templateId)")]
        DTOTemplate GetTemplate(int templateId);
    }
}
