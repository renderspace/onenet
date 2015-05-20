using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using StackExchange.Redis;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.IO;

namespace One.Net.BLL.Caching
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static object lockMe = new object();
        IDatabase _database;
        ConnectionMultiplexer _connection;

        public RedisCacheProvider()
        {
            var redisAccessKey = ConfigurationManager.AppSettings["Cache.RedisAccessKey"];
            var connectionString = ConfigurationManager.AppSettings["Cache.ConnectionString"];//"contoso5.redis.cache.windows.net,password=...");
            _connection = ConnectionMultiplexer.Connect(string.Format("{0},ssl=true,password={1}", connectionString, redisAccessKey));
            _database = _connection.GetDatabase();
        }

        public T Get<T>(string key) where T : class
        {
            var data = _database.StringGet(key);
            if (!data.IsNull && data.HasValue)
            {
                var blobBytes = (byte[])data;
                var deserialisedObject = Deserialize<T>(blobBytes);
                return deserialisedObject;
            }

            return default(T);
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
                        if (obj is IList && ((IList)obj).Count > 0)
                        {
                            this.Put(key, obj, slidingExpiryWindow);
                        }
                        else if (obj != default(T) && obj != null)
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
            var success = _database.StringSet(key, Serialize(data), slidingExpiryWindow);
        }

        public void Put(string key, byte[] data, TimeSpan? slidingExpiryWindow = null)
        {
            if (slidingExpiryWindow == null)
            {
                slidingExpiryWindow = TimeSpan.FromMinutes(12);
            }
            var success = _database.StringSet(key, data, slidingExpiryWindow);
        }

        public bool IsSet(string key)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _database.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public void RemoveAll()
        {
            var allEndpoints = _connection.GetEndPoints();
            if (allEndpoints != null && allEndpoints.Length > 0)
            {
                foreach (var endpoint in allEndpoints)
                {
                    var server = _connection.GetServer(endpoint);
                    try
                    {
                        server.FlushAllDatabases();
                        var allKeys = server.Keys();
                        _database.KeyDelete(allKeys.ToArray(), CommandFlags.FireAndForget);
                    }
                    catch (Exception ex)
                    {
   
                    }
                }
            }
        }

        public void RemoveWithPartialKey(string partialKey)
        {
            throw new NotImplementedException();
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
    }
}
