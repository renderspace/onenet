using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
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

namespace One.Net.BLL
{
    [Serializable]
    public class BWebsite : BusinessBaseClass
    {
        public const string CACHE_SITE_LIST = "List<BOWebSite> List()";

        protected new static readonly ILog log = LogManager.GetLogger(typeof(BWebsite));
        static readonly DbWebsite webSiteDb = new DbWebsite();
        readonly BInternalContent intContentB = new BInternalContent();
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
            string cacheKey = PAGE_CACHE_ID(pageId, languageId, publishFlag);
            BOPage page = OCache.Get(cacheKey) as BOPage;

            if (page == null)
            {
                log.Info("Page MISS [" + cacheKey + "]");
                page = webSiteDb.GetPage(pageId, publishFlag, languageId);
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
                        BOPage tempPage = OCache.Get(cacheKey) as BOPage;
                        if (null == tempPage)
                            OCache.Max(cacheKey, page);
                    }
                }
            }
            return page;
        }

        private BOPage GetUnCachedPage(int pageId, bool publish)
        {
            BOPage page = webSiteDb.GetPage(pageId, publish, LanguageId);

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

        private BOPage GetUnCachedPage(int pageId, int languageId, bool publish)
        {
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
            return webSiteDb.ListChildrenIds(pageId, publishFlag);
        }

        public string GetPageUri(int pageId)
        {
            BOPage page = GetPage(pageId, LanguageId);
            return page.URI;
        }

        public enum AddSubPageResult { Ok, OkRootPage, PartialLinkNotValid, PartialLinkExistsOnThisLevel, TriedToAddRootPageToNonEmptySite, NoTemplates }

        public AddSubPageResult AddSubPage(string requestedPageTitle, int webSiteId, int currentPageId)
        {
            BOPage parentPageModel = GetPage(currentPageId);
            BOWebSite site = Get(webSiteId);
            if (site.LanguageId < 1)
            {
                site.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            }
            int newOrder = 0;
            string newParLink = "";

            newParLink = PrepareParLink(requestedPageTitle);
            if (!ValidateParLinkSyntax(newParLink))
                return AddSubPageResult.PartialLinkNotValid;

            if (parentPageModel != null) // Not root
            {
                // determine page order
                List<BOPage> pages = ListChildrenPages(parentPageModel.Id);
                if (pages != null && pages.Count > 0)
                {
                    newOrder = pages[pages.Count - 1].Order + 1;
                }
            }
            else
            {
                currentPageId = -1;
            }

            if (!ValidateParLinkAgainstDB(currentPageId == -1 ? (int?)null : currentPageId, -1, newParLink, webSiteId))
                return AddSubPageResult.PartialLinkExistsOnThisLevel;
            bool addingRootPage = false;
            if (currentPageId == -1)
            {
                addingRootPage = true;
                // check if there are no existing pages on this website
                if (GetSiteStructure(webSiteId).Count != 0)
                {
                    return AddSubPageResult.TriedToAddRootPageToNonEmptySite;
                }
            }

            List<BOTemplate> templateData = ListTemplates("3");
            int templateID = 1;
            if (templateData.Count > 0)
            {
                templateID = templateData[0].Id.Value;
            }
            else
            {
                return AddSubPageResult.NoTemplates;
            }

            var page = new BOPage
            {
                Title = requestedPageTitle,
                Template = new BOTemplate { Id = templateID },
                PublishFlag = false,
                LanguageId = Thread.CurrentThread.CurrentCulture.LCID,
                MenuGroup = 0,
                WebSiteId = webSiteId,
                BreakPersistance = false,
                ParLink = newParLink,
                Order = newOrder,
                RedirectToUrl = ""
            };

            if (currentPageId > 0)
            {
                page.ParentId = currentPageId;
            }

            ChangePage(page);
            OneSiteMapProvider.ReloadSiteMap();
            return addingRootPage ? AddSubPageResult.OkRootPage : AddSubPageResult.Ok;
        }

        public string PrepareParLink(string parLink)
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
            if (publishFlag)
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            if (page == null)
                return;
            page.SubTitle = "";
            page.Html = "";

            if (!publishing)
                page.IsChanged = true;

            intContentB.Change(page);
            if (page.IsRoot)
            {
                page.Level = 0;
                page.URI.TrimStart(new char[] { '/' });
                OCache.Remove(CACHE_SITE_LIST);
            }
            else
            {
                page.Level = webSiteDb.GetPage(page.ParentId.Value, page.PublishFlag, page.LanguageId).Level + 1;
            }
            webSiteDb.ChangePage(page);
            OCache.Remove(PAGE_CACHE_ID(page.Id, page.LanguageId, page.PublishFlag));
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
            if (publishFlag)
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
            if (publishFlag)
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
                OCache.Remove(CACHE_SITE_LIST);

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
            if (publishFlag)
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
                OCache.Remove(CACHE_SITE_LIST);

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
                OCache.Remove(PAGE_CACHE_ID(offlinePage.Id, offlinePage.LanguageId, offlinePage.PublishFlag));
                return true;
            }
            else
                return false;
        }

        public bool PublishPage(int pageId)
        {
            if (publishFlag)
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
                            else if (instance.Name == "TextContent")
                                PublishTextContentModuleInstance(instance, page.LanguageId);
                            else if (instance.Name == "SpecialContent")
                                PublishTextContentModuleInstance(instance, page.LanguageId);
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

        /*
        public void PublishPageTree(int pageId)
        {
            PublishPage(pageId);
            List<BOPage> children = ListChildrenPages(pageId);
            foreach (BOPage page in children)
            {
                PublishPageTree(page.Id);
            }
        }
        */
        private void PublishTextContentModuleInstance(BOModuleInstance instance, int languageId)
        {
            instance.Changed = false;
            ChangeModuleInstance(instance, languageId, true);

            int previewContentId = int.Parse(instance.Settings["ContentId"].Value);
            BOInternalContent content = intContentB.Get(previewContentId, languageId);

            if (content == null)
                return; // nothing to get published.

            BOModuleInstance publishedInstance = webSiteDb.GetModuleInstance(instance.Id, true);
            if (publishedInstance != null)
                content.ContentId = int.Parse(publishedInstance.Settings["ContentId"].Value);
            else
                content.ContentId = null;

            intContentB.Change(content);
            publishedInstance = instance;

            publishedInstance.Settings["ContentId"] = new BOSetting { Name = "ContentId", Type = "Int", Value = content.ContentId.Value.ToString(), UserVisibility = BOSetting.USER_VISIBILITY_SPECIAL };
            publishedInstance.PublishFlag = true;
            ChangeModuleInstance(publishedInstance, languageId, true);
        }

        public List<BOPage> GetSiteStructure(int webSiteId)
        {
            return webSiteDb.GetSiteStructure(webSiteId, LanguageId, publishFlag);
        }

        public PagedList<BOPage> ListUnpublishedPages(int webSiteId, ListingState state)
        {
            return webSiteDb.ListUnpublishedPages(webSiteId, state, LanguageId);
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
                webSiteDb.ChangeModuleInstance(instance);
            }
            else
            {
                instance.Changed = true;
                BOPage containingPage = this.GetPage(instance.PageId, languageId);
                BOModuleInstance existingInstance = this.GetModuleInstance(instance.Id);
                if (existingInstance != null)
                {
                    // instance exists
                    if (existingInstance.PlaceHolderId == instance.PlaceHolderId)
                    {
                        // PlaceHolderId hasn't changed, so just changed the instance.
                        webSiteDb.ChangeModuleInstance(instance);
                    }
                    else
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

                        webSiteDb.ChangeModuleInstance(instance);
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

                        // instance.Order = containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances[containingPage.PlaceHolders[instance.PlaceHolderId].ModuleInstances.Count - 1].Order + 1;
                    }
                    else
                        instance.Order = 0;

                    webSiteDb.ChangeModuleInstance(instance);
                }

                // Mark page as changed
                containingPage.IsChanged = true;
                containingPage.PublishFlag = false;
                ChangePage(containingPage);
            }
        }

        public List<BOModuleInstance> ListModuleInstances(int pageId)
        {
            List<BOModuleInstance> instances = new List<BOModuleInstance>();
            BOPage page = GetPage(pageId);
            if (page == null)
                return instances;

            foreach (BOPlaceHolder placeholder in page.PlaceHolders.Values)
            {
                foreach (BOModuleInstance instance in placeholder.ModuleInstances)
                {
                    instances.Add(instance);
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
            if (publishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOModuleInstance moduleInstance = GetModuleInstance(moduleInstanceId);
            moduleInstance.PendingDelete = !unDelete;
            ChangeModuleInstance(moduleInstance);
        }

        public BOModuleInstance GetModuleInstance(int moduleInstanceId, bool publishFlag)
        {
            return webSiteDb.GetModuleInstance(moduleInstanceId, publishFlag);
        }

        public BOModuleInstance GetModuleInstance(int moduleInstanceId)
        {
            return this.GetModuleInstance(moduleInstanceId, publishFlag);
        }

        public void ChangeModuleInstanceSettings(Dictionary<string, BOSetting> entries, int moduleInstanceId)
        {
            BOModuleInstance mi = webSiteDb.GetModuleInstance(moduleInstanceId, publishFlag);
            mi.Settings = entries;
            ChangeModuleInstance(mi);
        }

        #endregion

        public List<BOWebSite> List()
        {
            List<BOWebSite> websites = OCache.Get(CACHE_SITE_LIST) as List<BOWebSite>;
            if (websites == null)
            {
                websites = webSiteDb.List();
                
                lock (cacheLockingWebSiteList)
                {
                    List<BOWebSite> tempWebsites = OCache.Get(CACHE_SITE_LIST) as List<BOWebSite>;
                    if (null == tempWebsites)
                        OCache.Max(CACHE_SITE_LIST, websites);
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
            OCache.Remove(CACHE_SITE_LIST);
        }

        internal void ChangeWebsite(BOWebSite website, string connString)
        {
            intContentB.Change(website, connString);
            webSiteDb.Change(website, connString);
            OCache.Remove(CACHE_SITE_LIST);
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
                    IISHelper.CreateNewAppPool(sm, appPoolName, "v4.5");
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
                log.Error("AddWebSite", ex);
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

            // EDIT web.config
            using (var sm = new ServerManager())
            {
                if (newDatabase)
                {
                    IISHelper.AddConnectionString(sm, siteUrl.Host, "MsSqlConnectionString", builder.ConnectionString);
                }
                IISHelper.AddAppSetting(sm, siteUrl.Host, "WebSiteId", website.Id.ToString());
                sm.CommitChanges();
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
                log.Error("PopulateNewDatabase", ex);
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
                log.Info("CheckIfItLooksLikeOurDatabase says non likely", ex);
                return false;
            }
        }

        private bool AddDatabaseUser(SqlConnectionStringBuilder builder, string newUsername, string newPassword)
        {
            var databaseName = builder.InitialCatalog;

            // if this fails because user already exists.. perahps we can move on.
            SqlHelper.ExecuteNonQuery(builder.ConnectionString, CommandType.Text, "CREATE LOGIN " + newUsername + " WITH PASSWORD = '" + newPassword + "'");

            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction tr = conn.BeginTransaction())
                 {
                    SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "CREATE USER " + newUsername + " FOR LOGIN " + newUsername + "");
                    SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "ALTER ROLE [One.Net.FrontEnd] ADD MEMBER  " + newUsername + "");
                    SqlHelper.ExecuteNonQuery(tr, CommandType.Text, "ALTER ROLE [One.Net.BackEnd] ADD MEMBER  " + newUsername + "");
                    tr.Commit();
                    return true;
                 }
            
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
                log.Error("PopulateNewDatabase", ex);
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
            List<BOTemplate> list = ListTemplates(null);
            foreach (BOTemplate template in list)
            {
                if (template.Id == id && template.Type == "ImageTemplate")
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Write(template.Source, 0, template.Source.Length);
                        stream.Seek(0, SeekOrigin.Begin);
                        var formatter = new LoosyFormatter();
                        return (BOImageTemplate)formatter.Deserialize(stream);
                    }
                }
                else if (template.Id == id)
                    return template;
            }
            return null;
        }

        /// <summary>
        /// Lists all templates by template type
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static List<BOTemplate> ListTemplates(string typeId)
        {
            List<BOTemplate> list = OCache.Get("ListTemplates") as List<BOTemplate>;
            if (list == null)
            {
                list = webSiteDb.ListTemplates();
                if (null != list)
                {
                    lock (cacheLockingTemplatesList)
                    {
                        List<BOTemplate> tempList = OCache.Get("ListTemplates") as List<BOTemplate>;
                        if (null == tempList)
                            OCache.Max("ListTemplates", list);
                    }
                    if (log.IsDebugEnabled)
                        log.Debug("ListTemplates found " + list.Count + " templates.");
                }
            }

            if (!string.IsNullOrEmpty(typeId) && null != list)
            {
                List<BOTemplate> result = new List<BOTemplate>();
                foreach (BOTemplate template in list)
                {
                    if (string.Equals(typeId, template.Type, StringComparison.InvariantCultureIgnoreCase))
                        result.Add(template);
                }
                return result;
            }
            else
                return list;
        }

        public void ChangeTemplate(BOTemplate template)
        {
            webSiteDb.ChangeTemplate(template);
        }

        /// <summary>
        /// List all availible placeholders in this system.
        /// </summary>
        /// <returns></returns>
        public List<BOPlaceHolder> ListPlaceHolders()
        {
            List<BOPlaceHolder> list = OCache.Get("ListPlaceHolders") as List<BOPlaceHolder>;
            if (list == null)
            {
                list = webSiteDb.ListPlaceHolders();
                OCache.Max("ListPlaceHolders", list);
            }

            return list;
        }

        public void ChangePlaceHolder(BOPlaceHolder placeHolder)
        {
            webSiteDb.ChangePlaceHolder(placeHolder);
            OCache.Remove("ListPlaceHolders");
        }

        /// <summary>
        /// Lists all availible modules on this system.
        /// </summary>
        /// <returns></returns>
        public static List<BOModule> ListModules()
        {
            return webSiteDb.ListModules();
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
            var currentPage = GetUnCachedPage(pageId, fromLanguageId, false);

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

            var rootUrl = publishFlag ? website.ProductionUrl : website.PreviewUrl;

            BOPage _root = null;

            var state = new ListingState(10000, 0, SortDir.Ascending, "PageId");

            List<BOPage> pages = webSiteDb.ListPages(webSiteId, state, Thread.CurrentThread.CurrentCulture.LCID, publishFlag);

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


            var builder = new StringBuilder();

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
}