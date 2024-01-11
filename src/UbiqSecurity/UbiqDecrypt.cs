using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UbiqSecurity.Billing;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
	public class UbiqDecrypt : IDisposable
	{
		private readonly IUbiqCredentials _credentials;
		private readonly IBillingEventsManager _billingEvents;

		private IUbiqWebService _ubiqWebService;	// null on dispose
		private CipherHeader _cipherHeader;			// extracted from beginning of ciphertext
		private ByteBuffer _byteBuffer;
		private DecryptionKeyResponse _decryptionKey;
		private AesGcmBlockCipher _aesGcmBlockCipher;

		public UbiqDecrypt(IUbiqCredentials ubiqCredentials)
			: this(ubiqCredentials, new UbiqConfiguration())
		{
		}

		public UbiqDecrypt(IUbiqCredentials ubiqCredentials, UbiqConfiguration configuration)
		{
			_credentials = ubiqCredentials;
			_ubiqWebService = new UbiqWebServices(ubiqCredentials);
			_billingEvents = new BillingEventsManager(configuration, _ubiqWebService);
		}

		internal UbiqDecrypt(IUbiqCredentials ubiqCredentials, IUbiqWebService webService, IBillingEventsManager billingEvents)
		{
			_credentials = ubiqCredentials;
			_ubiqWebService = webService;
			_billingEvents = billingEvents;
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
				Reset();

				_billingEvents?.Dispose();

				if (_ubiqWebService != null)
				{
					_ubiqWebService.Dispose();
					_ubiqWebService = null;
				}
			}
		}

		public async Task<byte[]> DecryptAsync(byte[] data)
		{
			// handy inline function
			void WriteBytesToStream(Stream stream, byte[] bytes)
			{
				stream.Write(bytes, 0, bytes.Length);
			}

			using (var memoryStream = new MemoryStream())
			{
				
				WriteBytesToStream(memoryStream, Begin());
				WriteBytesToStream(memoryStream, await UpdateAsync(data, 0, data.Length).ConfigureAwait(false));
				WriteBytesToStream(memoryStream, End());

				return memoryStream.ToArray();
			}
		}

		public byte[] Begin()
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(nameof(_ubiqWebService));
			}

			if (_aesGcmBlockCipher != null)
			{
				throw new InvalidOperationException("decryption in progress");
			}

			// prepare to receive initial header bytes
			_cipherHeader = null;
			_byteBuffer = null;

			// note: cached '_decryptionKey' may be present from a previous decryption run

			return Array.Empty<byte>();
		}

		// each encryption has a header on it that identifies the algorithm
		// used and an encryption of the data key that was used to encrypt
		// the original plain text. there is no guarantee how much of that
		// data will be passed to this function or how many times this
		// function will be called to process all of the data. to that end,
		// this function buffers data internally, when it is unable to
		// process it.

		// the function buffers data internally until the entire header is
		// received. once the header has been received, the encrypted data
		// key is sent to the server for decryption. after the header has
		// been successfully handled, this function always decrypts all of
		// the data in its internal buffer
		public async Task<byte[]> UpdateAsync(byte[] cipherBytes, int offset, int count)
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(nameof(_ubiqWebService));
			}

			byte[] plainBytes = Array.Empty<byte>();	// returned

			if (_byteBuffer == null)
			{
				_byteBuffer = new ByteBuffer();
			}

			// make sure new data is appended to end
			_byteBuffer.Enqueue(cipherBytes, offset, count);

			if (_cipherHeader == null)
			{
				// see if we've got enough data for the header record
				using (var byteStream = new MemoryStream(_byteBuffer.Peek()))
				{
					_cipherHeader = CipherHeader.Deserialize(byteStream);
				}

				if (_cipherHeader != null)
				{
					// success: prune cipher header bytes from the buffer
					_byteBuffer.Dequeue(_cipherHeader.Length());

					if (_decryptionKey != null)
					{
						// See if we can reuse the key from a previous decryption, meaning
						// the new data was encrypted with the same key as the old data - i.e.
						// both cipher headers have the same key.
						//
						// If not, clear the previous decryption key.
						if (!_cipherHeader.EncryptedDataKeyBytes.SequenceEqual(_decryptionKey.LastCipherHeaderEncryptedDataKeyBytes))
						{
							Reset();
						}
					}

					// If needed, use the header info to fetch the decryption key.
					if (_decryptionKey == null)
					{
						// JIT: request encryption key from server
						_decryptionKey = await _ubiqWebService.GetDecryptionKeyAsync(_cipherHeader.EncryptedDataKeyBytes).ConfigureAwait(false);
					}

					if (_decryptionKey != null)
					{
						var algorithmInfo = new AlgorithmInfo(_cipherHeader.AlgorithmId);

						// save key extracted from header to detect future key changes
						_decryptionKey.LastCipherHeaderEncryptedDataKeyBytes = _cipherHeader.EncryptedDataKeyBytes;

						// create decryptor from header-specified algorithm + server-supplied decryption key
						_aesGcmBlockCipher = new AesGcmBlockCipher(forEncryption: false,
							algorithmInfo: algorithmInfo,
							key: _decryptionKey.UnwrappedDataKey,
							initVector: _cipherHeader.InitVectorBytes,
							additionalBytes: ((_cipherHeader.Flags & CipherHeader.FLAGS_AAD_ENABLED) != 0)
								? _cipherHeader.Serialize()
								: null);

						await _billingEvents.AddBillingEventAsync(_credentials.AccessKeyId, string.Empty, string.Empty, BillingAction.Decrypt, DatasetType.Unstructured, 0, 1);
					}
				}
				else
				{
					// holding pattern... need more header bytes
					return plainBytes;
				}
			}

			if (_decryptionKey != null && _aesGcmBlockCipher != null)
			{
				// pass all available buffered bytes to the decryptor
				if (_byteBuffer.Length > 0)
				{
					// tricky: the block cipher object will process all provided ciphertext 
					// (including the trailing MAC signature), but may only return a subset of that
					// as plaintext
					var bufferedBytes = _byteBuffer.Dequeue(_byteBuffer.Length);
					plainBytes = _aesGcmBlockCipher.Update(bufferedBytes, 0, bufferedBytes.Length);
				}
			}

			return plainBytes;
		}

		public byte[] End()
		{
			if (_ubiqWebService == null)
			{
				throw new ObjectDisposedException(nameof(_ubiqWebService));
			}

			var finalPlainBytes = _aesGcmBlockCipher.Finalize();
			_aesGcmBlockCipher = null;
			_byteBuffer = null;
			return finalPlainBytes;
		}

		public static async Task<byte[]> DecryptAsync(IUbiqCredentials ubiqCredentials, byte[] data)
		{
			using (var ubiqDecrypt = new UbiqDecrypt(ubiqCredentials))
			{
				return await ubiqDecrypt.DecryptAsync(data);
			}
		}

		// Reset the internal state of the decryption object.
		// This function can be called at any time to abort an existing
		// decryption operation.  It is also called by internal functions
		// when a new decryption requires a different key than the one
		// used by the previous decryption.
		private void Reset()
		{
			if (_decryptionKey != null)
			{
				_decryptionKey = null;
			}

			_aesGcmBlockCipher = null;
		}
	}
}
