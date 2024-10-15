using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UbiqSecurity.Billing;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
	public class UbiqEncrypt : IDisposable
	{
		private readonly IUbiqCredentials _ubiqCredentials;
		private readonly IBillingEventsManager _billingEvents;
		private readonly int _usesRequested;

		private IUbiqWebService _ubiqWebService;	// null when disposed
		private EncryptionKeyResponse _encryptionKey;
		private AesGcmBlockCipher _aesGcmBlockCipher;

		public UbiqEncrypt(IUbiqCredentials ubiqCredentials, int usesRequested)
			: this(ubiqCredentials, usesRequested, new UbiqConfiguration())
		{
		}

		public UbiqEncrypt(IUbiqCredentials ubiqCredentials, int usesRequested, UbiqConfiguration ubiqConfiguration)
		{
			_ubiqCredentials = ubiqCredentials;
			_usesRequested = usesRequested;
			_ubiqWebService = new UbiqWebServices(_ubiqCredentials);
			_billingEvents = new BillingEventsManager(ubiqConfiguration, _ubiqWebService);
		}

		public void AddReportingUserDefinedMetadata(string jsonString)
		{
			_billingEvents.AddUserDefinedMetadata(jsonString);
		}

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_billingEvents?.Dispose();

				if (_ubiqWebService != null)
				{
					_ubiqWebService.Dispose();
					_ubiqWebService = null;
				}
			}
		}

		public async Task<byte[]> EncryptAsync(byte[] plainBytes)
		{
			// handy inline function
			void WriteBytesToStream(Stream stream, byte[] bytes)
			{
				stream.Write(bytes, 0, bytes.Length);
			}

			using (var memoryStream = new MemoryStream())
			{
				WriteBytesToStream(memoryStream, await BeginAsync().ConfigureAwait(false));
				WriteBytesToStream(memoryStream, Update(plainBytes, 0, plainBytes.Length));
				WriteBytesToStream(memoryStream, End());

				return memoryStream.ToArray();
			}
		}

		public string GetCopyOfUsage() => _billingEvents.GetSerializedEvents();

		public async Task<byte[]> BeginAsync()
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			else if (_aesGcmBlockCipher != null)
			{
				throw new InvalidOperationException("encryption in progress");
			}

			if (_encryptionKey == null)
			{
				// JIT: request encryption key from server
				_encryptionKey = await _ubiqWebService.GetEncryptionKeyAsync(_usesRequested).ConfigureAwait(false);
			}

			await _billingEvents.AddBillingEventAsync(_ubiqCredentials.AccessKeyId, string.Empty, string.Empty, BillingAction.Encrypt, DatasetType.Unstructured, 0, 1);

			var algorithmInfo = new AlgorithmInfo(_encryptionKey.SecurityModel.Algorithm);

			// generate random IV for encryption
			byte[] initVector = new byte[algorithmInfo.InitVectorLength];
			var random = RandomNumberGenerator.Create();
			random.GetBytes(initVector);

			var cipherHeader = new CipherHeader
			{
				Version = 0,
				Flags = CipherHeader.FLAGS_AAD_ENABLED,
				AlgorithmId = algorithmInfo.Id,
				InitVectorLength = (byte)initVector.Length,
				EncryptedDataKeyLength = (short)_encryptionKey.EncryptedDataKeyBytes.Length,
				InitVectorBytes = initVector,
				EncryptedDataKeyBytes = _encryptionKey.EncryptedDataKeyBytes
			};

			// note: include cipher header bytes in AES result!
			var cipherHeaderBytes = cipherHeader.Serialize();

			_aesGcmBlockCipher = new AesGcmBlockCipher(
				forEncryption: true,
				algorithmInfo: algorithmInfo, 
				key: _encryptionKey.UnwrappedDataKey,
				initVector: initVector,
				additionalBytes: cipherHeaderBytes);

			return cipherHeaderBytes;
		}

		public byte[] Update(byte[] plainBytes, int offset, int count)
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			else if ((_encryptionKey == null) || (_aesGcmBlockCipher == null))
			{
				throw new InvalidOperationException("encryptor not initialized");
			}

			var cipherBytes = _aesGcmBlockCipher.Update(plainBytes, offset, count);
			return cipherBytes;
		}

		public byte[] End()
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			var finalBytes = _aesGcmBlockCipher.Finalize();
			_aesGcmBlockCipher = null;
			return finalBytes;
		}

        [Obsolete("Static EncryptAsync method is deprecated, please use equivalent instance method")]
		public static async Task<byte[]> EncryptAsync(IUbiqCredentials ubiqCredentials, byte[] plainBytes)
		{
			using (var ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1))
			{
				return await ubiqEncrypt.EncryptAsync(plainBytes);
			}
		}
	}
}
