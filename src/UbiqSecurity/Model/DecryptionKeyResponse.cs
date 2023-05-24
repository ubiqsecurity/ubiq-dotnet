using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.IO;

namespace UbiqSecurity.Model
{
	internal class DecryptionKeyResponse
	{
		#region Serializable Properties
		[JsonProperty("encrypted_private_key")]
		public string EncryptedPrivateKey { get; set; }

		[JsonProperty("encryption_session")]
		public string EncryptionSession { get; set; }

		[JsonProperty("key_fingerprint")]
		public string KeyFingerprint { get; set; }

		[JsonProperty("wrapped_data_key")]
		public string WrappedDataKey { get; set; }
		#endregion

		#region Non-serialized Properties (runtime only)
		[JsonIgnore]
		public byte[] UnwrappedDataKey { get; private set; }

		[JsonIgnore]
		public int KeyUseCount { get; set; }

		[JsonIgnore]
		public byte[] LastCipherHeaderEncryptedDataKeyBytes { get; set; }
		#endregion

		#region Methods
		public void PostProcess(string secretCryptoAccessKey)
		{
			// reference: https://stackoverflow.com/questions/44767290/decrypt-passphrase-protected-pem-containing-private-key
			using (var keyReader = new StringReader(EncryptedPrivateKey))
			{
				// decrypt server-provided PEM private key using our secret passphrase
				var pemReader = new PemReader(keyReader, new PasswordFinder(secretCryptoAccessKey));
				object privateKeyObject = pemReader.ReadObject();

				if (privateKeyObject is RsaPrivateCrtKeyParameters rsaPrivatekey)
				{
					var rsaEngine = new OaepEncoding(
						new RsaEngine(),
						new Sha1Digest(),
						new Sha1Digest(),
						null);
					rsaEngine.Init(false, rsaPrivatekey);

					// use the client's private key to decrypt the server-provided data key,
					// storing the result in the 'UnwrappedDataKey' property
					var wrappedDataKeyBytes = Convert.FromBase64String(WrappedDataKey);
					UnwrappedDataKey = rsaEngine.ProcessBlock(wrappedDataKeyBytes, 0, wrappedDataKeyBytes.Length);
				}
			}
		}
		#endregion
	}
}
