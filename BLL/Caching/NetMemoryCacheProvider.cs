using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace One.Net.BLL.Caching
{
    public class NetMemoryCacheProvider : ICacheProvider
    {
        private static object lockMe = new object();
        private static Cache _cache;
        private static HttpRuntime _httpRuntime;

        public NetMemoryCacheProvider()
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
                    Monitor.Enter(typeof(NetMemoryCacheProvider));
                    if (null == _httpRuntime)
                    {
                        // Create an Http Content to give us access to the cache.
                        _httpRuntime = new HttpRuntime();
                    }
                }
                finally
                {
                    Monitor.Exit(typeof(NetMemoryCacheProvider));
                }
            }
        }

        public T Get<T>(string key) where T : class
        {
            return _cache[key] as T;
        }

        public T Get<T>(string key, Func<T> fn, TimeSpan? slidingExpiryWindow = null) where T : class
        {
            var obj = this.Get<T>(key);
            if (obj == default(T) || obj == null)
            {
                lock (lockMe)
                {
                    if (obj == default(T) || obj == null)
                    {
                        obj = fn();
                        if (obj != default(T) && obj != null)
                        {
                            this.Put(key, obj, slidingExpiryWindow);
                        }
                    }
                }
            }
            return obj;
        }

        public void Put(string key, object data, TimeSpan? slidingExpiryWindow = null)
        {
            if (slidingExpiryWindow == null)
            {
                slidingExpiryWindow = TimeSpan.FromMinutes(12);
            }
            if (data != null)
            {
                _cache.Insert(key, data, null, DateTime.MaxValue, slidingExpiryWindow.Value, CacheItemPriority.Default, null);
            }
        }

        public void Put(string key, byte[] data, TimeSpan? slidingExpiryWindow = null)
        {
            if (slidingExpiryWindow == null)
            {
                slidingExpiryWindow = TimeSpan.FromMinutes(12);
            }
            if (data != null)
            {
                _cache.Insert(key, data, null, DateTime.MaxValue, slidingExpiryWindow.Value, CacheItemPriority.Default, null);
            }
        }

        public bool IsSet(string key)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveAll()
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

        public void RemoveWithPartialKey(string partialKey)
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
    }
}
