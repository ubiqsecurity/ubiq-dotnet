using System;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Cache
{
	internal interface IDatasetCache : IDisposable
	{
		void Clear();

		Task<FfsRecord> GetAsync(string datasetName);
	}
}
