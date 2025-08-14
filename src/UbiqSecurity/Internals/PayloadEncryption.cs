using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace UbiqSecurity.Internals
{
    internal partial class PayloadEncryption
    {
        private const int ApiKeyLength = 18;
        private const int CryptoAccessKeyLength = 33;

        public static string GenerateRandomCryptoAccessKey()
        {
            return GenerateRandomBase64String(CryptoAccessKeyLength);
        }

        public static string GenerateRandomBase64String(int byteCount)
        {
            var cryptoProvider = new RNGCryptoServiceProvider();

            byte[] accessKeyBytes = new byte[byteCount];

            cryptoProvider.GetBytes(accessKeyBytes);

            return Convert.ToBase64String(accessKeyBytes);
        }

        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();

            KeyGenerationParameters keyParams = new KeyGenerationParameters(new SecureRandom(), 2048);

            generator.Init(keyParams);

            var pair = generator.GenerateKeyPair();

            return pair;
        }

        public static string EncryptPrivateKey(AsymmetricKeyParameter privateKey, string cryptoAccessKey)
        {
            var encryptedPrivateKeyInfo = EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(
               PkcsObjectIdentifiers.PbeWithSha1AndRC2Cbc,
               cryptoAccessKey.ToCharArray(),
               Array.Empty<byte>(),
               10,
               privateKey);

            var encryptedPrivateKeyPemObject = new PemObject("ENCRYPTED PRIVATE KEY", encryptedPrivateKeyInfo.GetDerEncoded());

            // convert to PEM format
            var csrPem = new StringBuilder();
            var csrPemWriter = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(new StringWriter(csrPem));
            csrPemWriter.WriteObject(encryptedPrivateKeyPemObject);
            csrPemWriter.Writer.Flush();

            return csrPem.ToString();
        }

        public static PayloadCertInfo GenerateCsr(string cryptoAccessKey)
        {
            var randomKey = GenerateRandomBase64String(ApiKeyLength);
            var keyPair = GenerateKeyPair();

            var csrDetails = new Dictionary<DerObjectIdentifier, string>
            {
                { X509Name.CN, randomKey },
                { X509Name.C, "US" },
                { X509Name.ST, "California" },
                { X509Name.L, "San Diego" },
                { X509Name.O, "Ubiq Security, Inc." },
                { X509Name.OU, "Ubiq Platform" },
            };

            var subject = new X509Name(csrDetails.Keys.Reverse().ToList(), csrDetails);

            var csr = new Pkcs10CertificationRequest(
                new Asn1SignatureFactory("SHA256withRSA", keyPair.Private),
                subject,
                keyPair.Public,
                null);

            // convert to PEM format
            var csrPem = new StringBuilder();
            var csrPemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StringWriter(csrPem));
            csrPemWriter.WriteObject(csr);
            csrPemWriter.Writer.Flush();

            return new PayloadCertInfo
            {
                CsrPem = csrPem.ToString(),
                EncryptedPrivateKey = EncryptPrivateKey(keyPair.Private, cryptoAccessKey),
            };
        }

        // reference:
        // https://stackoverflow.com/questions/70526272/c-sharp-how-to-decrypt-an-encrypted-private-key-with-bouncy-castle
        public static byte[] UnwrapDataKey(string encryptedPrivateKey, string wrappedDataKey, string cryptoAccessKey)
        {
            var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(new StringReader(encryptedPrivateKey), new PasswordFinder(cryptoAccessKey));

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
