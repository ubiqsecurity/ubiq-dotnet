using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal class FfsCacheManager : ICacheManager<FfsConfigurationResponse>, IDisposable
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

		internal FfsCacheManager(IUbiqCredentials credentials, UbiqWebServices ubiqWebServices)
		{
			_cacheLock = false;
			_credentials = credentials;
			_ubiqWebServices = ubiqWebServices;
			_cache = new MemoryCache("ffsCache");
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

		public async Task<FfsConfigurationResponse> GetAsync(CacheKey key)
		{
			var url = UrlHelper.GenerateFfsUrl(key.FfsName, _credentials);
			if(!_cache.Contains(url))
			{
				var ffs = await LoadAsync(key.FfsName);
				_cache.Add(url, ffs, _defaultPolicy);
			}

			return (FfsConfigurationResponse)_cache.Get(url);
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
					_cache = new MemoryCache($"FFS:{Guid.NewGuid()}", cacheSettings);
					_cacheLock = false;
				}
			}
		}

		private async Task<FfsConfigurationResponse> LoadAsync(string ffsName)
		{
			return await _ubiqWebServices.GetFfsConfigurationsync(ffsName);
		}

		#endregion
	}
}
