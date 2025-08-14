using System;

namespace UbiqSecurity.Internals.Structured
{
    internal abstract class FFX
    {
        public const string DefaultAlphabet = NumbersAndLowercaseAlphabet;
        public const string NumbersAlphabet = "0123456789";
        public const string NumbersAndLowercaseAlphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
        public const string NumbersAndLowercaseAndUppercaseAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        protected FFX(byte[] key, byte[] tweak, long txtmax, long twkmin, long twkmax, int radix, string alphabet)
        {
            // all 3 key sizes of AES are supported
            switch (key.Length)
            {
                case 16:
                case 24:
                case 32:
                    break;
                default:
                    throw new ArgumentException(UbiqResources.KeySize, nameof(key));
            }

            // FF1 and FF3-1 support a radix up to 65536, but the
            // implementation becomes increasingly difficult and
            // less useful in practice after the limits below.
            if (radix < 2 || radix > alphabet.Length)
            {
                throw new ArgumentException(UbiqResources.InvalidRadix, nameof(radix));
            }

            // for both ff1 and ff3-1: radix**minlen >= 1000000
            //
            // therefore:
            // minlen = ceil(log_radix(1000000))
            //        = ceil(log_10(1000000) / log_10(radix))
            //        = ceil(6 / log_10(radix))
            long txtmin = (int)Math.Ceiling(6 / Math.Log10(radix));
            if (txtmin < 2 || txtmin > txtmax)
            {
                throw new ArgumentException(UbiqResources.MinTextLengthRange);
            }

            // the default tweak must be specified
            if (tweak == null)
            {
                throw new ArgumentNullException(nameof(tweak), UbiqResources.InvalidTweak);
            }

            // check tweak lengths
            if (twkmin > twkmax || tweak.Length < twkmin || (twkmax > 0 && tweak.Length > twkmax))
            {
                throw new ArgumentException(UbiqResources.InvalidTweakLength);
            }

            Radix = radix;
            Alphabet = alphabet;
            MinimumInputLength = txtmin;
            MaximumInputLength = txtmax;
            MinimumTweakLength = twkmin;
            MaximumTweakLength = twkmax;

            Tweak = new byte[tweak.Length];

            Array.Copy(tweak, Tweak, tweak.Length);
        }

        protected long MinimumInputLength { get; private set; }

        protected long MaximumInputLength { get; private set; }

        protected long MinimumTweakLength { get; private set; }

        protected long MaximumTweakLength { get; private set; }

        protected byte[] Tweak { get; private set; }

        protected int Radix { get; private set; }

        protected string Alphabet { get; private set; } = DefaultAlphabet;

        // Perform an exclusive-or of the corresponding bytes in two byte arrays
        public static void Xor(byte[] d, int doff, byte[] s1, int s1off, byte[] s2, int s2off, int len)
        {
            for (int i = 0; i < len; i++)
            {
                d[doff + i] = (byte)(s1[s1off + i] ^ s2[s2off + i]);
            }
        }

        public abstract string Cipher(string x, byte[] tweak, bool encrypt);

        /// <summary>
        /// Encrypt a string, returning a cipher text using the same alphabet.
        /// The key, tweak parameters, and radix were all already set
        /// by the initialization of the FF3_1 object.
        /// </summary>
        /// <param name="plainText">the plain text to be encrypted</param>
        /// <param name="tweak">the tweak used to perturb the encryption</param>
        /// <returns>the encryption of the plain text, the cipher text</returns>
        public string Encrypt(string plainText, byte[] tweak)
        {
            return Cipher(plainText, tweak, true);
        }

        /// <summary>
        /// Encrypt a string, returning a cipher text using the same alphabet.
        /// The key, tweak parameters, and radix were all already set
        /// by the initialization of the FF3_1 object.
        /// </summary>
        /// <param name="plainText">The plain text to be encrypted</param>
        /// <returns>the encryption of the plain text, the cipher text</returns>
        public string Encrypt(string plainText)
        {
            return Encrypt(plainText, null);
        }

        /// <summary>
        /// Decrypt a string, returning the plain text.
        /// The key, tweak parameters, and radix were all already set
        /// by the initialization of the FF3_1 object.
        /// </summary>
        /// <param name="cipherText">the cipher text to be decrypted</param>
        /// <param name="tweak">the tweak used to perturb the encryption</param>
        /// <returns>the decryption of the cipher text, the plain text</returns>
        public string Decrypt(string cipherText, byte[] tweak)
        {
            return Cipher(cipherText, tweak, false);
        }

        /// <summary>
        /// Decrypt a string, returning the plain text.
        /// The key, tweak parameters, and radix were all already set
        /// by the initialization of the FF3_1 object.
        /// </summary>
        /// <param name="cipherText">the cipher text to be decrypted</param>
        /// <returns>the decryption of the cipher text, the plain text</returns>
        public string Decrypt(string cipherText)
        {
            return Decrypt(cipherText, null);
        }
    }
}
