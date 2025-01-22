using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Fpe;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
    internal class FfxCache : IFfxCache, IDisposable
    {
        private readonly IUbiqCredentials _credentials;
        private readonly UbiqConfiguration _configuration;
        private readonly IStructuredKeyCache _structuredKeyCache;

        private MemoryCache _cache;
        private bool _cacheLock;

        internal FfxCache(IUbiqWebService ubiqWebService, UbiqConfiguration configuration, IUbiqCredentials credentials)
        {
            _configuration = configuration;
            _credentials = credentials;
            _structuredKeyCache = new StructuredKeyCache(ubiqWebService, configuration);

            _cacheLock = false;
            _cache = new MemoryCache("FFX");

            InitCache();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            _cache.Dispose();
            InitCache();
        }

        public async Task<FfxContext> GetAsync(FfsRecord ffs, int? keyNumber)
        {
            return await GetAsync(new FfsKeyId { FfsRecord = ffs, KeyNumber = keyNumber });
        }

        public async Task<FfxContext> GetAsync(FfsKeyId key)
        {
            var cacheKey = $"{key.FfsRecord.Name}|{key.KeyNumber ?? -1}";

            var ffx = (FfxContext)_cache.Get(cacheKey);
            if (ffx == null)
            {
                ffx = await GetFfsKeyAsync(key);

                _cache.Set(cacheKey, ffx, GetCachePolicy());

                // if we were doing an encypt and didn't know the keynumber ahead of time (just want to use the latest one)
                // we'll cache it twice, once w/ keynumber -1 (for future encrypts) and onces as the real keynumber (for future decrypts)
                if (!key.KeyNumber.HasValue)
                {
                    _cache.Set($"{key.FfsRecord.Name}|{ffx.KeyNumber}", ffx, GetCachePolicy());
                }
            }

            return ffx;
        }

        public void TryAdd(FfsKeyId key, FfxContext context)
        {
            var cacheKey = $"{key.FfsRecord.Name}|{key.KeyNumber}";

            if (_cache.Contains(cacheKey))
            {
                return;
            }

            _cache.Set(cacheKey, context, GetCachePolicy());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cache?.Dispose();
            }
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
                        { "CacheMemoryLimitMegabytes", $"{1024}" },
                        { "PhysicalMemoryLimit", $"{5}" },	//set % here
						{ "pollingInterval", "00:00:10" }
                    };
                    _cache = new MemoryCache($"FFX:{Guid.NewGuid()}", cacheSettings);
                    _cacheLock = false;
                }
            }
        }

        private CacheItemPolicy GetCachePolicy()
        {
            var ttlSeconds = _configuration.KeyCaching.TtlSeconds;

            if (!_configuration.KeyCaching.Structured || _configuration.KeyCaching.Encrypt)
            {
                ttlSeconds = 0;
            }

            return new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(ttlSeconds),
            };
        }

        private async Task<FfxContext> GetFfsKeyAsync(FfsKeyId keyId)
        {
            FfxContext context = new FfxContext();

            var keyResponse = await _structuredKeyCache.GetAsync(keyId);

            if (keyResponse.EncryptedPrivateKey == null || keyResponse.WrappedDataKey == null)
            {
                throw new InvalidOperationException("Missing keys in FPE key definition");
            }

            byte[] tweak = Array.Empty<byte>();
            if (keyId.FfsRecord.TweakSource == "constant")
            {
                tweak = Convert.FromBase64String(keyId.FfsRecord.Tweak);
            }

            byte[] key = PayloadEncryption.UnwrapDataKey(keyResponse.EncryptedPrivateKey, keyResponse.WrappedDataKey, _credentials.SecretCryptoAccessKey);

            switch (keyId.FfsRecord.EncryptionAlgorithm)
            {
                case "FF1":
                    context.SetFF1(
                        new FF1(
                            key,
                            tweak,
                            keyId.FfsRecord.TweakMinLength,
                            keyId.FfsRecord.TweakMaxLength,
                            keyId.FfsRecord.InputCharacters.Length,
                            keyId.FfsRecord.InputCharacters
                        ),
                        keyResponse.KeyNumber
                    );
                    break;
                default:
                    throw new NotSupportedException($"Unsupported FPE Algorithm: {keyId.FfsRecord.EncryptionAlgorithm}");
            }

            return context;
        }
    }
}
