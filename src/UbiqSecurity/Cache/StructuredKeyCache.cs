using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
    internal class StructuredKeyCache : IStructuredKeyCache, IDisposable
    {
        private readonly IUbiqWebService _ubiqWebService;
        private readonly UbiqConfiguration _configuration;

        private bool _cacheLock;
        private MemoryCache _cache;

        internal StructuredKeyCache(IUbiqWebService ubiqWebService, UbiqConfiguration configuration)
        {
            _ubiqWebService = ubiqWebService;
            _configuration = configuration;

            _cacheLock = false;
            _cache = new MemoryCache("StructuredKey");

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

        public async Task<FpeKeyResponse> GetAsync(FfsKeyId ffsKeyId)
        {
            var cacheKey = $"{ffsKeyId.FfsRecord.Name}|{ffsKeyId.KeyNumber ?? -1}";

            var response = (FpeKeyResponse)_cache.Get(cacheKey);
            if (response == null)
            {
                if (ffsKeyId.KeyNumber.HasValue)
                {
                    response = await _ubiqWebService.GetFpeDecryptionKeyAsync(ffsKeyId.FfsRecord.Name, ffsKeyId.KeyNumber.Value);
                }
                else
                {
                    response = await _ubiqWebService.GetFpeEncryptionKeyAsync(ffsKeyId.FfsRecord.Name);
                }

                if (response == null)
                {
                    throw new InvalidOperationException("FpeKeyResponse is null");
                }

                _cache.Set(cacheKey, response, GetCachePolicy());
            }

            return response;
        }

        public void TryAdd(FfsKeyId ffsKeyId, FpeKeyResponse response)
        {
            var cacheKey = $"{ffsKeyId.FfsRecord.Name}|{ffsKeyId.KeyNumber ?? -1}";

            if (_cache.Contains(cacheKey))
            {
                return;
            }

            _cache.Set(cacheKey, response, GetCachePolicy());
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
                        { "PollingInterval", "00:00:10" }
                    };
                    _cache = new MemoryCache($"StructuredKey:{Guid.NewGuid()}", cacheSettings);
                    _cacheLock = false;
                }
            }
        }

        private CacheItemPolicy GetCachePolicy()
        {
            var ttlSeconds = _configuration.KeyCaching.Structured ? _configuration.KeyCaching.TtlSeconds : 0;

            return new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(ttlSeconds),
            };
        }
    }
}
