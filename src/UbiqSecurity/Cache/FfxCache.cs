using System;
using System.Collections.Specialized;
#if DEBUG
using System.Diagnostics;
#endif
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Fpe;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
	internal class FfxCache : IFfxCache, IDisposable
	{
		private static readonly CacheItemPolicy DefaultPolicy = new CacheItemPolicy
		{
			AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30)
		};

		private MemoryCache _cache;
		private bool _cacheLock;
		private readonly IUbiqCredentials _credentials;
		private readonly IUbiqWebService _ubiqWebService;

		internal FfxCache(IUbiqCredentials credentials, IUbiqWebService ubiqWebService)
		{
			_cacheLock = false;
			_credentials = credentials;
			_ubiqWebService = ubiqWebService;
			_cache = new MemoryCache("FFX");
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

		public async Task<FfxContext> GetAsync(FfsRecord ffs, int? keyNumber)
		{
			return await GetAsync(new FfsKeyId { FfsRecord = ffs, KeyNumber = keyNumber });
		}

		public async Task<FfxContext> GetAsync(FfsKeyId key)
		{
			var cacheKey = $"{key.FfsRecord.Name}|{key.KeyNumber ?? -1}";

			if (!_cache.Contains(cacheKey))
			{
#if DEBUG
				Debug.WriteLine($"FFX cache miss {cacheKey}");
#endif

				var ffxContext = await GetFfsKeyAsync(key);

				_cache.Set(cacheKey, ffxContext, DefaultPolicy);

				// if we were doing an encypt and didn't know the keynumber ahead of time (just want to use the latest one)
				// we'll cache it twice, once w/ keynumber -1 (for future encrypts) and onces as the real keynumber (for future decrypts)
				if (!key.KeyNumber.HasValue)
				{
					_cache.Set($"{key.FfsRecord.Name}|{ffxContext.KeyNumber}", ffxContext, DefaultPolicy);
				}
			}

			return (FfxContext)_cache.Get(cacheKey);
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

		private async Task<FfxContext> GetFfsKeyAsync(FfsKeyId keyId)
		{
			FfxContext context = new FfxContext();

			FpeKeyResponse keyResponse;

			if (keyId.KeyNumber.HasValue)
			{
				keyResponse = await _ubiqWebService.GetFpeDecryptionKeyAsync(keyId.FfsRecord.Name, keyId.KeyNumber.Value);
			}
			else
			{
				keyResponse = await _ubiqWebService.GetFpeEncryptionKeyAsync(keyId.FfsRecord.Name);
			}

			if (keyResponse.EncryptedPrivateKey == null || keyResponse.WrappedDataKey == null)
			{
				throw new InvalidOperationException("Missing keys in FPE key definition");
			}

			byte[] tweak = Array.Empty<byte>();
			if (keyId.FfsRecord.TweakSource == "constant")
			{
				tweak = Convert.FromBase64String(keyId.FfsRecord.Tweak);
			}

			byte[] key = KeyUnwrapper.UnwrapKey(keyResponse.EncryptedPrivateKey, keyResponse.WrappedDataKey, _credentials.SecretCryptoAccessKey);

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
				case "FF3_1":
					context.SetFF3_1(
						new FF3_1(
							key,
							tweak,
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
