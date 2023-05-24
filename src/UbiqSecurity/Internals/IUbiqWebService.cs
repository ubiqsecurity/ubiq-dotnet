using System;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal interface IUbiqWebService : IDisposable
	{
		Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKey);

		Task<EncryptionKeyResponse> GetEncryptionKeyAsync(int uses);

		Task<FfsRecord> GetFfsDefinitionAsync(string ffsName);

		Task<FpeKeyResponse> GetFpeDecryptionKeyAsync(string ffsName, int keyNumber);

		Task<FpeKeyResponse> GetFpeEncryptionKeyAsync(string ffsName);

		Task<FpeBillingResponse> SendTrackingEventsAsync(TrackingEventsRequest trackingEventsRequest);
	}
}
