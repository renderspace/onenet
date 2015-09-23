using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using One.Net.BLL.DAL;
using One.Net.BLL.Utility;
using System.Threading;
using System.Globalization;
using Microsoft.Web.Administration;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Data.SqlClient;
using MsSqlDBUtility;
using System.Data;
using System.Text;
using System.Xml;
using System.Web;
using System.Web.UI;
using One.Net.BLL.Model.Attributes;

namespace One.Net.BLL
{
    [Serializable]
    public class BWebsite : BusinessBaseClass
    {
        public const string CACHE_SITE_LIST = "List<BOWebSite> List()";
        static readonly DbWebsite webSiteDb = new DbWebsite();
        readonly BInternalContent intContentB = new BInternalContent();
        readonly BContentTemplate contentTemplateB = new BContentTemplate();
        readonly BContent contentB = new BContent();

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingWebSiteList = new Object();
        private static readonly object cacheLockingTemplatesList = new Object();
        private static readonly object cacheLockingPage = new Object();

        public BOWebSite Get(int websiteID)
        {
            return List().Where(s => s.Id == websiteID).FirstOrDefault();
        }

        public int? GetRootPageId(int webSiteId)
        {
            BOWebSite webSite = Get(webSiteId);
            return (webSite != null) ? webSite.RootPageId : null;
        }

        public BOPage GetPage(int pageId)
        {
            return GetPage(pageId, LanguageId);
        }

        private static string PAGE_CACHE_ID(int pageId, int languageId, bool publishFlag)
        {
            return "SinglePage_" + pageId + "_L" + languageId + "_" + publishFlag;
        }

        private BOPage GetPage(int pageId, int languageId)
        {
            string cacheKey = PAGE_CACHE_ID(pageId, languageId, PublishFlag);
            BOPage page = cache.Get<BOPage>(cacheKey);

            if (page == null)
            {
                log.Info("Page MISS [" + cacheKey + "]");
                page = webSiteDb.GetPage(pageId, PublishFlag, languageId);
                if (page != null)
                {
                    foreach (BOPlaceHolder placeHolder in page.PlaceHolders.Values)
                    {
                        for (int i = 0; i < placeHolder.ModuleInstances.Count; i++)
                        {
                            bool isInherited = placeHolder.ModuleInstances[i].IsInherited;
                            placeHolder.ModuleInstances[i] = GetModuleInstance(placeHolder.ModuleInstances[i].Id);
                            placeHolder.ModuleInstances[i].IsInherited = isInherited;
                        }
                    }
                    lock (cacheLockingPage)
                    {
                        BOPage tempPage = cache.Get<BOPage>(cacheKey);
                        if (null == tempPage)
                            cache.Put(cacheKey, page);
                    }
                }
            }
            return page;
        }

        private BOPage GetUnCachedPage(int pageId, bool publish, int languageId = 0)
        {

            if (languageId == 0)
                languageId = LanguageId;

            BOPage page = webSiteDb.GetPage(pageId, publish, languageId);

            if (page != null)
            {
                foreach (BOPlaceHolder placeHolder in page.PlaceHolders.Values)
                {
                    for (int i = 0; i < placeHolder.ModuleInstances.Count; i++)
                    {
                        bool isInherited = placeHolder.ModuleInstances[i].IsInherited;
                        placeHolder.ModuleInstances[i] = GetModuleInstance(placeHolder.ModuleInstances[i].Id);
                        placeHolder.ModuleInstances[i].IsInherited = isInherited;
                    }
                }
            }
            return page;
        }

        public List<BOPage> ListChildrenPages(int pageId)
        {
            List<BOPage> site = GetSiteStructure(GetPage(pageId).WebSiteId);
            List<BOPage> children = new List<BOPage>();
            foreach (BOPage page in site)
            {
                if ((page.ParentId.HasValue && page.ParentId.Value == pageId))
                {
                    children.Add(page);
                }
            }
            return children;
        }

        public List<int> ListChildrenIds(int pageId)
        {
            return webSiteDb.ListChildrenIds(pageId, PublishFlag);
        }

        public List<int> ListPublishedDescendantIds(int pageId)
        {
            List<int> publishedDescendantIds = null;
            
            this.ListDescendantIds(pageId, ref publishedDescendantIds);
            
            return publishedDescendantIds;
        }

        private void ListDescendantIds(int pageId, ref List<int> publishedDescendantIds)
        {
            if (publishedDescendantIds == null)
                publishedDescendantIds = new List<int>();

            var foundUnPublishedChildren = webSiteDb.ListChildrenIds(pageId, false);
            var foundPublishedChildren = webSiteDb.ListChildrenIds(pageId, true);

            foreach (int childId in foundPublishedChildren)
            {
                publishedDescendantIds.Add(childId);
            }

            foreach (int childId in foundUnPublishedChildren)
            {
                ListDescendantIds(childId, ref publishedDescendantIds);
            }
        }

        public string GetPageUri(int pageId)
        {
            BOPage page = GetPage(pageId, LanguageId);
            if (page == null)
                return null;
            return page.URI;
        }

        public enum AddSubPageResult { Ok, OkRootPage, PartialLinkNotValid, PartialLinkExistsOnThisLevel, TriedToAddRootPageToNonEmptySite, NoTemplates }

