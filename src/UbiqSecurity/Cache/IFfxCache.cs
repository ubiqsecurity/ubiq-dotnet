using System;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
	internal interface IFfxCache : IDisposable
	{
		void Clear();

		Task<FfxContext> GetAsync(FfsRecord ffs, int? keyNumber);

		Task<FfxContext> GetAsync(FfsKeyId key);

		void TryAdd(FfsKeyId key, FfxContext context);
	}
}
