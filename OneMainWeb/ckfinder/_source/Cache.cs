using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Caching;

namespace CKFinder
{
	public class Cache
	{
		private static object syncObject = new object();
	
		private static bool IsCacheable
		{
			get
			{
				return HttpContext.Current != null;
			}
		}
	
		protected static int CacheDuration
		{
			get
			{
				if (IsCacheable)
                {
                    return 3600;
                }
				return 0;
			}
		}

		public static void Add(string key, object value)
		{
			if ((CacheDuration > 0) && (value != null))
			{
				HttpContext.Current.Cache.Insert(key, value, null, DateTime.Now.AddSeconds(CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
			}
		}

		public static object Get(string key)
		{
			if (CacheDuration > 0)
			{   
				return HttpContext.Current.Cache[key];
			}
			return null;
		}

		public static void Clear()
		{
			if (CacheDuration > 0)
			{
				lock (syncObject)
				{
					var keyList = new List<string>();
					foreach (DictionaryEntry entry in HttpContext.Current.Cache)
						keyList.Add(entry.Key as string);

					foreach (var key in keyList)
						HttpContext.Current.Cache.Remove(key);
				}
			}
		}

        public static void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

	    public static void Remove<T>(string partialKey) where T : new()
		{
			if (CacheDuration > 0)
			{
				var keyList = new List<string>();
				foreach (DictionaryEntry entry in HttpContext.Current.Cache)
				{
					string key = entry.Key as string;
					if (key != null && key.Contains(partialKey))
						keyList.Add(key);
				}
				foreach (string key in keyList)
					HttpContext.Current.Cache.Remove(key);
			}
		}

		public static void Remove<T>() where T : new()
		{
			if (CacheDuration > 0)
			{
				var keyList = new List<string>();
				foreach (DictionaryEntry entry in HttpContext.Current.Cache)
				{
					keyList.Add(entry.Key as string);
				}
				foreach (string key in keyList)
					HttpContext.Current.Cache.Remove(key);
			}
		}

		public static void RemoveWithPartialKey(string partialKey)
		{
			if (CacheDuration > 0)
			{
				var keyList = new List<string>();
				foreach (DictionaryEntry entry in HttpContext.Current.Cache)
				{
					var key = entry.Key as string;
					if (key != null && key.StartsWith(partialKey))
						keyList.Add(key);
				}
				foreach (string key in keyList)
					HttpContext.Current.Cache.Remove(key);
			}
		}
	}
}