        public AddSubPageResult AddSubPage(string requestedPageTitle, int webSiteId, int currentPageId)
        {
            BOPage parentPage = GetPage(currentPageId);
            string newParLink = "";

            newParLink = PrepareParLink(requestedPageTitle);
            if (!ValidateParLinkSyntax(newParLink))
                return AddSubPageResult.PartialLinkNotValid;

            var page = new BOPage
            {
                Title = requestedPageTitle,
                PublishFlag = false,
                LanguageId = Thread.CurrentThread.CurrentCulture.LCID,
                MenuGroup = 0,
                WebSiteId = webSiteId,
                BreakPersistance = false,
                RedirectToUrl = "",
                URI = "",
                SubRouteUrl = ""
            };

            bool addingRootPage = false;
            if (parentPage != null) // Not root
            {
                List<BOPage> pages = ListChildrenPages(parentPage.Id);
                if (pages != null && pages.Count > 0)
                {
                    page.Order = pages[pages.Count - 1].Order + 1;
                }
                page.Template = new BOTemplate { Id = parentPage.Template.Id.Value };
                page.ParentId = parentPage.Id;
                page.Settings = parentPage.Settings;
            }
            else // root
            {
                currentPageId = -1;
                addingRootPage = true;
                // check if there are no existing pages on this website
                if (GetSiteStructure(webSiteId).Count != 0)
                {
                    return AddSubPageResult.TriedToAddRootPageToNonEmptySite;
                }
                newParLink = "";
                var templates = ListTemplates("3");
                if (templates.Count == 0)
                {
                    return AddSubPageResult.NoTemplates;
                }
                page.Template = new BOTemplate { Id = templates[0].Id.Value };
            }

            if (!ValidateParLinkAgainstDB(currentPageId == -1 ? (int?)null : currentPageId, -1, newParLink, webSiteId))
                return AddSubPageResult.PartialLinkExistsOnThisLevel;

            page.ParLink = newParLink;
            ChangePage(page);
            OneSiteMapProvider.ReloadSiteMap();
            return addingRootPage ? AddSubPageResult.OkRootPage : AddSubPageResult.Ok;
        }

        public static string PrepareParLink(string parLink)
        {
            return CleanStringForUrl((parLink.ToLower()).Replace(" ", "-")); ;
        }

        /// <summary>
        /// Replaces out all characters not appropriate for URL's.
        /// Also it replaces some like blank space with underscore (_)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CleanStringForUrl(string str)
        {
            string answer = str.ToLower(CultureInfo.InvariantCulture);
            answer = answer.Replace("š", "s");
            answer = answer.Replace("ž", "z");
            answer = answer.Replace("ć", "c");
            answer = answer.Replace("č", "c");
            answer = answer.Replace("đ", "d");

            answer = answer.Replace("Š", "S");
            answer = answer.Replace("Ž", "Z");
            answer = answer.Replace("Ć", "C");
            answer = answer.Replace("Č", "C");
            answer = answer.Replace("Ð", "D");

            answer = answer.Replace("/", "-");
            answer = answer.Replace(" ", "-");
            answer = answer.Replace("?", "");
            answer = answer.Replace(",", "");
            answer = answer.Replace(":", "-");
            answer = answer.Replace(".", "-");
            answer = answer.Replace(";", "-");
            answer = answer.Replace("&", "and");
            answer = answer.Replace("#", "No");
            answer = answer.Replace("\"", "-");
            answer = answer.Replace("*", "-");
            return answer;
        }

        public void ChangePage(BOPage page)
        {
            ChangePage(page, false);
        }

        private void ChangePage(BOPage page, bool publishing)
        {
            if (PublishFlag)
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            if (page == null)
                return;

            if (!publishing)
                page.IsChanged = true;

            intContentB.Change(page);
            if (page.IsRoot)
            {
                page.Level = 0;
                page.URI.TrimStart(new char[] { '/' });
                cache.Remove(CACHE_SITE_LIST);
            }
            else
            {
                page.Level = webSiteDb.GetPage(page.ParentId.Value, page.PublishFlag, page.LanguageId).Level + 1;
            }
            webSiteDb.ChangePage(page);
            cache.Remove(PAGE_CACHE_ID(page.Id, page.LanguageId, page.PublishFlag));
        }

        public void SwapOrderOfPages(int page1Id, int page2Id)
        {
            BOPage page1 = GetPage(page1Id);
            BOPage page2 = GetPage(page2Id);
            int page1idx = page1.Order;
            int page2idx = page2.Order;

            if (page1.ParentId != page2.ParentId || page1.WebSiteId != page2.WebSiteId || page1.PublishFlag != page2.PublishFlag)
            {
                throw new ArgumentException("pages do not share the same parent, so swap is not possible");
            }
            page1.Order = page2idx;
            page1.IsChanged = true;
            ChangePage(page1);
            page2.Order = page1idx;
            page2.IsChanged = true;
            ChangePage(page2);
        }

