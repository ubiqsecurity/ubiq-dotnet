using System;
using System.Collections.Specialized;
#if DEBUG
using System.Diagnostics;
#endif
using System.Runtime.Caching;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
	internal class DatasetCache : IDatasetCache, IDisposable
	{
		private static CacheItemPolicy DefaultPolicy => new CacheItemPolicy
		{
			AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30)
		};

		private readonly IUbiqWebService _ubiqWebService;

		private MemoryCache _cache;
		private bool _cacheLock;

		internal DatasetCache(IUbiqWebService ubiqWebService)
		{
			_cacheLock = false;
			_ubiqWebService = ubiqWebService;
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
#if DEBUG
                Debug.WriteLine($"FFX cache miss {ffsName}");
#endif

				ffs = await _ubiqWebService.GetFfsDefinitionAsync(ffsName);
				if (ffs == null)
				{
					throw new ArgumentException($"Dataset '{ffsName}' does not exist", nameof(ffsName));
				}

				_cache.Set(ffsName, ffs, DefaultPolicy);
			}

			return ffs;
		}

		public void TryAdd(FfsRecord dataset)
		{
			if (_cache.Contains(dataset.Name))
			{
				return;
			}

			_cache.Set(dataset.Name, dataset, DefaultPolicy);
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
						{ "PhysicalMemoryLimit", $"{5}" },  //set % here
						{ "pollingInterval", "00:00:10" }
					};
					_cache = new MemoryCache($"FFS:{Guid.NewGuid()}", cacheSettings);
					_cacheLock = false;
				}
			}
		}
	}
}
