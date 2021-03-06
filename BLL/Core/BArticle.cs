using System;
using System.Collections.Generic;
using System.Transactions;
using One.Net.BLL.DAL;
using System.Linq;
using System.Threading;
using System.Globalization;

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
        /// Upgrades article and regular tables to add human_readable_url field.
        /// Clears cache.
        /// </summary>
        public void UpgradeArticles()
        {
            articleDB.UpgradeArticles();
            ClearCache();
        }

        public string GenerateHumanReadableArticleUrl(string title)
        {
            return GenerateHumanReadableArticleUrl(title, 0);
        }

        private string GenerateHumanReadableArticleUrl(string title, int postFix = 0)
        {
            var humanReadableUrlPart = "";

            var urlPart = BWebsite.PrepareParLink(title);
            if (postFix > 0)
                urlPart += "-" + postFix;

            var existingArticle = GetUnCachedArticle(urlPart, false);
            if (existingArticle == null)
                humanReadableUrlPart = urlPart;
            else
                humanReadableUrlPart = GenerateHumanReadableArticleUrl(title, (postFix + 1));

            return humanReadableUrlPart;
        }

        public string GenerateHumanReadableRegularUrl(string title)
        {
            return GenerateHumanReadableRegularUrl(title, 0);
        }

        private string GenerateHumanReadableRegularUrl(string title, int postFix = 0)
        {
            var humanReadableUrlPart = "";

            var urlPart = BWebsite.PrepareParLink(title);
            if (postFix > 0)
                urlPart += "-" + postFix;

            var existingRegular = GetUnCachedRegular(urlPart);
            if (existingRegular == null)
                humanReadableUrlPart = urlPart;
            else
                humanReadableUrlPart = GenerateHumanReadableRegularUrl(title, (postFix + 1));

            return humanReadableUrlPart;
        }

        public int AutoCreateHumanReadableUrlArticles(bool autoGeneratePartialLink = true)
        {
            var regulars = new List<int>();
            var excludeRegulars = new List<int>();
            var state = new ListingState();
            state.RecordsPerPage = 10000;

            state.SortDirection = SortDir.Descending;
            state.SortField = "Id";

            var articles = articleDB.ListArticles(PublishFlag, null, null, regulars, state, LanguageId, excludeRegulars);
            var count = 0;
            foreach (var a in articles.Where(ar => string.IsNullOrWhiteSpace(ar.HumanReadableUrl)))
            {
                var humanReadableUrlPart = autoGeneratePartialLink ? BWebsite.PrepareParLink(a.Title) : a.Id.ToString();
                var article = GetUnCachedArticle(a.Id.Value, a.PublishFlag);
                try
                {
                    if (string.IsNullOrWhiteSpace(humanReadableUrlPart))
                        throw new Exception("IsNullOrWhiteSpace(humanReadableUrlPart");
                    article.HumanReadableUrl = humanReadableUrlPart;
                    ChangeArticle(article);
                }
                catch (Exception ex)
                {
                    log.Info(ex, "duplicate human readable url");
                    article.HumanReadableUrl = humanReadableUrlPart + "-" + article.Id.ToString();
                    ChangeArticle(article);
                } 
                count++;
            }
            ClearCache();
            return count;
        }

        public int AutoCreateHumanReadableUrlRegulars(bool autoGeneratePartialLink = true)
        {
            var regulars = new List<int>();
            var state = new ListingState();
            state.RecordsPerPage = 10000;

            var reg = articleDB.ListRegulars(state, false, false, LanguageId);
            var count = 0;
            foreach (var r in reg.Where(ar => string.IsNullOrWhiteSpace(ar.HumanReadableUrl)))
            {
                var humanReadableUrlPart = autoGeneratePartialLink ? BWebsite.PrepareParLink(r.Title) : r.Id.ToString();

                try
                {
                    if (string.IsNullOrWhiteSpace(humanReadableUrlPart))
                        throw new Exception("IsNullOrWhiteSpace(humanReadableUrlPart");
                    r.HumanReadableUrl = humanReadableUrlPart;
                    ChangeRegular(r);
                }
                catch (Exception ex)
                {
                    log.Info(ex, "duplicate human readable url");
                    r.HumanReadableUrl = humanReadableUrlPart + "-" + r.Id.ToString();
                    ChangeRegular(r);
                }
                count++;
            }
            ClearCache();
            return count;
        }

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
                BOArticle existingArticle = GetUnCachedArticle(article.Id.Value, article.PublishFlag);
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
        /// <returns></returns>
        public BOArticle GetArticle(int id)
        {
            return GetArticle(id, PublishFlag);
        }
        
        /// <summary>
        /// Retrieves BOArticle object based on human readable url.
        /// </summary>
        /// <param name="humanReadableUrl"></param>
        /// <returns></returns>
        public BOArticle GetArticle(string humanReadableUrl, int overrideLanguageId = 0)
        {

            var originalLanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(overrideLanguageId);
            }

            BOArticle article = null;
            bool useCache = PublishFlag;
            string cacheKey = CACHE_LANG_PREFIX + ARTICLE_CACHE_ID(humanReadableUrl, PublishFlag);
            if (useCache)
                article = cache.Get<BOArticle>(cacheKey);

            if (article == null)
            {
                article = GetUnCachedArticle(humanReadableUrl, PublishFlag);

                if (article != null && !article.MissingTranslation && useCache)
                {
                    lock (cacheLockingArticleGet)
                    {
                        BOArticle tempArticle = cache.Get<BOArticle>(cacheKey);
                        if (null == tempArticle)
                            cache.Put(cacheKey, article);
                    }
                }
            }
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(originalLanguageId);
            }
            return article;
        }

        /// <summary>
        /// Retrieves BOArticle object based on id and publishFlag.
        /// If publishFlag is true, an attempt is made to return a fully cached object. If object is not cached, an uncached object is returned.
        /// If publishFlag is false, an uncached object is returned.
        /// </summary>        /// <param name="articleId"></param>
        /// <param name="publish"></param>
        /// <returns></returns>
        private BOArticle GetArticle(int articleId, bool publish)
        {
            BOArticle article = null;
            bool useCache = true;
            string cacheKey = CACHE_LANG_PREFIX + ARTICLE_CACHE_ID(articleId, publish);
            if (useCache)
                article = cache.Get<BOArticle>(cacheKey);

            if (article == null)
            {
                article = GetUnCachedArticle(articleId, publish);

                if (article != null && !article.MissingTranslation && useCache)
                {
                    lock (cacheLockingArticleGet)
                    {
                        BOArticle tempArticle = cache.Get<BOArticle>(cacheKey);
                        if (null == tempArticle)
                            cache.Put(cacheKey, article); 
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
        /// <returns></returns>
        internal BOArticle GetUnCachedArticle(int id, bool publish)
        {
            BOArticle article = articleDB.GetArticle(id, publish, LanguageId);

            if (article != null && article.Id.HasValue)
            {
                article.Regulars = articleDB.ListArticleRegulars(article.Id.Value, article.PublishFlag, LanguageId);
            }

            return article;
        }

        private BOArticle GetUnCachedArticle(string humanReadableUrl, bool publish)
        {
            BOArticle article = articleDB.GetArticle(humanReadableUrl, publish, LanguageId);

            if (article != null && article.Id.HasValue)
            {
                article.Regulars = articleDB.ListArticleRegulars(article.Id.Value, article.PublishFlag, LanguageId);
            }

            return article;
        }

        public PagedList<BOArticle> ListArticles(List<int> regularIds, DateTime? from, DateTime? to, ListingState state, string titleSearch, List<int> excludeRegularIds)
        {
            log.Debug("ListArticles:" + regularIds.ToArray().ToString());

            PagedList<BOArticle> articles = null;

            if (string.IsNullOrEmpty(titleSearch))
            {
                articles = articleDB.ListArticles(PublishFlag, from, to, regularIds, state, LanguageId, excludeRegularIds);
            }
            else
            {
                articles = articleDB.ListFilteredArticles(PublishFlag, from, to, state, LanguageId, titleSearch, regularIds, excludeRegularIds);
            }

            foreach (BOArticle article in articles)
            {
                article.Regulars = articleDB.ListArticleRegulars(article.Id.Value, article.PublishFlag, LanguageId);
            }
            return articles;
        }

        /// <summary>
        /// Retrieves paged list of articles based on ListingState and publishFlag.
        /// Uses delegate SingleArticleGet to retrieve individual objects. 
        /// Caching of individual objects is based on publish flag.
        /// </summary>
        public PagedList<BOArticle> ListArticles(List<int> regularIds, ListingState state, string titleSearch, DateTime? requestedMonth, DateTime? requestedYear, List<int> excludeRegularIds, int overrideLanguageId = 0)
        {
            var originalLanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(overrideLanguageId);
            }

            DateTime? from = null;
            DateTime? to = null;

            int preservedRecordsPerPage = state.RecordsPerPage.HasValue ? state.RecordsPerPage.Value : 10;

            if (state.SortField == "random")
            {
                state.RecordsPerPage = 30;
            }

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
            string LIST_CACHE_ID = "LA_" + LanguageId + state.GetCacheIdentifier() + string.Join<int>(",", regularIds) + (requestedMonth.HasValue ? requestedMonth.Value.ToString() : "") + ":" + titleSearch + ":" + string.Join<int>(",", excludeRegularIds);
            LIST_CACHE_ID += requestedYear.HasValue ? requestedYear.Value.ToString() : "";
            // Only cache 1st page of online articles, don't cache on admin and don't cache searches
            bool useCache = PublishFlag && state.FirstRecordIndex < state.RecordsPerPage && string.IsNullOrEmpty(titleSearch);

            if (useCache)
            {
                articles = cache.Get<PagedList<BOArticle>>(LIST_CACHE_ID);
            }

            if (articles == null)
            {
                if (string.IsNullOrEmpty(titleSearch))
                {
                    articles = articleDB.ListArticles(PublishFlag, from, to, regularIds, state, LanguageId, excludeRegularIds);
                }
                else
                {
                    articles = articleDB.ListFilteredArticles(PublishFlag, from, to, state, LanguageId, titleSearch, regularIds, excludeRegularIds);
                }

                foreach (BOArticle article in articles)
                {
                    article.Regulars = articleDB.ListArticleRegulars(article.Id.Value, article.PublishFlag, LanguageId);
                }

                if (useCache)
                {
                    lock (cacheLockingArticleList)
                    {
                        PagedList<BOArticle> tempArticles = cache.Get<PagedList<BOArticle>>(LIST_CACHE_ID);
                        if (null == tempArticles)
                        {
                            if (state.SortField == "random")
                                cache.Put(LIST_CACHE_ID, articles, TimeSpan.FromMinutes(10));
                            else
                                cache.Put(LIST_CACHE_ID, articles);
                        }
                    }
                }
            }

            if (articles != null && state != null && state.SortField == "random")
            {
                var fetchedArticles = articles;
                var temp = fetchedArticles.OrderBy(a => Guid.NewGuid()).Take(preservedRecordsPerPage).ToList();
                articles = new PagedList<BOArticle>(temp);
                articles.AllRecords = fetchedArticles.AllRecords;
            }
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(originalLanguageId);
            }
            return articles;
        }

        public DateTime? GetFirstDateWithArticles(string regularIds, int fromYear, int fromMonth, bool? excludePast)
        {
            return articleDB.GetFirstDateWithArticles(PublishFlag, StringTool.SplitStringToIntegers(regularIds), fromYear, fromMonth, LanguageId, excludePast);
        }

        public List<BOArticleMonthDay> ListArticleMonthDays(string regularIds, bool showArticleCount, int year, int month)
        {
            List<BOArticleMonthDay> dates = null;
            string LIST_CACHE_ID = "LAMD_" + LanguageId + regularIds + PublishFlag + showArticleCount + year + month;

            if (PublishFlag)
                dates = cache.Get<List<BOArticleMonthDay>>(LIST_CACHE_ID);

            if (dates == null)
            {
                dates = articleDB.ListArticleMonthDays(PublishFlag, StringTool.SplitStringToIntegers(regularIds), showArticleCount, year, month, LanguageId);

                if (PublishFlag)
                {
                    lock (cacheLockingArticleDateListGet)
                    {
                        List<BOArticleMonthDay> tempDates = cache.Get<List<BOArticleMonthDay>>(LIST_CACHE_ID);
                        if (tempDates == null)
                            cache.Put(LIST_CACHE_ID, dates);
                    }
                }
            }

            return dates;
        }

        public List<BOArticleMonth> ListArticleMonths(string regularIds, bool showArticleCount, int overrideLanguageId = 0)
        {
            var originalLanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(overrideLanguageId);
            }
            List<BOArticleMonth> dates = null;
            string LIST_CACHE_ID = "LAD_" + LanguageId + regularIds + PublishFlag + showArticleCount;

            if (PublishFlag)
                dates = cache.Get<List<BOArticleMonth>>(LIST_CACHE_ID);

            if (dates == null)
            {
                dates = articleDB.ListArticleMonths(PublishFlag, StringTool.SplitStringToIntegers(regularIds), showArticleCount, LanguageId);

                if (PublishFlag)
                {
                    lock (cacheLockingArticleDateListGet)
                    {
                        List<BOArticleMonth> tempDates = cache.Get<List<BOArticleMonth>>(LIST_CACHE_ID);
                        if (tempDates == null)
                            cache.Put(LIST_CACHE_ID, dates);
                    }
                }
            }
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(originalLanguageId);
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
            BORegular regular = cache.Get<BORegular>(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(regularId));
            if (regular == null)
            {
                regular = GetUnCachedRegular(regularId);

                if (regular != null && !regular.MissingTranslation)
                {
                    cache.Put(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(regularId), regular);
                }
            }
            return regular;
        }

        /// <summary>
        /// Retrieves BORegular object based on humanReadableParameter.
        /// An attempt is made to return a fully cached object. 
        /// </summary>
        /// <param name="humanReadableParameter"></param>
        /// <returns></returns>
        public BORegular GetRegular(string humanReadableParameter, int overrideLanguageId = 0)
        {
            var originalLanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(overrideLanguageId);
            }
            BORegular regular = cache.Get<BORegular>(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(humanReadableParameter));
            if (regular == null)
            {
                regular = GetUnCachedRegular(humanReadableParameter);

                if (regular != null && !regular.MissingTranslation)
                {
                    cache.Put(CACHE_LANG_PREFIX + REGULAR_CACHE_ID(humanReadableParameter), regular);
                }
            }
            if (overrideLanguageId > 0)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(originalLanguageId);
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
            BORegular regular = articleDB.GetRegular(id, !PublishFlag);

            if (regular != null && regular.MissingTranslation && regular.ContentId.HasValue)
                regular.Title = BInternalContent.GetContentTitleInAnyLanguage(regular.ContentId.Value);

            return regular;
        }

        /// <summary>
        /// Retrieves BORegular object based on id and publishFlag.
        /// Loads object Content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private BORegular GetUnCachedRegular(string humanReadableParameter)
        {
            var regular = articleDB.GetRegular(humanReadableParameter, !PublishFlag);

            if (regular != null && regular.MissingTranslation && regular.ContentId.HasValue)
                regular.Title = BInternalContent.GetContentTitleInAnyLanguage(regular.ContentId.Value);

            return regular;
        }

        /// <summary>
        /// Retrieves paged list of regular based on ListingState and publishFlag.
        /// Uses delegate SingleRegularGet to retrieve individual objects. 
        /// Caching of individual objects is based on publish flag.
        /// </summary>
        public IEnumerable<BORegular> ListRegulars(ListingState state)
        {
            var regulars = articleDB.ListRegulars(state, PublishFlag, false, LanguageId);
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

            foreach (int lang in languages)
            {
                BOArticle articleOffline = articleDB.GetArticle(id, false, lang);
                BOArticle articleOnline = articleDB.GetArticle(id, true, lang);

                if (articleOffline != null)
                {
                    articleOffline.Regulars = articleDB.ListArticleRegulars(id, false, lang);

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
            BOArticle articleOffline = GetUnCachedArticle(id, false);
            BOArticle articleOnline = GetUnCachedArticle(id, true);

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
            BOArticle articleOffline = GetUnCachedArticle(id, false);
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
            BOArticle articleOffline = GetUnCachedArticle(id, false);
            BOArticle articleOnline = GetUnCachedArticle(id, true);

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

        private static string REGULAR_CACHE_ID(string humanReadableParameter)
        {
            return "_Regular_" + humanReadableParameter;
        }

        private static string ARTICLE_CACHE_ID(int articleId, bool publishFlag)
        {
            return (publishFlag ? "_Article_" : "_PreviewArticle_") + articleId;
        }

        private static string ARTICLE_CACHE_ID(string humanReadableUrl, bool publishFlag)
        {
            return (publishFlag ? "_Article_" : "_PreviewArticle_") + humanReadableUrl;
        }

        #endregion Cache Ids
    }
}