        public void UndeletePage(int pageId)
        {
            if (PublishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOPage page = GetPage(pageId);
            page.MarkedForDeletion = false;
            page.IsChanged = true;
            ChangePage(page);
        }

        /// <summary>
        /// Count value is count of pages marked for deletion... if any are marked for deletion then 
        /// final delete is not allowed. Only when count is 0 across the board do we begin deleting 
        /// pages finally.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public void DeleteTree(int pageId, ref int markedCount, ref int deletedCount)
        {
            if (PublishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOPage offlinePage = GetUnCachedPage(pageId, false);
            BOPage onlinePage = GetUnCachedPage(pageId, true);

            List<BOPage> children = GetSiteStructure(offlinePage.WebSiteId);
            foreach (BOPage loopPage in children)
            {
                if (loopPage.ParentId.HasValue && loopPage.ParentId.Value == pageId)
                {
                    DeleteTree(loopPage.Id, ref markedCount, ref deletedCount);
                }
            }

            if (offlinePage.IsRoot)
                cache.Remove(CACHE_SITE_LIST);

            if (offlinePage.MarkedForDeletion && markedCount == 0)
            {
                webSiteDb.DeletePage(pageId, false);
                intContentB.Delete(offlinePage.ContentId.Value);
                webSiteDb.DeletePage(pageId, true);
                if (onlinePage != null)
                    intContentB.Delete(onlinePage.ContentId.Value);
                deletedCount++;
            }
            else
            {
                offlinePage.MarkedForDeletion = true;
                offlinePage.IsChanged = true;
                ChangePage(offlinePage);
                markedCount++;
            }
        }


        public enum DeletePageByIdResult { Deleted, DeletedRoot, HasChildren, Error }

        public DeletePageByIdResult DeletePageById(int pageId)
        {
            BOPage currentPageModel = GetPage(pageId);

            if (currentPageModel != null)
            {
                if (ListChildrenPages(currentPageModel.Id).Count != 0)
                    return DeletePageByIdResult.HasChildren;

                var result = DeletePage(pageId);
                if (!result)
                    return DeletePageByIdResult.Error;

                return currentPageModel.IsRoot ? DeletePageByIdResult.DeletedRoot : DeletePageByIdResult.Deleted;
            }
            return DeletePageByIdResult.Error;
        }

        private bool DeletePage(int pageId)
        {
            if (PublishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOPage offlinePage = GetUnCachedPage(pageId, false);
            BOPage onlinePage = GetUnCachedPage(pageId, true);

            List<BOPage> children = GetSiteStructure(offlinePage.WebSiteId);
            bool hasChildren = false;
            foreach (BOPage loopPage in children)
            {
                if (loopPage.ParentId.HasValue && loopPage.ParentId.Value == pageId)
                {
                    hasChildren = true;
                    break;
                }
            }

            if (offlinePage.IsRoot)
                cache.Remove(CACHE_SITE_LIST);

            if (!hasChildren && offlinePage.MarkedForDeletion)
            {
                webSiteDb.DeletePage(pageId, false);
                intContentB.Delete(offlinePage.ContentId.Value);
                webSiteDb.DeletePage(pageId, true);
                if (onlinePage != null)
                    intContentB.Delete(onlinePage.ContentId.Value);
                return true;
            }
            else if (!hasChildren)
            {
                offlinePage.MarkedForDeletion = true;
                offlinePage.IsChanged = true;
                ChangePage(offlinePage);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UnPublishPage(int pageId)
        {
            BOPage offlinePage = GetUnCachedPage(pageId, false);
            BOPage onlinePage = GetUnCachedPage(pageId, true);

            if (!offlinePage.IsNew)
            {
                webSiteDb.DeletePage(pageId, true);
                intContentB.Delete(onlinePage.ContentId.Value);
                offlinePage.IsChanged = true;
                webSiteDb.ChangePage(offlinePage);
                cache.Remove(PAGE_CACHE_ID(offlinePage.Id, offlinePage.LanguageId, offlinePage.PublishFlag));
                return true;
            }
            else
                return false;
        }

        public bool PublishPage(int pageId)
        {
            if (PublishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            try
            {
                BOPage page = null;

                page = GetPage(pageId);
                // When publisher calls this method with the wrong language ID, nothing gets published
                // This won't affect anything else since pages only exist in one language really.
                if (page == null)
                {
                    List<int> languages = ListLanguages();
                    foreach (int i in languages)
                    {
                        page = GetPage(pageId, i);
                        if (page != null)
                            break;
                    }
                }

                if (null == page)
                {
                    log.Error("Page is null, publish failed -> PageID:" + pageId);
                    return false;
                }

                if (page.MarkedForDeletion)
                {
                    bool result = DeletePage(pageId);
                    return result;
                }

                if (!page.IsChanged)
                    return false; // nothing to publish

                if (!page.IsRoot)
                {
                    BOPage parentPage = GetPage(page.ParentId.Value, page.LanguageId);
                    if (parentPage.IsNew)
                        return false; // we can't publish a page that has no published parent.
                }

                page.IsChanged = false;

                ChangePage(page, true); // mark the original as published

                // take care of page title (if B
                BOPage publishedPage = webSiteDb.GetPage(page.Id, true, page.LanguageId);
                if (page.IsNew || publishedPage == null || (publishedPage != null && page.ContentId == publishedPage.ContentId))
                    page.ContentId = null;
                else
                    page.ContentId = publishedPage.ContentId;

                page.PublishFlag = true;
                // do actual publishing of main page data

                ChangePage(page, true);
                // proceed with publishing of modules
                foreach (BOPlaceHolder placeholder in page.PlaceHolders.Values)
                {
                    foreach (BOModuleInstance instance in placeholder.ModuleInstances)
                    {
                        if (!instance.IsInherited && instance.PageId == page.Id)
                        {
                            if (instance.PendingDelete)
                                webSiteDb.DeleteModuleInstance(instance.Id);
                            else if (instance.ModuleName == "TextContent")
                                PublishTextContentModuleInstance(instance, page.LanguageId);
                            else if (instance.ModuleName == "SpecialContent")
                                PublishTextContentModuleInstance(instance, page.LanguageId);
                            else if (instance.ModuleName == "TemplateContent")
                                PublishTemplateContentModuleInstance(instance, page.LanguageId);
                            else
                            {
                                instance.Changed = false;
                                ChangeModuleInstance(instance, LanguageId, true);
                                instance.PublishFlag = true;
                                ChangeModuleInstance(instance, LanguageId, true);
                            }
                        }
                    }
                }
                /*
                foreach (BOPlaceHolder placeholder in page.PlaceHolders.Values)
                {
                    foreach (BOModuleInstance instance in placeholder.ModuleInstances)
                    {
                        string moduleClassName = "One.Net.BLL.B" + instance.Name;
                        Type[] implementedInterfaces = Type.GetType(moduleClassName).GetInterfaces();
                        foreach (Type implementedInterface in implementedInterfaces)
                        {
                            if (implementedInterface == typeof(IModuleInstancePublisher))
                            {

                                IModuleInstancePublisher publisher = (IModuleInstancePublisher)Activator.CreateInstance(Type.GetType(moduleClassName));
                                publisher.PublishModuleInstance(instance.Id, instance.Settings);
                            }
                        }
                    }
                }
                */
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                log.Error("Publish failed", ex);
                return false;
            }
            return true;
        }

        private void PublishTemplateContentModuleInstance(BOModuleInstance instance, int languageId)
        {
            instance.Changed = false;
            ChangeModuleInstance(instance, languageId, true);

            var contentTemplate = contentTemplateB.GetContentTemplate(instance.Id, false);

            if (contentTemplate == null)
                return; // nothing to publish;

            BOModuleInstance publishedInstance = DbWebsite.GetModuleInstance(instance.Id, true);
            if (publishedInstance != null)
                contentTemplate.Id = int.Parse(publishedInstance.Settings["ContentTemplateId"].Value);
            else
                contentTemplate.Id = null;

            contentTemplateB.ChangeContentTemplate(instance.Id, contentTemplate);

            publishedInstance = instance;

            publishedInstance.Settings["ContentTemplateId"] = new BOSetting { Name = "ContentTemplateId", Type = SettingTypeEnum.Int, Value = contentTemplate.Id.Value.ToString(), UserVisibility = VisibilityEnum.SPECIAL };
            publishedInstance.PublishFlag = true;
            ChangeModuleInstance(publishedInstance, languageId, true);
        }

        private void PublishTextContentModuleInstance(BOModuleInstance instance, int languageId)
        {
            instance.Changed = false;
            ChangeModuleInstance(instance, languageId, true);

            int previewContentId = int.Parse(instance.Settings["ContentId"].Value);
            var content = intContentB.GetUnCached(previewContentId, languageId);

            if (content == null)
                return; // nothing to get published.

            BOModuleInstance publishedInstance = DbWebsite.GetModuleInstance(instance.Id, true);
            if (publishedInstance != null)
                content.ContentId = int.Parse(publishedInstance.Settings["ContentId"].Value);
            else
                content.ContentId = null;

            intContentB.Change(content);
            publishedInstance = instance;

            publishedInstance.Settings["ContentId"] = new BOSetting { Name = "ContentId", Type = SettingTypeEnum.Int, Value = content.ContentId.Value.ToString(), UserVisibility = VisibilityEnum.SPECIAL  };
            publishedInstance.PublishFlag = true;
            ChangeModuleInstance(publishedInstance, languageId, true);
        }

        public List<BOPage> GetSiteStructure(int webSiteId)
        {
            return webSiteDb.GetSiteStructure(webSiteId, LanguageId, PublishFlag);
        }

        public int PublishAllPages(int webSiteId)
        {
            var pages = webSiteDb.ListPages(webSiteId, LanguageId, false, true);
            int publishedCount = 0;

            foreach (var p in pages)
            {
                if (PublishPage(p.Id))
                {
                    publishedCount++;
                }
            }
            return publishedCount;
        }

        #region Module Instances

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceOne"></param>
        /// <param name="instanceTwo"></param>
        public void SwapModuleInstances(BOModuleInstance instanceOne, BOModuleInstance instanceTwo)
        {
            if (instanceOne.PlaceHolderId == instanceTwo.PlaceHolderId && instanceOne.PageId == instanceTwo.PageId)
            {
                int tempIdx = instanceOne.Order;
                instanceOne.Order = instanceTwo.Order;
                instanceTwo.Order = tempIdx;
                this.ChangeModuleInstance(instanceOne, LanguageId, false);
                this.ChangeModuleInstance(instanceTwo, LanguageId, false);
            }
        }

        public bool AddModulesInstance(int pageId, int placeholderId, int moduleId)
        {
            if (pageId >= 0 && moduleId >= 0 && placeholderId >= 0)
            {
                BOModuleInstance moduleInstance = new BOModuleInstance();
                moduleInstance.PageId = pageId;
                moduleInstance.PublishFlag = false;
                moduleInstance.PendingDelete = false;
                moduleInstance.Changed = true;
                moduleInstance.ModuleId = moduleId;
                moduleInstance.Order = -1;
                moduleInstance.PlaceHolderId = placeholderId;
                moduleInstance.PersistFrom = 0;
                moduleInstance.PersistTo = 0;
                ChangeModuleInstance(moduleInstance);
                return true;
            }
            return false;
        }

        public void MarkModuleInstanceChanged(int instanceId, int pageId)
        {
            DbWebsite.MarkModuleInstanceChanged(instanceId);
            DbWebsite.MarkPageChanged(pageId);
            cache.Remove(PAGE_CACHE_ID(pageId, LanguageId, PublishFlag));
        }

        public void ChangeModuleInstance(BOModuleInstance instance)
        {
            ChangeModuleInstance(instance, LanguageId, false);
        }

        /// <summary>
        /// Note, when publishing, order stuff is ignored, since it plays no part in publishing 
        /// scenario.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="publishing"></param>
        private void ChangeModuleInstance(BOModuleInstance instance, int languageId, bool publishing)
        {
            if (publishing)
            {
                DbWebsite.ChangeModuleInstance(instance);
            }
            else
            {
                instance.Changed = true;
                BOPage containingPage = this.GetPage(instance.PageId, languageId);
                BOModuleInstance existingInstance = this.GetModuleInstance(instance.Id);
                if (existingInstance != null)
                {
                    if (existingInstance.PlaceHolderId != instance.PlaceHolderId)
                    {
                        // Instance PlaceHolderId changed, so add instance at end of PlaceHolder instances list.
                        if (containingPage.PlaceHolders.ContainsKey(instance.PlaceHolderId))
                        {
                            // Order of instance is PlaceHolder instances list count ( ie place instance at end of PlaceHolder instances list )
                            int numberOfTargetPlaceholderUsages = containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances.Count;
                            if (numberOfTargetPlaceholderUsages > 0)
                                instance.Order = containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances[numberOfTargetPlaceholderUsages - 1].Order + 1;
                            else
                                instance.Order = 0;
                        }
                        else
                            instance.Order = 0;
                    }
                }
                else
                {
                    // instance is new ( does not exist )
                    if (containingPage.PlaceHolders.ContainsKey(instance.PlaceHolderId))
                    {
                        // Order of new instance is PlaceHolder instances list count ( ie place instance at end of PlaceHolder instances list )
                        var cnt = containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances.Count;
                        if (cnt > 0)
                            instance.Order = containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances[cnt - 1].Order + 1;
                        else
                            instance.Order = 0;
                    }
                    else
                        instance.Order = 0;
                }
                DbWebsite.ChangeModuleInstance(instance);

                // Mark page as changed
                containingPage.IsChanged = true;
                containingPage.PublishFlag = false;
                ChangePage(containingPage);
            }
        }

        public List<BOModuleInstance> ListModuleInstances(int pageId)
        {
            var instances = new List<BOModuleInstance>();

            BOPage page = GetUnCachedPage(pageId, false);
            if (page == null)
                return instances;

            foreach (BOPlaceHolder placeholder in page.PlaceHolders.Values)
            {
                foreach (BOModuleInstance instance in placeholder.ModuleInstances)
                {
                    instances.Add(instance);
                }
            }

            foreach (var mi in instances)
            {
                if (mi != null && (mi.ModuleName == "TextContent" || mi.ModuleName == "SpecialContent") && !mi.IsInherited)
                {
                    if (mi.Settings != null && mi.Settings.ContainsKey("ContentId"))
                    {
                        int contentId = int.Parse(mi.Settings["ContentId"].Value);
                        if (contentId > 0)
                        {
                            continue;
                        }
                        else
                        {
                            var content = new BOInternalContent();
                            content.Title = "";
                            content.SubTitle = "";
                            content.Teaser = "";
                            content.Html = "";
                            content.LanguageId = LanguageId;
                            intContentB.Change(content);
                            mi.Settings["ContentId"] = new BOSetting("ContentId", SettingTypeEnum.Int, content.ContentId.Value.ToString(), VisibilityEnum.SPECIAL);
                            ChangeModuleInstance(mi);
                        }
                    }
                }
            }
            return instances;
        }

        public void DeleteModuleInstance(int moduleInstanceId)
        {
            DeleteModuleInstanceHelper(moduleInstanceId, false);
        }

        public void UnDeleteModuleInstance(int moduleInstanceId)
        {
            DeleteModuleInstanceHelper(moduleInstanceId, true);
        }

        private void DeleteModuleInstanceHelper(int moduleInstanceId, bool unDelete)
        {
            if (PublishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOModuleInstance moduleInstance = GetModuleInstance(moduleInstanceId);
            moduleInstance.PendingDelete = !unDelete;
            ChangeModuleInstance(moduleInstance);
        }

        public BOModuleInstance GetModuleInstance(int moduleInstanceId, bool publishFlag)
        {
            return DbWebsite.GetModuleInstance(moduleInstanceId, publishFlag);
        }

        public BOModuleInstance GetModuleInstance(int moduleInstanceId)
        {
            return this.GetModuleInstance(moduleInstanceId, PublishFlag);
        }

        public void ChangeModuleInstanceSettings(Dictionary<string, BOSetting> entries, int moduleInstanceId)
        {
            BOModuleInstance mi = DbWebsite.GetModuleInstance(moduleInstanceId, PublishFlag);
            mi.Settings = entries;
            ChangeModuleInstance(mi);
        }

        #endregion

        public IEnumerable<BOWebSite> List()
        {
            List<BOWebSite> websites = cache.Get<List<BOWebSite>>(CACHE_SITE_LIST);
            if (websites == null)
            {
                websites = webSiteDb.List();
                
                lock (cacheLockingWebSiteList)
                {
                    List<BOWebSite> tempWebsites = cache.Get<List<BOWebSite>>(CACHE_SITE_LIST);
                    if (null == tempWebsites)
                        cache.Put(CACHE_SITE_LIST, websites);
                }
            }
            if (websites != null)
            {
                foreach (BOWebSite site in websites)
                {
                    site.LoadContent(intContentB.Get(site.ContentId.Value, site.LanguageId));
                }
            }
            return websites;
        }

        public void ChangeWebsite(BOWebSite website)
        {
            intContentB.Change(website);
            webSiteDb.Change(website);
            cache.Remove(CACHE_SITE_LIST);
        }

        internal void ChangeWebsite(BOWebSite website, string connString)
        {
            intContentB.Change(website, connString);
            webSiteDb.Change(website, connString);
            cache.Remove(CACHE_SITE_LIST);
        }

        public bool SiteExistsInDatabase(string title, string previewUrl)
        {
            var list = List();
            var result = list.Where(s => s.Title == title || s.PreviewUrl == previewUrl).FirstOrDefault();
            return result != null;
        }

        public enum AddWebSiteResult { Error, IisError, NotAllowed, SameNameExistsIis, SameNameExistsDatabase, FileSystemError, CreateDatabaseError, Success }

        public AddWebSiteResult AddWebSite(BOWebSite website, bool newDatabase, DirectoryInfo currentPhysicalRootPath, string connectString)
        {
            SqlConnectionStringBuilder builder = null;
            if (newDatabase && !string.IsNullOrWhiteSpace(connectString))
            {
                builder = new SqlConnectionStringBuilder(connectString);   
            }
            else if (newDatabase && string.IsNullOrWhiteSpace(connectString))
            {
                return AddWebSiteResult.Error;
            }
            else
            {
                builder = new SqlConnectionStringBuilder(SqlHelper.ConnStringMain);
            }
            var appPoolName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(appPoolName) || string.IsNullOrWhiteSpace(website.PreviewUrl) || website.Id > 0)
            {
                return AddWebSiteResult.Error;
            }
            if (SiteExistsInDatabase(website.Title, website.PreviewUrl))
            {
                return AddWebSiteResult.SameNameExistsDatabase;
            }
            // end of basic sanity check
            
            var siteUrl = new UrlBuilder(website.PreviewUrl);
            DirectoryInfo newWebsiteRoot = new DirectoryInfo(Path.Combine(currentPhysicalRootPath.Parent.FullName, siteUrl.Host));

            // DB POPULATE
            var newPassword = StringTool.RandomString(6, true);
            if (newDatabase)
            {
                var looksLikeOurs = CheckIfItLooksLikeOurDatabase(builder);
                if (!looksLikeOurs)
                { 
                    var result = PopulateNewDatabase(builder);
                    if (!result)
                        return AddWebSiteResult.CreateDatabaseError;
                }

                // if this blows then it wasn't really our DB
                AddDatabaseUser(builder, appPoolName, newPassword);

                builder = new SqlConnectionStringBuilder(connectString);
                builder.UserID = appPoolName;
                builder.Password = newPassword;
            }

            // IIS CREATE
            using (var sm = new ServerManager())
            {
                if (!sm.Sites.AllowsAdd)
                {
                    return AddWebSiteResult.NotAllowed;
                }
                if (sm.Sites.Any(t => t.Name == siteUrl.Host))
                {
                    return AddWebSiteResult.SameNameExistsIis;
                }
                if (!IISHelper.AppPoolExists(sm, appPoolName))
                {
                    IISHelper.CreateNewAppPool(sm, appPoolName, "v4.0");
                }
                IISHelper.CreateWebSite(sm, siteUrl.Host, appPoolName, website.PreviewUrl, newWebsiteRoot.FullName);
                sm.CommitChanges();
            }

            // FILES
            try
            {
                IISHelper.DirectoryCopy(currentPhysicalRootPath.FullName, newWebsiteRoot, copyAdminFolders: true);
                var dirSec = newWebsiteRoot.GetAccessControl();
                dirSec.SetAccessRuleProtection(false, true);
                dirSec.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
                newWebsiteRoot.SetAccessControl(dirSec);
            }
            catch (Exception ex)
            {
                log.Error(ex, "AddWebSite");
                return AddWebSiteResult.FileSystemError;
            }

            if (newDatabase)
            {
                ChangeWebsite(website, builder.ConnectionString);    
            }
            else
            {
                ChangeWebsite(website);
            }
            return AddWebSiteResult.Success;
        }

        public bool CreateNewDatabase(string connString)
        {
            var builder = new SqlConnectionStringBuilder(connString);
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                return false;
            var databaseSql = StringTool.GetTextContentFromResource("Sql.1_database.sql.template").Replace("@INITIAL_CATALOG@", builder.InitialCatalog);
            builder.InitialCatalog = "";

            try
            {
                SqlHelper.RunScript(builder.ConnectionString, databaseSql);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex, "PopulateNewDatabase");
            }
            return false;
        }

        public bool CheckIfItLooksLikeOurDatabase(SqlConnectionStringBuilder builder)
        {
            try 
            {
                var numberOfSettings = (int) SqlHelper.ExecuteScalar(builder.ConnectionString, CommandType.Text, "SELECT COUNT(*) [dbo].[settings_list]");
                return numberOfSettings > 10;
            }
            catch(Exception ex)
            {
                log.Info(ex, "CheckIfItLooksLikeOurDatabase says non likely");
                return false;
            }
        }

        private bool AddDatabaseUser(SqlConnectionStringBuilder builder, string newUsername, string newPassword)
        {
            var databaseName = builder.InitialCatalog;

            try
            {
                // if this fails because user already exists.. perahps we can move on.
                SqlHelper.ExecuteNonQuery(builder.ConnectionString, CommandType.Text, "CREATE LOGIN " + newUsername + " WITH PASSWORD = '" + newPassword + "'");

                using (var conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction tr = conn.BeginTransaction())
                    {
                        SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "CREATE USER " + newUsername + " FOR LOGIN " + newUsername + "");

                        SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "EXECUTE sp_addrolemember  [One.Net.FrontEnd], " + newUsername + "");
                        SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "EXECUTE sp_addrolemember  [One.Net.BackEnd], " + newUsername + "");
                        /*
                        SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "ALTER ROLE [One.Net.FrontEnd] ADD MEMBER  " + newUsername + "");
                        SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "ALTER ROLE [One.Net.BackEnd] ADD MEMBER  " + newUsername + ""); */
                        tr.Commit();
                        return true;
                    }

                }
            }
            catch (SqlException sex)
            {
                throw new Exception("AddDatabaseUser: " + newUsername, sex);
            }
        }

        private bool PopulateNewDatabase(SqlConnectionStringBuilder builder) 
        {

            var tablesSql = StringTool.GetTextContentFromResource("Sql.2_tables.sql.template").Replace("@INITIAL_CATALOG@", builder.InitialCatalog);
            var insertsSql = StringTool.GetTextContentFromResource("Sql.3_standard_inserts.sql.template").Replace("@INITIAL_CATALOG@", builder.InitialCatalog);
            var spsSql = StringTool.GetTextContentFromResource("Sql.4_stored_procedures.sql.template").Replace("@INITIAL_CATALOG@", builder.InitialCatalog);
            var securitySql = StringTool.GetTextContentFromResource("Sql.5_security.sql.template").Replace("@INITIAL_CATALOG@", builder.InitialCatalog);
            try
            {
                SqlHelper.RunScript(builder.ConnectionString, tablesSql);
                SqlHelper.RunScript(builder.ConnectionString, insertsSql);
                SqlHelper.RunScript(builder.ConnectionString, spsSql);
                SqlHelper.RunScript(builder.ConnectionString, securitySql);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex, "PopulateNewDatabase");
            }
            return false;
        }

       

