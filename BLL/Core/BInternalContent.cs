using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    /// <summary>
    /// This class is strictly to be initialized and used by BLL. All access to content should be routed trough this class
    /// </summary>
    [Serializable]
    public class BInternalContent : BusinessBaseClass
    {
        protected const string CACHE_ID = "BOInternalContent_";

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingInternalContent = new Object();

        public BOInternalContent Get(int contentID)
        {
            return Get(contentID, LanguageId);
        }

        public BOInternalContent Get(int contentID, int languageID)
        {
            string cacheKey = CACHE_LANG_PREFIX + CACHE_ID + contentID;

            var content = cache.Get<BOInternalContent>(cacheKey);
            if (content == null)
            {
                content = GetUnCached(contentID, languageID);
                if (content != null)
                {
                    // under heavy load mulitple requests will start the reading process, then the Cache
                    // inserts will invalidate cache. Better to check for existence first.
                    lock (cacheLockingInternalContent)
                    {
                        var tempContent = cache.Get<BOInternalContent>(cacheKey);
                        if (null == tempContent)
                            cache.Put(cacheKey, content);
                    }
                }
            }
            else
            {
                // cached content needs to have BuildInternalStructures called against it to populate Images and other structures.
                content.Teaser = content.Teaser;
                content.Html = content.Html;
            }
            return content;
        }

        static internal string GetContentTitleInAnyLanguage(int contentId)
        {
            return contentDb.GetContentTitleInAnyLanguage(contentId);
        }

        public BOInternalContent GetUnCached(int contentID)
        {
            return GetUnCached(contentID, LanguageId);
        }

        public List<BOInternalContentAudit> ListAudits(int contentId, int languageId)
        {
            return contentDb.ListAudits(contentId, languageId);
        }

        public BOInternalContentAudit GetAudit(string guid)
        {
            return contentDb.GetAudit(guid);
        }

        public BOInternalContent GetUnCached(int contentID, int languageID)
        {
            if (contentID < 1)
                return null;

            BOInternalContent content = contentDb.GetContent(contentID, languageID);
            if (content == null)
            {
                content = new BOInternalContent();
                content.ContentId = contentID;
                content.Title = "";
                content.SubTitle = "";
                content.Teaser = "";
                content.Html = "";
                content.LanguageId = languageID;
                content.MissingTranslation = true;
            }
            return content;
        }


        /// <summary>
        /// Clones and owerwrites all content on target content contentId. Use with caution!
        /// </summary>
        /// <param name="offlineContentId">Source</param>
        /// <param name="onlineContentId">Target</param>
        public void CloneContent(int offlineContentId, int onlineContentId)
        {
            foreach (int langId in ListLanguages())
            {
                BOInternalContent cont = contentDb.GetContent(offlineContentId, langId);
                if (null != cont && !cont.MissingTranslation)
                {
                    BOInternalContent publishedContent = new BOInternalContent();
                    cont.CloneContent(publishedContent);

                    log.Info("Method:CloneContent Cloned Offline Content:{" + offlineContentId + "} to Online Content:{" + onlineContentId + "}");
                    
                    publishedContent.ContentId = onlineContentId;
                    contentDb.ChangeContent(publishedContent);
                    contentDb.AuditContent(publishedContent);   
                }
            }
        }

        public bool Change(BOInternalContent content)
        {
            content.MissingTranslation = false;
            var result = contentDb.ChangeContent(content);
            contentDb.AuditContent(content);
            ClearLanguageVariations(CACHE_ID + content.ContentId.Value);
            return result;
        }

        internal void Change(BOInternalContent content, string connString)
        {
            content.MissingTranslation = false;
            contentDb.ChangeContent(content, connString);
        }

        public void Delete(int contentId)
        {
            contentDb.Delete(contentId);
        }
    }
}
