using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	public interface ICacheManager<T>
		where T : class
	{
		void Clear();

		Task<T> GetAsync(CacheKey key, bool encrypt);
	}
}
