using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal class FpeEncryptedKeyCacheManager : ICacheManager<FpeEncryptionKeyResponse>, IDisposable
	{
		#region Private Data

		private MemoryCache _cache;
		private bool _cacheLock;
		private readonly IUbiqCredentials _credentials;
		private UbiqWebServices _ubiqWebServices;

		private CacheItemPolicy _defaultPolicy = new CacheItemPolicy
		{
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30)
        };

		#endregion

		#region Constructors

		internal FpeEncryptedKeyCacheManager(IUbiqCredentials credentials, UbiqWebServices ubiqWebServices)
		{
			_cacheLock = false;
			_credentials = credentials;
			_ubiqWebServices = ubiqWebServices;
			_cache = new MemoryCache("fpeCache");
			InitCache();
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_cache.Dispose();
		}

		#endregion

		#region ICacheManager

		public void Clear()
		{
			_cache.Dispose();
			InitCache();
		}

		public async Task<FpeEncryptionKeyResponse> GetAsync(CacheKey key, bool encrypt)
		{
			var url = "";
			if (key.KeyNumber == null)
			{
				url = UrlHelper.GenerateFpeUrlEncrypt(key.FfsName, _credentials);
			}
			else
			{
				url = UrlHelper.GenerateFpeUrlDecrypt(key.FfsName, key.KeyNumber, _credentials);
			}
			if (!_cache.Contains(url))
			{
				var fpeKey = await LoadAsync(key.FfsName, key.KeyNumber);

				_cache.Add(url, fpeKey, _defaultPolicy);
				var keyUrl = "";

				if (encrypt == true)
				{
					keyUrl = UrlHelper.GenerateFpeUrlEncrypt(key.FfsName, _credentials);
				}
				else
				{
					keyUrl = UrlHelper.GenerateFpeUrlDecrypt(key.FfsName, key.KeyNumber, _credentials);
				}


				if(url != keyUrl)
				{
					_cache.Add(keyUrl, fpeKey, _defaultPolicy);
				}
			}

			var fpe = (FpeEncryptionKeyResponse)_cache.Get(url);
            // decrypt the server-provided encryption key
            fpe.PostProcess(_credentials.SecretCryptoAccessKey);

            return fpe;
		}

		#endregion

		#region Private Methods

		private void InitCache()
		{
			if (!_cacheLock)
			{
				_cacheLock = true;
				lock (_cache)
				{
					var cacheSettings = new NameValueCollection(3);
					cacheSettings.Add("CacheMemoryLimitMegabytes", Convert.ToString(1024));
                    cacheSettings.Add("PhysicalMemoryLimit", Convert.ToString(5));  //set % here
                    cacheSettings.Add("pollingInterval", Convert.ToString("00:00:10"));
					_cache = new MemoryCache($"FPE:{Guid.NewGuid()}", cacheSettings);
					_cacheLock = false;
				}
			}
		}

		private async Task<FpeEncryptionKeyResponse> LoadAsync(string ffsName, int? keyNumber)
		{
			return await _ubiqWebServices.GetFpeEncryptionKeyAsync(ffsName, keyNumber);
		}

		#endregion
	}
}
