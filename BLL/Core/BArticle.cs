using System;
using System.Collections.Generic;
using System.Transactions;
using One.Net.BLL.DAL;

namespace One.Net.BLL
{
    [Serializable]
    public class BArticle : BusinessBaseClass, IPublisher
    {
        private static readonly DbArticle articleDB = new DbArticle();
        private static readonly BInternalContent contentB = new BInternalContent();

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingArticleList = new Object();
        private static readonly object cacheLockingArticleGet = new Object();
        private static readonly object cacheLockingArticleDateListGet = new Object();

        #region Article methods

        /// <summary>
        /// Changes underlying BOInternalContent object and changes BOArticle object.
        /// Performs article actions based on article.PublishFlag.
        /// Removes object from Cache then adds new object to Cache.
        /// </summary>
        public void ChangeArticle(BOArticle article)
        {
            if (article == null)
                throw new ArgumentException("article is null");

            // retrieve existing content id from db in case its not provided
            if (article.Id.HasValue)
            {
                BOArticle existingArticle = GetUnCachedArticle(article.Id.Value, article.PublishFlag, true);
                if (existingArticle != null && existingArticle.Id.HasValue && existingArticle.ContentId.HasValue)
                    article.ContentId = existingArticle.ContentId;
            }

            contentB.Change(article);   // change the underlying content

            articleDB.ChangeArticle(article); // change the article
            ClearCache(article.Id.Value);
        }

        /// <summary>
        /// Retrieves BOArticle object based on id and publishFlag.
        /// If publishFlag is true, an attempt is made to return a fully cached object. If object is not cached, an uncached object is returned.
        /// If publishFlag is false, an uncached object is returned.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        public BOArticle GetArticle(int id, bool showUntranslated)
        {
            return GetArticle(id, publishFlag, showUntranslated);
        }

        /// <summary>
        /// Retrieves BOArticle object based on id and publishFlag.
        /// If publishFlag is true, an attempt is made to return a fully cached object. If object is not cached, an uncached object is returned.
        /// If publishFlag is false, an uncached object is returned.
        /// </summary>        /// <param name="articleId"></param>
        /// <param name="publish"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        private BOArticle GetArticle(int articleId, bool publish, bool showUntranslated)
        {
            BOArticle article = null;
            bool useCache = !showUntranslated;
            string cacheKey = CACHE_LANG_PREFIX + ARTICLE_CACHE_ID(articleId, publish);
            if (useCache)
                article = OCache.Get(cacheKey) as BOArticle;

            if (article == null)
            {
                article = GetUnCachedArticle(articleId, publish, showUntranslated);

                if (article != null && !article.MissingTranslation && useCache)
                {
                    lock (cacheLockingArticleGet)
                    {
                        BOArticle tempArticle = OCache.Get(cacheKey) as BOArticle;
                        if (null == tempArticle)
                            OCache.Max(cacheKey, article); 
                    }
                }
            }

            return article;
        }

        /// <summary>
        /// Retrieves BOArticle object based on id and publishFlag.
        /// Loads object Content
        /// </summary>
        /// <param name="id"></param>
        /// <param name="publish"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        private BOArticle GetUnCachedArticle(int id, bool publish, bool showUntranslated)
        {
            BOArticle article = articleDB.GetArticle(id, publish, LanguageId, showUntranslated);

            if (article != null && article.Id.HasValue)
            {
                article.Regulars = articleDB.ListRegulars(new ListingState(SortDir.Ascending, "title"), showUntranslated, article.Id, article.PublishFlag, LanguageId);

                if (showUntranslated && article.ContentId.HasValue && article.MissingTranslation)
                    article.Title = BInternalContent.GetContentTitleInAnyLanguage(article.ContentId.Value);
            }

            return article;
        }

        /// <summary>
        /// Retrieves BOArticle object based on id and languageId.
        /// Note: This method is used by publisher only
        /// </summary>
        /// <param name="id"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        private BOArticle GetPublishableArticle(int id, bool publishFlag, int languageId)
        {
            return articleDB.GetArticle(id, publishFlag, languageId, false);
        }

