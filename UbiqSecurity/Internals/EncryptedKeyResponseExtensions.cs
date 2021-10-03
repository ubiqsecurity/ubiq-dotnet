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
	internal static class EncryptedKeyResponseExtensions
	{
        internal static void PostProcess(this IEncryptedPrivateKeyModel model, string secretCryptoAccessKey)
        {
            // reference: https://stackoverflow.com/questions/44767290/decrypt-passphrase-protected-pem-containing-private-key
            using (var keyReader = new StringReader(model.EncryptedPrivateKey))
            {
                // decrypt server-provided PEM private key using our secret passphrase
                var pemReader = new PemReader(keyReader, new PasswordFinder(secretCryptoAccessKey));
                object privateKeyObject = pemReader.ReadObject();
                var rsaPrivatekey = privateKeyObject as RsaPrivateCrtKeyParameters;
                if (rsaPrivatekey != null)
                {
                    var rsaEngine = new OaepEncoding(
                        new RsaEngine(),
                        new Sha1Digest(),
                        new Sha1Digest(),
                        null);
                    rsaEngine.Init(false, rsaPrivatekey);

                    // use the client's private key to decrypt the server-provided data key,
                    // storing the result in the 'UnwrappedDataKey' property
                    var wrappedDataKeyBytes = Convert.FromBase64String(model.WrappedDataKey);
                    model.UnwrappedDataKey = rsaEngine.ProcessBlock(wrappedDataKeyBytes, 0, wrappedDataKeyBytes.Length);
                }
            }
        }
    }
}
