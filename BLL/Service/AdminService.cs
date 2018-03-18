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
    public class AdminService : IAdminService
    {
        protected static BArticle articleB = new BArticle();
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public string Ping()
        {
            return "AdminService:" + Thread.CurrentPrincipal.Identity.Name;
        }

        public string GenerateArticleParLink(string title)
        {
            return articleB.GenerateHumanReadableArticleUrl(title);
        }

        public string GenerateRegularParLink(string title)
        {
            return articleB.GenerateHumanReadableRegularUrl(title);
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

            Thread.CurrentThread.CurrentCulture = new CultureInfo(Int32.Parse(postedContentTemplate.LanguageId));

            var templateId = 0;
            if (instance != null && instance.Settings != null && instance.Settings.ContainsKey("TemplateId"))
                templateId = int.Parse(instance.Settings["TemplateId"].Value);
            else
                int.TryParse(postedContentTemplate.TemplateId, out templateId);

            var dtoTemplate = this.GetTemplate(templateId);

            if (dtoTemplate != null && dtoTemplate.ContentFields != null && postedContentTemplate.ContentFields != null)
            {
                foreach (var field in dtoTemplate.ContentFields)
                {
                    var fieldId = field.Key.Trim().Replace(" ", "_").ToLower();
                    if (postedContentTemplate.ContentFields.ContainsKey(fieldId) && !storedContentTemplate.ContentFields.ContainsKey(fieldId))
                    {
                        storedContentTemplate.ContentFields.Add(field.Key, postedContentTemplate.ContentFields[fieldId]);
                    }
                }
            }

            contentTemplateB.ChangeContentTemplate(instanceId, storedContentTemplate);

            return true;
        }

        public bool RevertTextContent(DTOContent content)
        {
            if (content == null || string.IsNullOrWhiteSpace(content.InstanceId))
            {
                log.Error("RevertTextContent null content");
                return false;
            }

            var languageId = 0;
            int.TryParse(content.LanguageId, out languageId);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);

            var contentId = 0;
            int.TryParse(content.ContentId, out contentId);

            var instanceId = 0;
            int.TryParse(content.InstanceId, out instanceId);

            var textContentB = new BTextContent();

            var onlineContent = textContentB.GetTextContent(instanceId, true);

            if (onlineContent != null)
            {
                // TODO: after content is reverted to published version, the offline content should no longer be marked as changed? 
                textContentB.ChangeTextContent(instanceId, onlineContent.Title, onlineContent.SubTitle, onlineContent.Teaser, onlineContent.Html);
            }

            return true;
        }

        public bool IsTextContentPublished(int instanceId)
        {
            if (instanceId == 0)
                return false;
            var textContentB = new BTextContent();
            var instance = textContentB.GetTextContent(instanceId, true);

            return instance != null;
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

            var webSiteB = new BWebsite();

            if (file == null)
            {
                var result = contentB.Change(existingContent);
                if (!result)
                {
                    log.Error(" contentB.Change returned false");
                }
                
                var instanceIds = DbContent.GetTextContentInstanceId(existingContent.ContentId.Value);
                int i = 0;
                int pageId = 0;
                foreach (var moduleInstanceId in instanceIds)
                {
                    if (i == 0) {
                        var moduleInstance = webSiteB.GetModuleInstance(moduleInstanceId);
                        pageId = moduleInstance.PageId;
                    }
                    webSiteB.MarkModuleInstanceChanged(moduleInstanceId, pageId);
                    i++;
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
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);

            var files = fileB.List(folderId);

            var result = new List<DTOFile>();
            foreach (var f in files.OrderByDescending(f => f.Created))
            {
                result.Add(new DTOFile { Id = f.Id.Value.ToString(), Name = f.Name, Size = (f.Size / 1024).ToString(), Icon = GenerateFileIcon(f, 60), ContentId = (f.ContentId.HasValue ? f.ContentId.Value : 0).ToString(), Uri = "/_files/" + f.Id.Value + "/" + f.Name });
            }
            return result;
        }

        public bool DeleteFile (int fileId)
        {
            var fileB = new BFileSystem();
            return fileB.Delete(fileId);
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

        public string GetFolders(int parentId, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var folders = fileB.ListFolders();

            var childrenIEnumerable = folders.Where(f => f.ParentId.HasValue && f.ParentId.Value == parentId);

            var parentFolder = folders.Where(f => f.Id.HasValue && f.Id.Value == parentId).FirstOrDefault();
            if (parentFolder == null)
                return "";            

            return JsonConvert.SerializeObject(childrenIEnumerable.ToList<BOCategory>());
        }

        public string GetFolderFiles(int folderId, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            var fileB = new BFileSystem();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);

            var files = fileB.List(folderId);

            var result = new List<DTOFile>();
            foreach (var f in files.OrderByDescending(f => f.Created))
            {
                result.Add(new DTOFile { Id = f.Id.Value.ToString(), Name = f.Name, Size = (f.Size / 1024).ToString(), Icon = GenerateFileIcon(f, 60), ContentId = (f.ContentId.HasValue ? f.ContentId.Value : 0).ToString(), Uri = "/_files/" + f.Id.Value + "/" + f.Name, Title = f.Title });
            }

            return JsonConvert.SerializeObject(result);
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
            var lastChanged = "";
            if (file == null)
                return "";
            if (file.Content != null)
            {
                lastChanged = "title=\"" + file.Content.DisplayLastChanged + "\"";
            }
            else
            {
                lastChanged = "title=\"" + file.LastChanged.ToString() + "\"";
            }
            
            string extension = file.Extension.ToLower().Replace(".", "");
            string ret;

            ret = "<a " + lastChanged + " href=\"/_files/" + file.Id.Value + "/" + file.Name + "\" target=\"_blank\">";

            if (extension == "jpg" || extension == "gif" || extension == "png" || extension == "jpeg")// ||
            {
                ret += "<img  src=\"/_files/" + file.Id.Value + "/" + file.Name + "?w=" + width + "\" /></a>";
            }
            else
            {
                ret += "<img  src=\"/adm/Icons.ashx?extension=" + extension.Trim('.').ToLower() + "\" />";
            }

            return ret;
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
                var item = new DTOSearchableItem() { Id = page.Key.ToString(), Title = page.Value };
                result.Add(item);
            }

            return result;
        }

        public List<DTOSearchableItem> SearchDictionary(string keyword, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var result = new List<DTOSearchableItem>();
            var contentB = new BContent();
            var state = new ListingState();
            state.RecordsPerPage = 10;
            state.SortDirection = SortDir.Descending;
            state.FirstRecordIndex = 0;
            state.SortField = "title";
            PagedList<BODictionaryEntry> entries = contentB.ListDictionaryEntries(state, false, keyword);

            foreach (var entry in entries)
            {
                var item = new DTOSearchableItem() { Id = entry.KeyWord, Title = entry.Title };
                result.Add(item);
            }

            return result;
        }

        public List<DTOSearchableItem> SearchArticles(string keyword, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var result = new List<DTOSearchableItem>();
            var contentB = new BContent();
            var state = new ListingState();
            state.RecordsPerPage = 10;
            state.SortDirection = SortDir.Descending;
            state.FirstRecordIndex = 0;
            state.SortField = "title";
            PagedList<BOArticle> articles = articleB.ListArticles(new List<int>(), null, null, state, keyword, new List<int>());

            foreach (var article in articles)
            {
                var item = new DTOSearchableItem() { Id = article.Id.Value.ToString(), Title = article.Title };
                result.Add(item);
            }

            return result;
        }

        public static string RenderStatusIcons(object objMarkedForDeletion, object objIsChanged)
        {
            string title = "";
            string strReturn = "";
            if (objIsChanged != null && objMarkedForDeletion != null)
            {
                if (bool.Parse(objMarkedForDeletion.ToString()))
                {
                    strReturn = "/Res/brisanje.png";
                    title = "Marked for deletion";
                }
                else if (bool.Parse(objIsChanged.ToString()))
                {
                    strReturn = "/Res/objava.png";
                    title = "Changes waiting for publish";
                }
                else
                {
                    strReturn = "/Res/objavljeno.png";
                    title = "Published";
                }
            }
            return "<img data-toggle='tooltip' data-placement='left' src='" + strReturn + "' alt='' title='" + title + "' />";
        }

        public List<DTOArticleSearch> ListArticles(int languageId, int page)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var result = new List<DTOArticleSearch>();
            var contentB = new BContent();
            var state = new ListingState();
            state.RecordsPerPage = 15;
            state.SortDirection = SortDir.Descending;
            int firstRecordIndex = (page * state.RecordsPerPage.Value) - state.RecordsPerPage.Value;
            state.FirstRecordIndex = firstRecordIndex < 0 ? 0 : firstRecordIndex;
            state.SortField = "title";
            PagedList<BOArticle> articles = articleB.ListArticles(new List<int>(), null, null, state, "", new List<int>());

            foreach (var a in articles)
            {
                var item = new DTOArticleSearch() {
                    Id = a.Id.Value.ToString(),
                    Status = RenderStatusIcons(a.MarkedForDeletion, a.IsChanged),
                    Title = a.Title,
                    HumanReadableUrl = a.HumanReadableUrl,
                    DisplayDate = a.DisplayDate,
                    Categories = a.RegularsList
                };
                result.Add(item);
            }

            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-AllRecords", articles.AllRecords.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-CurrentPage", articles.CurrentPage.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-RecordsPerPage", state.RecordsPerPage.ToString());
            return result;
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

        [DataMember, JsonProperty]
        public string Title { get; set; }
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
                        if (!ContentFields.ContainsKey(key))
                        {
                            ContentFields.Add(key, value);
                        }
                        
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
        public string TemplateId { get; set; }

        [DataMember, JsonProperty]
        public string LanguageId { get; set; }

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
        public string InstanceId { get; set; }

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

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOSearchableItem
    {
        [DataMember, JsonProperty]
        public string Id { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOArticleSearch
    {
        [DataMember, JsonProperty]
        public string Id { get; set; }

        [DataMember, JsonProperty]
        public string Status { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string HumanReadableUrl { get; set; }

        [DataMember, JsonProperty]
        public DateTime DisplayDate { get; set; }

        [DataMember, JsonProperty]
        public string Categories { get; set; }
    }
}
