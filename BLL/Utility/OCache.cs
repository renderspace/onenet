using System;
using System.Collections;
using System.Threading;
using System.Web;
using System.Web.Caching;
using log4net;
using System.Collections.Generic;

namespace One.Net.BLL
{
    public class OCache
    {
        public const int Day = 17280;
		public const int Hour = 720;
		public const int Minute = 12;
		public const double Second = 0.2;

        private static readonly Cache _cache;
        private static HttpRuntime _httpRuntime;
        protected static readonly ILog log = LogManager.GetLogger(typeof(OCache));
        
        /// <summary>
        /// Static initializer should ensure we only have to look up the current cache
        /// instance once.
        /// </summary>
        static OCache()
        {
            if (null == HttpContext.Current)
            {
                EnsureHttpRuntime();
                _cache = HttpRuntime.Cache;
            }
            else
            {
                _cache = HttpContext.Current.Cache;
            }
        }

        private static void EnsureHttpRuntime()
        {
            if (null == _httpRuntime)
            {
                try
                {
                    Monitor.Enter(typeof(OCache));
                    if (null == _httpRuntime)
                    {
                        // Create an Http Content to give us access to the cache.
                        _httpRuntime = new HttpRuntime();
                    }
                }
                finally
                {
                    Monitor.Exit(typeof(OCache));
                }
            }
        }

        /// <summary>
        /// Removes all items from the Cache
        /// </summary>
        public static void Clear()
        {
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            ArrayList al = new ArrayList();
            while (CacheEnum.MoveNext())
            {
                al.Add(CacheEnum.Key);
            }

            foreach (string key in al)
            {
                _cache.Remove(key);
            }

        }

        /// <summary>
        /// Insert the current "obj" into the cache. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Insert(string key, object obj)
        {
            Insert(key, obj, null, 1);
        }

        public static void Insert(string key, object obj, int seconds)
        {
            Insert(key, obj, null, seconds, CacheItemPriority.Normal);
        }

        public static void Insert(string key, object obj, CacheDependency dep)
        {
            Insert(key, obj, dep, Hour * 12);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
        {
            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
        {
            if (obj != null)
            {
				_cache.Insert(key, obj, dep, DateTime.Now.AddSeconds(/*Factor * */seconds), Cache.NoSlidingExpiration, /*TimeSpan.Zero, */priority, null);
            }
        }

        public static void Add(string key, object obj, CacheDependency dep, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            _cache.Add(key, obj, dep, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        /// <summary>
        /// Insert an item into the cache for the Maximum allowed time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Max(string key, object obj)
        {
            Max(key, obj, null);
        }

        public static void Max(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, DateTime.MaxValue, new TimeSpan(0, 12, 0), CacheItemPriority.Normal, OnCacheChanged);
            }
        }

        public static void Max(string key, object obj, CacheItemPriority priority)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, null, DateTime.MaxValue, new TimeSpan(0, 12, 0), priority, null);
            }
        }

        static void OnCacheChanged(string key, object item, CacheItemRemovedReason reason)
        {
            if (log.IsDebugEnabled)
                log.Debug("OnCacheChanged key:" + key + " Reason:" + reason.ToString());

            Remove(key);
        }

		public static void Remove(string key)
		{
			_cache.Remove(key);
		}

        public static void RemoveWithPartialKey(string partialKey)
        {
            var keyList = new List<string>();
            foreach (DictionaryEntry entry in _cache)
            {
                var key = entry.Key as string;
                if (key != null && key.StartsWith(partialKey))
                    keyList.Add(key);
            }
            foreach (string key in keyList)
                _cache.Remove(key);
        }

        public static object Get(string key)
        {
            return _cache[key];
        }
    }
}