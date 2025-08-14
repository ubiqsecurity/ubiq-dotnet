using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace UbiqSecurity.Internals.Unstructured
{
    internal class AesGcmBlockCipher
    {
        private readonly GcmBlockCipher _gcmBlockCipher;

        internal AesGcmBlockCipher(
            bool forEncryption,
            AlgorithmInfo algorithmInfo,
            byte[] key,
            byte[] initVector,
            byte[] additionalBytes = null)
        {
            if (key.Length != algorithmInfo.KeyLength)
            {
                throw new ArgumentException("key length mismatch", nameof(key));
            }
            else if (initVector.Length != algorithmInfo.InitVectorLength)
            {
                throw new ArgumentException("init vector length mismatch", nameof(initVector));
            }

            _gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var aeadParameters = new AeadParameters(
                key: new KeyParameter(key),
                macSize: algorithmInfo.MacLength * 8,       // size, in bits
                nonce: initVector,
                associatedText: additionalBytes);

            _gcmBlockCipher.Init(forEncryption, aeadParameters);
        }

        internal byte[] Update(byte[] inBytes, int inOffset, int inCount)
        {
            var outBytes = new byte[_gcmBlockCipher.GetOutputSize(inBytes.Length)];
            var length = _gcmBlockCipher.ProcessBytes(inBytes, inOffset, inCount, outBytes, 0);
            if (length < outBytes.Length)
            {
                var shortenedOutBytes = new byte[length];
                Array.Copy(outBytes, shortenedOutBytes, shortenedOutBytes.Length);
                return shortenedOutBytes;
            }
            else
            {
                return outBytes;
            }
        }

        internal byte[] Finalize()
        {
            var finalBytes = new byte[32];      // large enough for MAC result
            var retLen = _gcmBlockCipher.DoFinal(finalBytes, 0);
            if (retLen < finalBytes.Length)
            {
                var shortenedFinalBytes = new byte[retLen];
                Array.Copy(finalBytes, shortenedFinalBytes, shortenedFinalBytes.Length);
                return shortenedFinalBytes;
            }
            else
            {
                return finalBytes;
            }
        }
    }
}
