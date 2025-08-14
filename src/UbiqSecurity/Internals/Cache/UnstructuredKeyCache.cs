using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Cache
{
    internal class UnstructuredKeyCache : IUnstructuredKeyCache, IDisposable
    {
        private readonly IUbiqWebService _ubiqWebService;
        private readonly UbiqConfiguration _configuration;
        private readonly IUbiqCredentials _credentials;

        private bool _cacheLock;
        private MemoryCache _cache;

        internal UnstructuredKeyCache(IUbiqWebService ubiqWebService, UbiqConfiguration configuration, IUbiqCredentials credentials)
        {
            _ubiqWebService = ubiqWebService;
            _configuration = configuration;
            _credentials = credentials;

            _cacheLock = false;
            _cache = new MemoryCache("UnstructuredKey");

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

        public async Task<DecryptionKeyResponse> GetAsync(byte[] encryptedDataKeyBytes)
        {
            var key = Convert.ToBase64String(encryptedDataKeyBytes);

            var response = (DecryptionKeyResponse)_cache.Get(key);
            if (response == null)
            {
                response = await GetDecryptionKeyAsync(encryptedDataKeyBytes);
                if (response == null)
                {
                    throw new InvalidOperationException("DecryptionKeyResponse is null");
                }

                _cache.Set(key, response, GetCachePolicy());
            }

            // decryption key response may have been cached w/ or w/o the unwrapped key populated based on configuration.KeyCaching.Encrypt
            if (response.UnwrappedDataKey == null || response.UnwrappedDataKey.Length == 0)
            {
                response.UnwrappedDataKey = PayloadEncryption.UnwrapDataKey(
                    response.EncryptedPrivateKey,
                    response.WrappedDataKey,
                    _credentials.SecretCryptoAccessKey);
            }

            return response;
        }

        public void TryAdd(byte[] encryptedDataKeyBytes, DecryptionKeyResponse response)
        {
            var key = Convert.ToBase64String(encryptedDataKeyBytes);

            if (_cache.Contains(key))
            {
                return;
            }

            _cache.Set(key, response, GetCachePolicy());
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
                    _cache = new MemoryCache($"UnstructuredKey:{Guid.NewGuid()}", cacheSettings);
                    _cacheLock = false;
                }
            }
        }

        private CacheItemPolicy GetCachePolicy()
        {
            var ttlSeconds = _configuration.KeyCaching.Unstructured ? _configuration.KeyCaching.TtlSeconds : 0;

            return new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(ttlSeconds),
            };
        }

        private async Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKeyBytes)
        {
            var decryptionKeyResponse = await _ubiqWebService.GetDecryptionKeyAsync(encryptedDataKeyBytes);

            // don't include unwrapped key if we want the cache to be encrypted
            if (_configuration.KeyCaching?.Encrypt == true)
            {
                decryptionKeyResponse.UnwrappedDataKey = null;
            }

            return decryptionKeyResponse;
        }
    }
}
