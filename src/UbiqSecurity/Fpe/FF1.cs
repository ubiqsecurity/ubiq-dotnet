using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using UbiqSecurity.Fpe.Ciphers;
using UbiqSecurity.Fpe.Helpers;
using ICipher = UbiqSecurity.Fpe.Ciphers.ICipher;

namespace UbiqSecurity.Fpe
{
	/// <summary>
	/// FF1 algortihm for format-preserving encryption
	/// </summary>
	internal class FF1 : FFX, IFFX
	{
		private readonly ICipher _cipher;
		private readonly byte[] _key;

		public FF1(byte[] key, byte[] twk, long twkmin, long twkmax, int radix)
			: base(key, twk, (long)1 << 32, twkmin, twkmax, radix, DefaultAlphabet)
		{
			_key = key;
			_cipher = new SystemCipher();
		}

		public FF1(byte[] key, byte[] twk, long twkmin, long twkmax, int radix, string alphabet)
			: base(key, twk, (long)1 << 32, twkmin, twkmax, radix, alphabet)
		{
			_key = key;
			_cipher = new SystemCipher();
		}

		public FF1(byte[] key, byte[] twk, long twkmin, long twkmax, int radix, string alphabet, ICipher cipher)
			: base(key, twk, (long)1 << 32, twkmin, twkmax, radix, alphabet)
		{
			_key = key;
			_cipher = cipher;
		}

		/// <summary>
		/// The comments below reference the steps of the algorithm described here:
		/// https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-38Gr1-draft.pdf
		/// </summary>
		/// <param name="x"></param>
		/// <param name="tweak"></param>
		/// <param name="encrypt">Boolean to indicate if <paramref name="x"/> should be encrypted (true) or decrypted (false)</param>
		/// <returns></returns>
		[SuppressMessage("Style", "IDE0047:Remove unnecessary parentheses", Justification = "Using extra parens for clarity in large formulas")]
		public override string Cipher(string x, byte[] tweak, bool encrypt)
		{
			// Step 1
			int n = x.Length;
			int u = n / 2;
			int v = n - u;

			// Step 3, 4
			int b = ((int)Math.Ceiling((Math.Log(_radix) / Math.Log(2)) * v) + 7) / 8;
			int d = (4 * ((b + 3) / 4)) + 4;

			int p = 16;
			int r = ((d + 15) / 16) * 16;

			string A;
			string B;
			byte[] PQ;
			byte[] R;
			int q;

			// use default tweak if none is supplied
			if (tweak == null)
			{
				tweak = _twk;
			}

			// check text tweak if none is supplied
			if (n < _txtmin || n > _txtmax)
			{
				throw new ArgumentException(UbiqResources.InvalidInputLength);
			}
			else if (tweak.Length < _twkmin || (_twkmax > 0 && tweak.Length > _twkmax))
			{
				throw new ArgumentException(UbiqResources.InvalidTweakLength);
			}

			// the number of bytes in q
			q = ((tweak.Length + b + 1 + 15) / 16) * 16;

			// P and Q need to be adjacent in memory for the 
			// purposes of encryption
			PQ = new byte[p + q];
			R = new byte[r];

			// Step 2
			if (encrypt)
			{
				A = x.Substring(0, u);
				B = x.Substring(u);
			}
			else
			{
				B = x.Substring(0, u);
				A = x.Substring(u);
			}

			// Step 5
			PQ[0] = 1;
			PQ[1] = 2;
			PQ[2] = 1;
			PQ[3] = (byte)(_radix >> 16);
			PQ[4] = (byte)(_radix >> 8);
			PQ[5] = (byte)(_radix >> 0);
			PQ[6] = 10;
			PQ[7] = (byte)u;
			PQ[8] = (byte)(n >> 24);
			PQ[9] = (byte)(n >> 16);
			PQ[10] = (byte)(n >> 8);
			PQ[11] = (byte)(n >> 0);
			PQ[12] = (byte)(tweak.Length >> 24);
			PQ[13] = (byte)(tweak.Length >> 16);
			PQ[14] = (byte)(tweak.Length >> 8);
			PQ[15] = (byte)(tweak.Length >> 0);

			// Step 6i, the static parts
			Array.Copy(tweak, 0, PQ, p, tweak.Length);
			// remainder of q already initialized to 0

			for (var i = 0; i < 10; i++)
			{
				// Step 6v
				var m = (((i + (encrypt ? 1 : 0)) % 2) == 1) ? u : v;

				BigInteger c;
				BigInteger y;
				byte[] numb;

				// Step 6i, the non-static parts
				PQ[PQ.Length - b - 1] = (byte)(encrypt ? i : (9 - i));

				// convert the numeral string B to an integer and
				// export that integer as a byte array in to q
				c = BigIntegerHelper.Parse(B, _radix, _alphabet);

				// note: ToByteArray() returns bytes in little endian order
				// and needs to be reversed into big-endian order
				numb = c.ToByteArray();
				Array.Reverse(numb);

				if (numb[0] == 0 && numb.Length > 1)
				{
					/*
					 * Per the Java documentation, BigInteger.toByteArray always
					 * returns enough bytes to contain a sign bit. For the purposes
					 * of this function all numbers are unsigned; however, when the
					 * most-significant bit is set in a number, the Java library
					 * returns an extra most-significant byte that is set to 0.
					 * That byte must be removed for the cipher to work correctly.
					 */
					numb = numb.Skip(1).ToArray();
				}

				if (b <= numb.Length)
				{
					Array.Copy(numb, 0, PQ, PQ.Length - b, b);
				}
				else
				{
					// left pad w/ 0s
					ArrayHelper.Fill(PQ, (byte)0, PQ.Length - b, PQ.Length - numb.Length);
					Array.Copy(numb, 0, PQ, PQ.Length - numb.Length, numb.Length);
				}

				// Step 6ii
				//Prf(R, 0, PQ, 0, PQ.Length);
				_cipher.Prf(_key, PQ, R);

				// Step 6iii
				// if r is greater than 16, fill the subsequent blocks
				// with the result of ciph(R ^ 1), ciph(R ^ 2), ...
				for (var j = 1; j < r / 16; j++)
				{
					int l = j * 16;

					ArrayHelper.Fill(R, (byte)0, l, l + 12);
					R[l + 12] = (byte)(j >> 24);
					R[l + 13] = (byte)(j >> 16);
					R[l + 14] = (byte)(j >> 8);
					R[l + 15] = (byte)(j >> 0);

					Xor(R, l, R, 0, R, l, 16);

					_cipher.Ciph(_key, R, l, R, l);
				}

				// Step 6vi
				// calculate A +/- y mod radix**m
				// where y is the number formed by the first d bytes of R
				var yA = new byte[d];
				Array.Copy(R, yA, d);

				// reverse array, back into little-endian order
				Array.Reverse(yA);

				y = new BigInteger(yA);
				y = BigIntegerHelper.Mod(y, BigInteger.One << (8 * d));

				c = BigIntegerHelper.Parse(A, _radix, _alphabet);

				if (encrypt)
				{
					c += y;
				}
				else
				{
					c -= y;
				}

				c = BigIntegerHelper.Mod(c, BigInteger.Pow(new BigInteger(_radix), m));

				// Step 6viii
				A = B;

				// Step 6ix
				B = c.ToString(m, _radix, _alphabet);
			}

			// Step 7
			return encrypt ? (A + B) : (B + A);
		}
	}
}
