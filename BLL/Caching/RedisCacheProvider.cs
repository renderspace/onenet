using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Configuration;
using System.Runtime.Serialization;

namespace One.Net.BLL.Caching
{
    public class RedisCacheProvider : ICacheProvider
    {
        IDatabase _database;
        ConnectionMultiplexer _connection;

        public RedisCacheProvider()
        {
            var connectionString = ConfigurationManager.AppSettings["Cache.ConnectionString"];
            _connection = ConnectionMultiplexer.Connect(connectionString); //"contoso5.redis.cache.windows.net,password=...");
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
            if (obj == default(T))
            {
                obj = fn();
                this.Put(key, obj, slidingExpiryWindow);
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
