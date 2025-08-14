using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Cache
{
    internal class DatasetCache : IDatasetCache, IDisposable
    {
        private readonly IUbiqWebService _ubiqWebService;
        private readonly UbiqConfiguration _configuration;

        private bool _cacheLock;
        private MemoryCache _cache;

        internal DatasetCache(IUbiqWebService ubiqWebService, UbiqConfiguration configuration)
        {
            _ubiqWebService = ubiqWebService;
            _configuration = configuration;

            _cacheLock = false;
            _cache = new MemoryCache("FFS");

            InitCache();
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        public void Clear()
        {
            _cache.Dispose();
            InitCache();
        }

        public async Task<FfsRecord> GetAsync(string ffsName)
        {
            var ffs = (FfsRecord)_cache.Get(ffsName);
            if (ffs == null)
            {
                ffs = await _ubiqWebService.GetDatasetAsync(ffsName);
                if (ffs == null)
                {
                    throw new ArgumentException($"Dataset '{ffsName}' does not exist", nameof(ffsName));
                }

                _cache.Set(ffsName, ffs, GetCachePolicy());
            }

            return ffs;
        }

        public void TryAdd(FfsRecord dataset)
        {
            if (_cache.Contains(dataset.Name))
            {
                return;
            }

            _cache.Set(dataset.Name, dataset, GetCachePolicy());
        }

        private void InitCache()
        {
            if (!_cacheLock)
            {
                _cacheLock = true;
                lock (_cache)
                {
                    var cacheSettings = new NameValueCollection(3)
                    {
                        { "CacheMemoryLimitMegabytes", "1024" },
                        { "PhysicalMemoryLimit", "5" },
                        { "PollingInterval", "00:00:10" },
                    };
                    _cache = new MemoryCache($"FFS:{Guid.NewGuid()}", cacheSettings);
                    _cacheLock = false;
                }
            }
        }

        private CacheItemPolicy GetCachePolicy()
        {
            var ttlSeconds = _configuration.KeyCaching.TtlSeconds;

            return new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(ttlSeconds),
            };
        }
    }
}
