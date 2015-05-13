using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using NLog;
using One.Net.BLL.DAL;

namespace One.Net.BLL
{
    [Serializable]
    public abstract class BusinessBaseClass
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected static readonly DbContent contentDb = new DbContent();

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

        public List<int> ListLanguages()
        {
            List<int> list = OCache.Get("Languages") as List<int>;
            if (list == null)
            {
                list = contentDb.ListLanguages();
                OCache.Max("Languages", list);
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
                OCache.Remove(LNG_PREFIX + i + cacheIdentification);
            }
        }

        protected string CACHE_LANG_PREFIX
        {
            get { return PublishFlag + "_" + LNG_PREFIX + LanguageId; }
        }

        #endregion Caching
    }
}
