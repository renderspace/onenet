using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using NLog;
using One.Net.BLL.DAL;
using One.Net.BLL.Caching;

namespace One.Net.BLL
{
    [Serializable]
    public abstract class BusinessBaseClass
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected static readonly DbContent contentDb = new DbContent();
        protected static ICacheProvider cache = CacheFactory.ResolveCacheFromConfig();

        protected virtual int LanguageId { get { return Thread.CurrentThread.CurrentCulture.LCID; } }

        private bool publishFlag = false;

        public bool PublishFlag
        {
            get { return publishFlag; }
        }

        public BusinessBaseClass() 
        {
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);        
        }

        public void ClearCache()
        {
            cache.RemoveAll();
        }

        public List<int> ListLanguages()
        {
            List<int> list = cache.Get<List<int>>("Languages");
            if (list == null)
            {
                list = contentDb.ListLanguages();
                cache.Put("Languages", list);
            }
            return list;
        }

        #region Caching

        private const string LNG_PREFIX = "LNG";

        protected void ClearLanguageVariations(string cacheIdentification)
        {
            List<int> languages = ListLanguages();
            foreach (int i in languages)
            {
                cache.Remove(LNG_PREFIX + i + cacheIdentification);
            }
        }

        protected string CACHE_LANG_PREFIX
        {
            get { return PublishFlag + "_" + LNG_PREFIX + LanguageId; }
        }

        #endregion Caching
    }
}
