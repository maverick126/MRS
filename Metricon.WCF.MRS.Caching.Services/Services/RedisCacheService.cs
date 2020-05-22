using Metricon.WCF.MRS.Caching.Services.Services.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Configuration;
using System.Threading.Tasks;

namespace Metricon.WCF.MRS.Caching.Services.Services
{
    public class RedisCacheService : ICacheService
    {
        private static ConnectionMultiplexer _conn = null;
        private static ConnectionMultiplexer Connection
        {
            get
            {
                if (_conn == null || !_conn.IsConnected)
                {
                    _conn = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(ConfigurationManager.AppSettings["ElasticCacheConnectionString"].ToString()));
                }
                return _conn;
            }
        }
        private static IDatabase Database
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        public bool KeyExists(string key)
        {
            return Database.KeyExists(key);
        }

        public T Get<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(Database.StringGet(key));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>( await Database.StringGetAsync(key));
        }

        public bool Set<T>(string key, T value)
        {
            return Database.StringSet(key, JsonConvert.SerializeObject(value));
        }

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            return await Database.StringSetAsync(key, JsonConvert.SerializeObject(value));
        }
    }
}
