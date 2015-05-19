using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Net.BLL.Caching
{
    public static class CacheFactory
    {
        public static ICacheProvider ResolveCacheFromConfig()
        {
            var cacheType = ConfigurationManager.AppSettings["Cache.CacheToUse"]; 
            switch(cacheType.ToLower())
            {
                case "memcached":
                    throw new NotImplementedException("memcached");
                case "redis":
                    return new RedisCacheProvider();
                case "netmemory":
                default:
                    return new NetMemoryCacheProvider();

            }
        }
    }
}
