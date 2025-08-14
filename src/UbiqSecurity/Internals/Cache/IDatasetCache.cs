using System;
using System.Threading.Tasks;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Cache
{
    internal interface IDatasetCache : IDisposable
    {
        void Clear();

        Task<FfsRecord> GetAsync(string datasetName);

        void TryAdd(FfsRecord dataset);
    }
}
