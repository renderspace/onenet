using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Net.BLL.Caching
{
    public interface ICacheProvider
    {
        T Get<T>(string key) where T : class;
        T Get<T>(string key, Func<T> fn, TimeSpan? slidingExpiryWindow = null) where T : class;
        // object Get(string key);


        void Put(string key, object data, TimeSpan? slidingExpiryWindow = null);
        void Put(string key, byte[] data, TimeSpan? slidingExpiryWindow = null);
        bool IsSet(string key);
        void Remove(string key);
        void RemoveAll();
        void RemoveWithPartialKey(string partialKey);
    }
}
