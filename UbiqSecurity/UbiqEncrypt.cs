using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
	public class UbiqEncrypt : IDisposable
	{
		#region Private Data

		private readonly IUbiqCredentials _ubiqCredentials;
		private readonly int _usesRequested;

		private UbiqWebServices _ubiqWebServices;       // null when disposed
		private int _useCount;
		private EncryptionKeyResponse _encryptionKey;
		private AesGcmBlockCipher _aesGcmBlockCipher;

		#endregion

		#region Constructors

		public UbiqEncrypt(IUbiqCredentials ubiqCredentials, int usesRequested)
		{
			_ubiqCredentials = ubiqCredentials;
			_usesRequested = usesRequested;
			_ubiqWebServices = new UbiqWebServices(_ubiqCredentials);
		}

		#endregion

		#region IDisposable

		public virtual void Dispose()
		{
			if (_ubiqWebServices != null)
			{
				if (_encryptionKey != null)
				{
					// if key was used less times than requested, notify the server.
					if (_useCount < _usesRequested)
					{
						_ubiqWebServices.UpdateEncryptionKeyUsageAsync(_useCount, _usesRequested,
						_encryptionKey.KeyFingerprint, _encryptionKey.EncryptionSession).Wait();
					}
				}

				_ubiqWebServices.Dispose();
				_ubiqWebServices = null;
			}
		}

		#endregion

		#region Public Methods

		public async Task<byte[]> BeginAsync()
		{
			if (_ubiqWebServices == null)
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
				_encryptionKey = await _ubiqWebServices.GetEncryptionKeyAsync(_usesRequested).ConfigureAwait(false);
			}

			// check key 'usage count' against server-specified limit
			if (_useCount > _encryptionKey.MaxUses)
			{
				throw new InvalidOperationException("maximum key uses exceeded");
			}

			_useCount++;

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
			if (_ubiqWebServices == null)
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
			if (_ubiqWebServices == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			var finalBytes = _aesGcmBlockCipher.Finalize();
			_aesGcmBlockCipher = null;
			return finalBytes;
		}

		#endregion

		#region Static Methods

		public static async Task<byte[]> EncryptAsync(IUbiqCredentials ubiqCredentials, byte[] data)
		{
			// handy inline function
			void WriteBytesToStream(Stream stream, byte[] bytes)
			{
				stream.Write(bytes, 0, bytes.Length);
			}

			using (var memoryStream = new MemoryStream())
			{
				using (var ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1))
				{
					WriteBytesToStream(memoryStream, await ubiqEncrypt.BeginAsync().ConfigureAwait(false));
					WriteBytesToStream(memoryStream, ubiqEncrypt.Update(data, 0, data.Length));
					WriteBytesToStream(memoryStream, ubiqEncrypt.End());
				}

				return memoryStream.ToArray();
			}
		}

		#endregion
	}
}
