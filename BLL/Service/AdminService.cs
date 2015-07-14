﻿using System;
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
using System.ServiceModel;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class AdminService : IAdminService
    {

        protected static Logger log = LogManager.GetCurrentClassLogger();

        public string Ping()
        {
            return "AdminService:" + Thread.CurrentPrincipal.Identity.Name;
        }

        public IEnumerable<DTOAuditItem> GetContentHistory(int contentId, int languageId)
        {
            var auditB = new BInternalContent();
            var audits = auditB.ListAudits(contentId, languageId);

            var result = new List<DTOAuditItem>();
            foreach (var a in audits)
            {
                result.Add(new DTOAuditItem { AuditGuid = a.AuditGuid, Title = a.Title, DisplayLastChanged = a.DisplayLastChanged });
            }
            return result;
        }

        public DTOContent GetContent(int id, int languageId)
        {
            var contentB = new BInternalContent();
            var content = contentB.GetUnCached(id, languageId);
            if (content.ContentId.HasValue && content.MissingTranslation)
                content.Title = BInternalContent.GetContentTitleInAnyLanguage(content.ContentId.Value);
            var result = new DTOContent(content);
            return result;
        }

        public DTOContentTemplate GetContentTemplate(int instanceId)
        {
            var contentTemplateB = new BContentTemplate();
            var contentTemplate = contentTemplateB.GetContentTemplate(instanceId);
            return new DTOContentTemplate(instanceId, contentTemplate);
        }

        public DTOTemplate GetTemplate(int templateId)
        {
            var template = BWebsite.GetTemplate(templateId);
            return new DTOTemplate(template);
        }

        public bool ChangeContentTemplate(DTOContentTemplate postedContentTemplate)
        {
            if (postedContentTemplate == null)
                return false;

            var contentTemplateB = new BContentTemplate();

            var instanceId = 0;
            int.TryParse(postedContentTemplate.InstanceId, out instanceId);

            var storedContentTemplate = contentTemplateB.GetContentTemplate(instanceId);

            if (storedContentTemplate == null)
                storedContentTemplate = new BOContentTemplate();

            storedContentTemplate.DateModified = DateTime.Now;
            storedContentTemplate.PrincipalModified = postedContentTemplate.PrincipalModified;
            storedContentTemplate.ContentFields = new Dictionary<string, string>();

            var webSiteB = new BWebsite();
            var instance = webSiteB.GetModuleInstance(instanceId, false);

            var templateId = 0;
            if (instance != null && instance.Settings != null && instance.Settings.ContainsKey("TemplateId"))
                templateId = int.Parse(instance.Settings["TemplateId"].Value);

            var dtoTemplate = this.GetTemplate(templateId);

            if (dtoTemplate != null && dtoTemplate.ContentFields != null && postedContentTemplate.ContentFields != null)
            {
                foreach (var field in dtoTemplate.ContentFields)
                {
                    var fieldId = field.Key.Trim().Replace(" ", "_").ToLower();
                    if (postedContentTemplate.ContentFields.ContainsKey(fieldId))
                    {
                        storedContentTemplate.ContentFields.Add(field.Key, postedContentTemplate.ContentFields[fieldId]);
                    }
                }
            }

            contentTemplateB.ChangeContentTemplate(instanceId, storedContentTemplate);

            return true;
        }
        
        public bool ChangeContent(DTOContent content)
        {
            if (content == null || string.IsNullOrWhiteSpace(content.LanguageId))
            {
                log.Error("ChangeContent null content");
                return false;
            }

            var contentId = 0;
            int.TryParse(content.ContentId, out contentId);
            var languageId = 0;
            int.TryParse(content.LanguageId, out languageId);
            var fileId = 0;
            int.TryParse(content.FileId, out fileId);

            if (content.Title.Contains(BOInternalContent.NO_TRANSLATION_TAG))
            {
                log.Error("ChangeContent NO_TRANSLATION_TAG");
                return false;
            }

            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                log.Error("ChangeContent NOT authenticated.");
                return false;
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var fileB = new BFileSystem();
            BOFile file = null;
            if (fileId > 0)
            {
                file = fileB.Get(fileId);
                if (file.Content != null && file.ContentId != contentId)
                {
                    return false;
                }
            }

            var contentB = new BInternalContent();

            var existingContent = (file != null && file.Content != null) ? file.Content : contentB.GetUnCached(contentId, languageId);
            if (existingContent == null)
                existingContent = new BOInternalContent();
            
            existingContent.LanguageId = languageId;
            existingContent.Title = content.Title;
            existingContent.SubTitle = content.Subtitle;
            existingContent.Teaser = content.Teaser;
            if (content.Html != null)
                existingContent.Html = content.Html;

            if (file == null)
            {
                var result = contentB.Change(existingContent);
                if (!result)
                {
                    log.Error(" contentB.Change returned false");
                }
                var instanceIds = DbContent.GetTextContentInstanceId(existingContent.ContentId.Value);
                foreach (var moduleInstanceId in instanceIds)
                {
                    var instance = DbWebsite.GetModuleInstance(moduleInstanceId, false);
                    BWebsite webSiteB = new BWebsite();
                    webSiteB.ChangeModuleInstance(instance);
                }
                return result;
            }
            else
            {
                file.Content = existingContent;
                fileB.Change(file);
                return true;
            }
        }

        public List<DTOFile> ListFiles(int folderId, int languageId)
        {
            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);

            var files = fileB.List(folderId);

            var result = new List<DTOFile>();
            foreach (var f in files)
            {
                result.Add(new DTOFile { Id = f.Id.Value.ToString(), Name = f.Name, Size = (f.Size / 1024).ToString(), Icon = GenerateFileIcon(f, 60), ContentId = (f.ContentId.HasValue ? f.ContentId.Value : 0).ToString(), Uri = "/_files/" + f.Id.Value + "/" + f.Name });
            }
            return result;
        }

        public DTOFile GetFileForEditing(int id, int languageId)
        {
            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var f = fileB.Get(id);
            var result = new DTOFile { Id = f.Id.Value.ToString(), Name = f.Name, Size = (f.Size / 1024).ToString(), Icon = GenerateFileIcon(f, 60), ContentId = (f.ContentId.HasValue ? f.ContentId.Value : 0).ToString(), Uri = "/_files/" + f.Id.Value + "/" + f.Name };
            return result;
        }

        public string GetFolderTree(int selectedId, int languageId)
        {
            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var folders = fileB.ListFolders();

            var rootFolder = folders.Where(f => !f.ParentId.HasValue).FirstOrDefault();
            if (rootFolder == null)
                return "";

            var result = new StringBuilder();
            result.Append(@"[ {""text"": """ + rootFolder.Title.Replace('"', ' ') + "\", \"id\": \"" + rootFolder.Id + "\", " + (selectedId == rootFolder.Id ? "\"selected\":\"true\"," : "") + "  \"nodes\": [");

            
            AddChildren(rootFolder, folders, result, selectedId);
            result.Append("] } ]");

            var temp = result.ToString();
            return temp;
        }

        private static void AddChildren(BOCategory parent, List<BOCategory> categories, StringBuilder result, int selectedId)
        {
            var count = 0;
            foreach (BOCategory category in categories)
            {
                if (category.ParentId.HasValue && category.ParentId.Value == parent.Id.Value)
                {
                    result.Append(" {\"text\": \"" + category.Title.Replace('"', ' ') + "\", \"id\": \"" + category.Id + "\", \"noChildren\": \"" + category.ChildCount.Value.ToString() + "\", " +
                        (selectedId == category.Id ? "\"selected\":\"true\"," : "") + " \"nodes\": [");
                    if (category.ChildCount.Value > 0)
                    {
                        AddChildren(category, categories, result, selectedId);
                    }
                    result.Append("] }, ");
                    count++;
                }
            }
            if (count > 0)
                result.Remove(result.Length - 2, 2);
        }

        private static string GenerateFileIcon(BOFile file, int width)
        {
            string extension = file.Extension.ToLower().Replace(".", "");
            string ret;

            ret = "<a href=\"/_files/" + file.Id.Value + "/" + file.Name + "\" target=\"_blank\">";

            if (extension == "jpg" || extension == "gif" || extension == "png" || extension == "jpeg")// ||
            {
                ret += "<img src=\"/_files/" + file.Id.Value + "/" + file.Name + "?w=" + width + "\" /></a>";
            }
            else
            {
                ret += "<img src=\"/adm/Icons.ashx?extension=" + extension.Trim('.').ToLower() + "\" />";
            }

            return ret;
        }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOFile
    {
        [DataMember, JsonProperty]
        public string Id { get; set; }

        [DataMember, JsonProperty]
        public string Size { get; set; }

        [DataMember, JsonProperty]
        public string Name { get; set; }

        [DataMember, JsonProperty]
        public string Icon { get; set; }

        [DataMember, JsonProperty]
        public string ContentId { get; set; }

        [DataMember, JsonProperty]
        public string Uri { get; set; }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOTemplate
    {
        [DataMember, JsonProperty]
        public string TemplateId { get; set; }

        [DataMember, JsonProperty]
        public Dictionary<string, string> ContentFields { get; set; }

        public string TemplateContent { get;set; }

        public DTOTemplate()
        { }

        public DTOTemplate(BOTemplate t)
        {
            if (t != null)
            {
                if (t.Id.HasValue)
                    TemplateId = t.Id.Value.ToString();

                ContentFields = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(t.TemplateContent))
                {
                    const string pattern = @"\{([^\}]+)\}";
                    foreach (Match match in Regex.Matches(t.TemplateContent, pattern))
                    {
                        var pairString = match.Value.Replace("{", "").Replace("}", "");
                        var pair = StringTool.SplitString(pairString);
                        var key = pair[0];
                        var value = "";
                        if (pair.Count > 1)
                            value = pair[1];
                        ContentFields.Add(key, value);
                    }
                }
            }
        }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOContentTemplate
    {
        [DataMember, JsonProperty]
        public string InstanceId { get; set; }

        [DataMember, JsonProperty]
        public string ContentTemplateId { get; set; }

        [DataMember, JsonProperty]
        public string DateCreated { get; set; }

        [DataMember, JsonProperty]
        public string DateModifed { get; set; }

        [DataMember, JsonProperty]
        public string PrincipalCreated { get; set; }

        [DataMember, JsonProperty]
        public string PrincipalModified { get; set; }

        [DataMember, JsonProperty]
        public Dictionary<string, string> ContentFields { get; set; }

        public DTOContentTemplate()
        { }

        public DTOContentTemplate(int instanceId, BOContentTemplate c)
        {
            InstanceId = instanceId.ToString();

            if (c != null)
            {
                if (c.Id.HasValue)
                    ContentTemplateId = c.Id.Value.ToString();

                ContentFields = c.ContentFields;

                DateCreated = c.DateCreated.ToString();
                PrincipalCreated = c.PrincipalCreated;
                DateModifed = c.DateModified.HasValue ? c.DateModified.Value.ToString() : "";
                PrincipalModified = !string.IsNullOrEmpty(c.PrincipalModified) ? c.PrincipalModified : "";
            }
        }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOContent
    {
        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Subtitle { get; set; }

        [DataMember, JsonProperty]
        public string Teaser { get; set; }

        [DataMember, JsonProperty]
        public string Html { get; set; }

        [DataMember, JsonProperty]
        public string ContentId { get; set; }

        [DataMember, JsonProperty]
        public string LanguageId { get; set; }

        [DataMember, JsonProperty]
        public string FileId { get; set; }

        public DTOContent()
        { }

        public DTOContent(BOInternalContent c)
        {
            if (c.ContentId.HasValue)
                ContentId = c.ContentId.Value.ToString();
            Title = c.Title;
            Subtitle = c.SubTitle;
            Teaser = c.Teaser;
            Html = c.Html;
            LanguageId = c.LanguageId.ToString();
        }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOAuditItem
    {
        [DataMember, JsonProperty]
        public string AuditGuid { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string DisplayLastChanged { get; set; }
    }
}