        /// <summary>
        /// Retrieves paged list of unpublished, changed, translated articles based on ListingState and languageId
        /// Uses delegate SingleArticleGet to retrieve individual objects. 
        /// Individual objects are not cached
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public PagedList<BOArticle> ListUnpublishedArticles(ListingState state)
        {
            return articleDB.ListUnpublishedArticles(state, LanguageId);
        }

        public PagedList<BOArticle> ListArticles(string regularIds, DateTime? from, DateTime? to, ListingState state, string titleSearch)
        {
            PagedList<BOArticle> articles = null;
            if (string.IsNullOrEmpty(titleSearch))
                articles = articleDB.ListArticles(publishFlag, from, to, regularIds, state, LanguageId, false);
            else
            {
                var tempRegularList = StringTool.SplitStringToIntegers(regularIds);
                articles = articleDB.ListFilteredArticles(publishFlag, from, to, state, LanguageId, false, titleSearch, tempRegularList);
            }

            foreach (BOArticle article in articles)
            {
                article.Regulars = articleDB.ListRegulars(new ListingState(SortDir.Ascending, "title"), false, article.Id, article.PublishFlag, LanguageId);
            }
            return articles;
        }

        /// <summary>
        /// Retrieves paged list of articles based on ListingState and publishFlag.
        /// Uses delegate SingleArticleGet to retrieve individual objects. 
        /// Caching of individual objects is based on publish flag.
        /// </summary>
        public PagedList<BOArticle> ListArticles(string regularIds, bool showUntranslated, ListingState state, string titleSearch, DateTime? requestedMonth, DateTime? requestedYear)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (requestedMonth.HasValue)
            {
                from = new DateTime(requestedMonth.Value.Year, requestedMonth.Value.Month, 1, 0, 0, 0);
                to = new DateTime(requestedMonth.Value.Year, requestedMonth.Value.Month,
                    System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetDaysInMonth(requestedMonth.Value.Year, requestedMonth.Value.Month), 23, 59, 59);
            }
            else if (requestedYear.HasValue)
            {
                from = new DateTime(requestedYear.Value.Year, 1, 1, 0, 0, 0);
                to = new DateTime(requestedYear.Value.Year, 12, 31, 23, 59, 59);
            }

            PagedList<BOArticle> articles = null;
            string LIST_CACHE_ID = "LA_" + LanguageId + state.GetCacheIdentifier() + regularIds + (requestedMonth.HasValue ? requestedMonth.Value.ToString() : "") + ":" + titleSearch; 
            // Only cache 1st page of online articles, don't cache on admin and don't cache searches
            bool useCache = !showUntranslated && publishFlag && state.FirstRecordIndex < state.RecordsPerPage && string.IsNullOrEmpty(titleSearch);
            
            if (useCache)
                articles = OCache.Get(LIST_CACHE_ID) as PagedList<BOArticle>;

            if (articles == null)
            {
                if (string.IsNullOrEmpty(titleSearch))
                    articles = articleDB.ListArticles(publishFlag, from, to, regularIds, state, LanguageId, showUntranslated);
                else
                {
                    var tempRegularList = StringTool.SplitStringToIntegers(regularIds);
                    articles = articleDB.ListFilteredArticles(publishFlag, from, to, state, LanguageId, showUntranslated, titleSearch, tempRegularList);
                }

                foreach (BOArticle article in articles)
                {
                    if ( showUntranslated && article.ContentId.HasValue && article.MissingTranslation  )
                        article.Title = BInternalContent.GetContentTitleInAnyLanguage(article.ContentId.Value);

                    article.Regulars = articleDB.ListRegulars(new ListingState(SortDir.Ascending, "title"), showUntranslated, article.Id, article.PublishFlag, LanguageId);
                }

                if (useCache)
                {
                    lock (cacheLockingArticleList)
                    {
                        PagedList<BOArticle> tempArticles = OCache.Get(LIST_CACHE_ID) as PagedList<BOArticle>;
                        if (null == tempArticles)
                            OCache.Max(LIST_CACHE_ID, articles);
                    }
                }
            }
            return articles;
        }

