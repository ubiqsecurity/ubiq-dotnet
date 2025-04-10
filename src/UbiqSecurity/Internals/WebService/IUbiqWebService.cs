using System;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals.WebService
{
    internal interface IUbiqWebService : IDisposable
    {
        Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKey);

        Task<EncryptionKeyResponse> GetEncryptionKeyAsync(int uses);

        Task<FfsRecord> GetDatasetAsync(string datasetName);

        Task<FpeKeyResponse> GetFpeDecryptionKeyAsync(string datasetName, int keyNumber);

        Task<FpeKeyResponse> GetFpeEncryptionKeyAsync(string datasetName);

        Task<DatasetAndKeysResponse> GetDatasetAndKeysAsync(string datasetName);

        Task<FpeBillingResponse> SendTrackingEventsAsync(TrackingEventsRequest trackingEventsRequest);
    }
}
