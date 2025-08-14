using System;
using System.Threading.Tasks;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Cache
{
    internal interface IUnstructuredKeyCache : IDisposable
    {
        void Clear();

        Task<DecryptionKeyResponse> GetAsync(byte[] encryptedDataKeyBytes);

        void TryAdd(byte[] encryptedDataKeyBytes, DecryptionKeyResponse response);
    }
}
