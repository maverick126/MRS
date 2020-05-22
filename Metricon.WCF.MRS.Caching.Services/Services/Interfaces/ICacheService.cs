using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metricon.WCF.MRS.Caching.Services.Services.Interfaces
{
    public interface ICacheService
    {
        bool KeyExists(string key);

        T Get<T>(string key);

        bool Set<T>(string key, T value);

        Task<T> GetAsync<T>(string key);

        Task<bool> SetAsync<T>(string key, T value);
    }
}
