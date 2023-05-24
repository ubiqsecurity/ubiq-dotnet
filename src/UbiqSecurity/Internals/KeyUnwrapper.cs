using System;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal class KeyUnwrapper
	{
		// reference:
		// https://stackoverflow.com/questions/70526272/c-sharp-how-to-decrypt-an-encrypted-private-key-with-bouncy-castle
		public static byte[] UnwrapKey(string encryptedPrivateKey, string wrappedDataKey, string secretCryptoAccessKey)
		{
			var pemReader = new PemReader(new StringReader(encryptedPrivateKey), new PasswordFinder(secretCryptoAccessKey));

			var keyParameters = (RsaPrivateCrtKeyParameters)pemReader.ReadObject();

			var rsaEngine = new OaepEncoding(new RsaEngine(), new Sha1Digest(), new Sha1Digest(), null);
			rsaEngine.Init(false, keyParameters);

			// 'UnwrappedDataKey' is used for local encryptions
			var wrappedDataKeyBytes = Convert.FromBase64String(wrappedDataKey);
			
			var unwrappedDataKey = rsaEngine.ProcessBlock(wrappedDataKeyBytes, 0, wrappedDataKeyBytes.Length);

			return unwrappedDataKey;
		}
	}
}
