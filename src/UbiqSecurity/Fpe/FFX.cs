using System;

namespace UbiqSecurity.Fpe
{
	abstract class FFX
	{
		public const string DefaultAlphabet = NumbersAndLowercaseAlphabet;
		public const string NumbersAlphabet = "0123456789";
		public const string NumbersAndLowercaseAlphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
		public const string NumbersAndLowercaseAndUppercaseAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		protected readonly long _txtmin, _txtmax;
		protected readonly long _twkmin, _twkmax;
		protected readonly byte[] _twk;
		protected readonly int _radix;
		protected readonly string _alphabet = DefaultAlphabet;

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

			_radix = radix;
			_alphabet = alphabet;
			_txtmin = txtmin;
			_txtmax = txtmax;
			_twkmin = twkmin;
			_twkmax = twkmax;

			_twk = new byte[tweak.Length];

			Array.Copy(tweak, _twk, tweak.Length);
		}

		public abstract string Cipher(string x, byte[] tweak, bool encrypt);

		/// <summary>
		/// Encrypt a string, returning a cipher text using the same alphabet.
		/// The key, tweak parameters, and radix were all already set
		/// by the initialization of the FF3_1 object.
		/// </summary>
		/// <param name="X">the plain text to be encrypted</param>
		/// <param name="twk">the tweak used to perturb the encryption</param>
		/// <returns>the encryption of the plain text, the cipher text</returns>
		public string Encrypt(string X, byte[] twk)
		{
			return Cipher(X, twk, true);
		}

		/// <summary>
		/// Encrypt a string, returning a cipher text using the same alphabet.
		/// The key, tweak parameters, and radix were all already set
		/// by the initialization of the FF3_1 object.
		/// </summary>
		/// <param name="X">The plain text to be encrypted</param>
		/// <returns>the encryption of the plain text, the cipher text</returns>
		public string Encrypt(string X)
		{
			return Encrypt(X, null);
		}

		/// <summary>
		/// Decrypt a string, returning the plain text.
		/// The key, tweak parameters, and radix were all already set
		/// by the initialization of the FF3_1 object.
		/// </summary>
		/// <param name="X">the cipher text to be decrypted</param>
		/// <param name="twk">the tweak used to perturb the encryption</param>
		/// <returns>the decryption of the cipher text, the plain text</returns>
		public string Decrypt(string X, byte[] twk)
		{
			return Cipher(X, twk, false);
		}

		/// <summary>
		/// Decrypt a string, returning the plain text.
		/// The key, tweak parameters, and radix were all already set
		/// by the initialization of the FF3_1 object.
		/// </summary>
		/// <param name="X">the cipher text to be decrypted</param>
		/// <returns>the decryption of the cipher text, the plain text</returns>
		public string Decrypt(string X)
		{
			return Decrypt(X, null);
		}

		/// <summary>
		/// Perform an exclusive-or of the corresponding bytes
		/// in two byte arrays
		/// </summary>
		public static void Xor(byte[] d, int doff,
							   byte[] s1, int s1off,
							   byte[] s2, int s2off,
							   int len)
		{
			for (int i = 0; i < len; i++)
			{
				d[doff + i] = (byte)(s1[s1off + i] ^ s2[s2off + i]);
			}
		}
	}
}