        public List<BOArticleMonth> ListArticleMonths(string regularIds, bool showArticleCount)
        {
            List<BOArticleMonth> dates = null;
            string LIST_CACHE_ID = "LAD_" + LanguageId + regularIds + publishFlag + showArticleCount;

            if (publishFlag)
                dates = OCache.Get(LIST_CACHE_ID) as List<BOArticleMonth>;

            if (dates == null)
            {
                dates = articleDB.ListArticleMonths(publishFlag, StringTool.SplitStringToIntegers(regularIds), showArticleCount, LanguageId);
                
                if (publishFlag)
                {
                    lock (cacheLockingArticleDateListGet)
                    {
                        List<BOArticleMonth> tempDates = OCache.Get(LIST_CACHE_ID) as List<BOArticleMonth>;
                        if (tempDates == null)
                            OCache.Max(LIST_CACHE_ID, dates);
                    }
                }
            }

            return dates;
        }

        #endregion Article methods

        #region Regular methods

        public bool DeleteRegular(int id)
        {
            BORegular regular = articleDB.GetRegular(id, true);
            if (regular != null && regular.ArticleCount == 0)
            {
                articleDB.DeleteRegular(id);
                ClearLanguageVariations(REGULAR_CACHE_ID(id));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes underlying BOInternalContent object and changes BORegular object.
        /// Removes object from Cache then adds new object to Cache.
        /// </summary>
        public void ChangeRegular(BORegular regular)
        {
            if (regular == null)
                throw new ArgumentException("regular is null");

            // retrieve existing content id from db in case its not provided
            if (regular.Id.HasValue)
            {
                BORegular existingRegular = GetUnCachedRegular(regular.Id.Value);
                if (existingRegular != null && existingRegular.Id.HasValue && existingRegular.ContentId.HasValue)
                {
                    regular.ContentId = existingRegular.ContentId;
                }
            }

            contentB.Change(regular);
            articleDB.ChangeRegular(regular);
            ClearLanguageVariations(REGULAR_CACHE_ID(regular.Id.Value));
        }

        /// <summary>
        /// Retrieves BORegular object based on id and publishFlag.
        /// An attempt is made to return a fully cached object. 
        /// </summary>
        /// <param name="regularId"></param>
        /// <returns></returns>
        public BORegular GetRegular(int regularId)
        {
            BORegular regular = OCache.Get(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(regularId)) as BORegular;
            if (regular == null)
            {
                regular = GetUnCachedRegular(regularId);

                if (regular != null && !regular.MissingTranslation)
                {
                    OCache.Max(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(regularId), regular);
                }
            }
            return regular;
        }

        /// <summary>
        /// Retrieves BORegular object based on id and publishFlag.
        /// Loads object Content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private BORegular GetUnCachedRegular(int id)
        {
            BORegular regular = articleDB.GetRegular(id, !publishFlag);

            if (regular != null && regular.MissingTranslation && regular.ContentId.HasValue)
                regular.Title = BInternalContent.GetContentTitleInAnyLanguage(regular.ContentId.Value);

            return regular;
        }

        /// <summary>
        /// Retrieves paged list of regular based on ListingState and publishFlag.
        /// Uses delegate SingleRegularGet to retrieve individual objects. 
        /// Caching of individual objects is based on publish flag.
        /// </summary>
        public List<BORegular> ListRegulars(ListingState state, bool showUntranslated, int? articleId, bool? publish)
        {
            List<BORegular> regulars = articleDB.ListRegulars(state, showUntranslated, articleId, publish, LanguageId);

            if (showUntranslated)
                for (int i = 0; i < regulars.Count; i++)
                    if (regulars[i] != null && regulars[i].MissingTranslation && regulars[i].ContentId.HasValue)
                        regulars[i].Title = BInternalContent.GetContentTitleInAnyLanguage(regulars[i].ContentId.Value);

            return regulars;
        }

        #endregion General Regular methods

        #region IPublisher Members

        private void ClearCache(int artId)
        {
            ClearLanguageVariations(ARTICLE_CACHE_ID(artId, true));
            ClearLanguageVariations(ARTICLE_CACHE_ID(artId, false));
        }

        public bool Publish(int id)
        {
            bool success = false;
            ClearCache(id);

            // publish articles in all languages
            List<int> languages = ListLanguages();

            foreach (int i in languages)
            {
                BOArticle articleOffline = GetPublishableArticle(id, false, i);
                BOArticle articleOnline = GetPublishableArticle(id, true, i);

                if (articleOffline != null)
                {
                    articleOffline.Regulars = articleDB.ListRegulars(new ListingState(SortDir.Ascending, "title"), true, id, false, i);

                    if (articleOffline.MarkedForDeletion)
                    {
                        if (articleOffline.Id.HasValue)
                        {
                            articleDB.DeleteArticle(articleOffline.Id.Value, false);
                            contentB.Delete(articleOffline.ContentId.Value);
                            success = true;
                        }

                        if (articleOnline != null && articleOnline.Id.HasValue)
                        {
                            articleDB.DeleteArticle(articleOnline.Id.Value, true);
                            contentB.Delete(articleOnline.ContentId.Value);
                            success = true;
                        }
                    }
                    else
                    {
                        articleOffline.IsChanged = false;
                        int? onlineContentID = null;

                        if (articleOnline != null && articleOnline.Id > 0)
                        {
                            onlineContentID = articleOnline.ContentId.Value;
                        }

                        articleOnline = (BOArticle)articleOffline.Clone();
                        articleOnline.PublishFlag = true;
                        articleOnline.ContentId = onlineContentID;

                        ChangeArticle(articleOffline);
                        ChangeArticle(articleOnline);

                        contentB.CloneContent(articleOffline.ContentId.Value, articleOnline.ContentId.Value);
                        success = true;
                    }
                }
            }
            return success;
        }

        public bool UnPublish(int id)
        {
            ClearCache(id);
            BOArticle articleOffline = GetUnCachedArticle(id, false, false);
            BOArticle articleOnline = GetUnCachedArticle(id, true, false);

            if (articleOffline != null && articleOnline != null)
            {
                articleDB.DeleteArticle(id, true);
                contentB.Delete(articleOnline.ContentId.Value);
                articleOffline.IsChanged = true;
                ChangeArticle(articleOffline);
                return true;
            }
            else
                return false;
        }

        public bool MarkForDeletion(int id)
        {
            ClearCache(id);
            BOArticle articleOffline = GetUnCachedArticle(id, false, false);
            if (articleOffline != null && articleOffline.MarkedForDeletion == false)
            {
                articleOffline.MarkedForDeletion = true;
                articleDB.ChangeArticle(articleOffline); // change the article
                return true;
            }
            else
                return false;
        }

        public bool RevertToPublished(int id)
        {
            ClearCache(id);
            BOArticle articleOffline = GetUnCachedArticle(id, false, false);
            BOArticle articleOnline = GetUnCachedArticle(id, true, false);

            if (articleOffline != null && articleOnline != null)
            {
                int offlineContentID = articleOffline.ContentId.Value;
                articleOffline = (BOArticle)articleOnline.Clone();
                articleOffline.PublishFlag = false;
                articleOffline.ContentId = offlineContentID;

                this.ChangeArticle(articleOffline);
                return true;
            }
            else if (articleOffline != null && articleOffline.MarkedForDeletion)
            {
                articleOffline.MarkedForDeletion = false;
                articleDB.ChangeArticle(articleOffline);
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Cache Ids

        private static string REGULAR_CACHE_ID(int regularId)
        {
            return "_Regular_" + regularId;
        }

        private static string ARTICLE_CACHE_ID(int articleId, bool publishFlag)
        {
            return (publishFlag ? "_Article_" : "_PreviewArticle_") + articleId;
        }

        #endregion Cache Ids
    }
}