        public void ChangeSettings(Dictionary<string, BOSetting> entries, int websiteId)
        {
            BOWebSite site = Get(websiteId);
            if (site != null)
            {
                site.Settings = entries;
                ChangeWebsite(site);
            }
        }
        
        public static BOTemplate GetTemplate(int id)
        {
            BOImageTemplate imageTemplate = cache.Get<BOImageTemplate>("Template_" + id);
            if (imageTemplate != null)
                return imageTemplate;

            BOTemplate template = cache.Get<BOTemplate>("Template_" + id);
            if (template == null)
            {
                template = ListTemplates(null).Where(t => t.Id == id).FirstOrDefault();
                if (template != null && template.Type == "ImageTemplate")
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Write(template.Source, 0, template.Source.Length);
                        stream.Seek(0, SeekOrigin.Begin);
                        var formatter = new LoosyFormatter();
                        template = (BOImageTemplate)formatter.Deserialize(stream);
                    }
                }
                if (template != null)
                {
                    lock (cacheLockingTemplatesList)
                    {
                        var temp = cache.Get<BOTemplate>("Template_" + id);
                        if (null == temp)
                            cache.Put("Template_" + id, template);
                    }
                }
            }

            return template;
        }

        /// <summary>
        /// Lists all templates by template type
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static List<BOTemplate> ListTemplates(string typeIdsString)
        {
            List<BOTemplate> list = cache.Get<List<BOTemplate>>("ListTemplates:" + typeIdsString);
            if (list == null)
            {
                list = webSiteDb.ListTemplates();
                if (null != list)
                {
                    lock (cacheLockingTemplatesList)
                    {
                        List<BOTemplate> tempList = cache.Get<List<BOTemplate>>("ListTemplates:" + typeIdsString);
                        if (null == tempList)
                            cache.Put("ListTemplates:" + typeIdsString, list);
                    }
                    if (log.IsDebugEnabled)
                        log.Debug("ListTemplates found " + list.Count + " templates.");
                }
            }

            if (!string.IsNullOrEmpty(typeIdsString) && null != list)
            {
                var typeIds = typeIdsString.Split(';');
                List<BOTemplate> result = new List<BOTemplate>();
                foreach (BOTemplate template in list)
                {
                    foreach (var typeId in typeIds)
                    {
                        if (string.Equals(typeId, template.Type, StringComparison.InvariantCultureIgnoreCase))
                            result.Add(template);
                    }
                }
                return result;
            }
            else
                return list;
        }

        public void ChangeTemplate(BOTemplate template)
        {
            webSiteDb.ChangeTemplate(template);
            cache.RemoveWithPartialKey("ListTemplates");
        }

        public void DeleteTemplate(int templateId)
        {
            webSiteDb.DeleteTemplate(templateId);
            cache.RemoveWithPartialKey("ListTemplates");
        }

        /// <summary>
        /// List all availible placeholders in this system.
        /// </summary>
        /// <returns></returns>
        public static List<BOPlaceHolder> ListPlaceHolders()
        {
            List<BOPlaceHolder> list = cache.Get<List<BOPlaceHolder>>("ListPlaceHolders");
            if (list == null)
            {
                list = webSiteDb.ListPlaceHolders();
                cache.Put("ListPlaceHolders", list);
            }

            return list;
        }

        public void ChangePlaceHolder(BOPlaceHolder placeHolder)
        {
            webSiteDb.ChangePlaceHolder(placeHolder);
            cache.Remove("ListPlaceHolders");
        }

        /// <summary>
        /// Lists all availible modules on this system.
        /// </summary>
        /// <returns></returns>
        public static List<BOModule> ListModules(bool includeUsageCount = false)
        {
            var cacheKey = "ListModules" + includeUsageCount.ToString();
            var c = cache.Get<List<BOModule>>(cacheKey);
            if (c == null)
            { 
                c = webSiteDb.ListModules(includeUsageCount);
                cache.Put(cacheKey, c);
            }
            return c;
        }

        public static DataTable ListAvailibleModules(HttpContext context)
        {
            var coreModules = BFileSystem.ListPhysicalFolder(context.Server.MapPath("~/CommonModules/"), context.Server.MapPath("~/")).Where(fi => fi.Extension == ".ascx");
            var customModules = BFileSystem.ListPhysicalFolder(context.Server.MapPath("~/site_specific/custom_modules"), context.Server.MapPath("~/")).Where(fi => fi.Extension != null && fi.Extension == ".ascx");

            var diskModules = coreModules.Concat(customModules).ToList();
            var databaseModules = ListModules(true).OrderByDescending(m => m.NoUnpublishedInstances);

            var dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            dt.Columns.Add("NoUnpublishedInstances");
            dt.Columns.Add("NoPublishedInstances");
            dt.Columns.Add("NoSettingsInDatabase");
            dt.Columns.Add("NoSettingsInModule");

            foreach (var m in databaseModules)
            {
                var dr = dt.NewRow();
                dr["Id"] = m.Id;
                dr["Name"] = m.Name;
                dr["NoUnpublishedInstances"] = m.NoUnpublishedInstances;
                dr["NoPublishedInstances"] = m.NoPublishedInstances;
                dr["NoSettingsInDatabase"] = m.NoSettingsInDatabase;


                var onDisk = diskModules.Where(fi => fi.Name == m.Name + ".ascx").FirstOrDefault();
                if (onDisk != null)
                {
                    diskModules.Remove(onDisk);
                }
                dt.Rows.Add(dr);
            }
            foreach (var fi in diskModules)
            {
                var dr = dt.NewRow();
                dr["Name"] = Path.GetFileNameWithoutExtension(fi.Name);
                dt.Rows.InsertAt(dr, 0);
            }
            return dt;
        }

        

        public DataTable ListModuleUsage(int id)
        {
            var pages = DbWebsite.ListTopModuleUsage(id);

            var result = pages.Clone();
            result.Columns.Add("url");

            foreach(DataRow p in pages.Rows)
            {
                var r = result.NewRow();
                var uri = GetPageUri((int)p["id"]);
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    r["url"] = uri;
                    r["id"] = p["id"];
                    result.Rows.Add(r);
                }
                
            }
            return result;
        }

        public static bool AddModule(string moduleName)
        {
            var module = ListModules().Where(m => m.Name == moduleName).FirstOrDefault();
            if (module != null)
                return false;

            if (moduleName.EndsWith("ascx"))
                return false;

            return DbWebsite.AddModule(moduleName);
        }

        public static int UpdateModuleSettings(string moduleName, IEnumerable<Setting> settings)
        {
            var module = ListModules().Where(m => m.Name == moduleName).FirstOrDefault();
            if (module == null || string.IsNullOrWhiteSpace(module.Name))
                return -1;

            foreach (var s in settings)
            {
                if (string.IsNullOrWhiteSpace(s.Name) || s.Name.Equals("Page", StringComparison.InvariantCultureIgnoreCase) || s.Name.Equals("Website", StringComparison.InvariantCultureIgnoreCase))
                    return -1;
            }



            return DbWebsite.UpdateModuleSettings(module.Name, settings);
        }

        #region Old Stuff

        public bool ValidateParLinkAgainstDB(int? parentPageId, int pageId, string parLink, int websiteId)
        {
            return webSiteDb.ValidateParLink(parentPageId, pageId, parLink, websiteId);
        }


        public bool ValidateParLinkSyntax(string parLink)
        {
            bool valid = false;
            Regex reg = new Regex("[A-Za-z0-9]");
            valid = reg.IsMatch(parLink);
            return valid;
        }

        #endregion

        #region Moving sites

        public void CopyToWebsite(int toWebsiteId, int fromPageId, int fromWebsiteId)
        {
            int? toRootPageId = GetRootPageId(toWebsiteId);

            if (toRootPageId.HasValue)
            {
                log.Info("MoveToWebsite failed, existing root page id found for websiteId: " + toWebsiteId);
            }
            else
            {
                var fromWebsite = this.Get(fromWebsiteId);
                var toWebsite = this.Get(toWebsiteId);
                var fromPage = this.GetPage(fromPageId);

                if (fromWebsite != null && toWebsite != null && fromPage != null)
                {
                    CopyPageToWebsite(toWebsiteId, toWebsite.LanguageId, fromPageId, fromWebsite.LanguageId, fromPage.URI, null, 0);
                }
            }
        }

        private void CopyPageToWebsite(int toWebsiteId, int toLanguageId, int pageId, int fromLanguageId, string fromPageUri, int? newParentPageId, int newLevel)
        {
            var currentPage = GetUnCachedPage(pageId, false, fromLanguageId);

            if (currentPage != null)
            {
                var oldLevel = currentPage.Level;
                currentPage.Level = newLevel;
                var levelDifference = oldLevel - newLevel;
                currentPage.LanguageId = toLanguageId;
                currentPage.ContentId = null;
                currentPage.DateModified = null;
                currentPage.Id = 0;
                currentPage.IsChanged = true;
                currentPage.IsNew = true;
                currentPage.ParentId = newParentPageId;
                currentPage.WebSiteId = toWebsiteId;

                if (newParentPageId == null)
                    currentPage.ParLink = "";

                this.ChangePage(currentPage);
                var newPageId = currentPage.Id;

                var moduleInstances = ListModuleInstances(pageId);

                foreach (var instance in moduleInstances)
                {
                    if (!instance.IsInherited)
                    {
                        var oldInstanceId = instance.Id;
                        instance.Id = 0;
                        instance.PageId = newPageId;
                        instance.Changed = true;
                        instance.PublishFlag = false;

                        instance.PersistFrom = (instance.PersistFrom - levelDifference) >= 0 ? instance.PersistFrom - levelDifference : 0;
                        instance.PersistTo = (instance.PersistTo - levelDifference) >= 0 ? instance.PersistTo - levelDifference : 0;

                        if (instance != null && instance.Settings != null)
                        {
                            if (instance.Settings.ContainsKey("ContentId"))
                            {
                                int contentId = int.Parse(instance.Settings["ContentId"].Value);
                                if (contentId > 0)
                                {
                                    var content = intContentB.GetUnCached(contentId);
                                    content.ContentId = null;
                                    content.LanguageId = toLanguageId;
                                    intContentB.Change(content);
                                    instance.Settings["ContentId"].Value = content.ContentId.Value.ToString();
                                }
                            }

                            foreach (var key in instance.Settings.Keys)
                            {
                                if (key == "SingleArticleUri" ||
                                    key == "ArticleListUri" ||
                                    key == "SingleEventUri" ||
                                    key == "SingleLocationUri")
                                {
                                    if (instance.Settings[key].Value.StartsWith(fromPageUri))
                                    {
                                        instance.Settings[key].Value = instance.Settings[key].Value.Remove(0, fromPageUri.Length);
                                    }
                                }
                            }
                        }

                        ChangeModuleInstance(instance, toLanguageId, false);

                        var dictionaryList = contentB.ListDictionaryEntries(fromLanguageId, "_" + oldInstanceId.ToString());
                        foreach (var entry in dictionaryList)
                        {
                            entry.ContentId = null;
                            entry.LanguageId = toLanguageId;
                            if (entry.KeyWord.EndsWith("_" + oldInstanceId))
                            {
                                entry.KeyWord = entry.KeyWord.Replace("_" + oldInstanceId, "_" + instance.Id);
                                contentB.ChangeDictionaryEntry(entry);
                            }
                        }

                        log.Info("Module instance migrate. Old instance id:" + oldInstanceId + " new instance id:" + instance.Id.ToString());
                    }
                }

                var childList = ListChildrenIds(pageId);

                foreach (var id in childList)
                {
                    CopyPageToWebsite(toWebsiteId, toLanguageId, id, fromLanguageId, fromPageUri, newPageId, newLevel + 1);
                }
            }
        }

        #endregion Moving sites

        public string GetGoogleSiteMap(int webSiteId)
        {
            var website = Get(webSiteId);
            if (website == null)
                return "";

            var rootUrl = (PublishFlag ? website.ProductionUrl : website.PreviewUrl).TrimEnd('/');

            BOPage _root = null;

            List<BOPage> pages = webSiteDb.ListPages(webSiteId, Thread.CurrentThread.CurrentCulture.LCID, PublishFlag);

            var siteMap = new Dictionary<int, BOPage>();

            foreach (var p in pages)
            {
                if (_root == null && p.IsRoot)
                {
                    p.URI = "/";
                    _root = p;
                    siteMap.Add(p.Id, p);
                }
                else if (_root != null)
                {
                    var parent = siteMap[p.ParentId.Value];
                    if (parent != null)
                    {
                        
                        p.URI = parent.IsRoot ? ("/" + p.ParLink) : (parent.URI + "/" + p.ParLink);
                        siteMap.Add(p.Id, p);
                    }
                }
            }


            var builder = new StringWriterWithEncoding(Encoding.UTF8);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            settings.CloseOutput = true;

            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var p in siteMap.Values)
                {
                    if (!p.IsRedirected && p.RobotsIndex)
                    {
                        writer.WriteStartElement("url");
                        writer.WriteElementString("loc", rootUrl + p.URI.Trim().Trim('\n').Trim('\r'));
                        //writer.WriteElementString("lastmod", p.LastChanged.ToString("yyyy-mm-dd"));
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return builder.ToString();
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}