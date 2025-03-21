
using Application.Contracts;
using Commons.Classes;
using Commons.Constants;
using Commons.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Models.Entities;
using Models.ViewModels;

namespace Application.Services
{
    public partial class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        public T? Get<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration)
        {
            _memoryCache.Set(key, value, (TimeSpan)(expiration == null ? TimeSpan.FromMinutes(1440) : expiration));
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public void RemoveAll()
        {
            foreach (var key in CacheKeys.AllKeys)
            {
                _memoryCache.Remove(key);
            }
        }
    }
}
