using System;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Cache
{
    internal interface IStructuredKeyCache : IDisposable
    {
        void Clear();

        Task<FpeKeyResponse> GetAsync(FfsKeyId ffsKeyId);

        void TryAdd(FfsKeyId ffsKeyId, FpeKeyResponse response);
    }
}
