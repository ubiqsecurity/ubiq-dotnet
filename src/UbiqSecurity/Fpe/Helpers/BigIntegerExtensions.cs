using System;
using System.Numerics;
using System.Text;

namespace UbiqSecurity.Fpe.Helpers
{
	internal static class BigIntegerExtensions
	{
		/// <summary>
		/// Convert BigInteger to a string using a custom alphabet
		/// </summary>
		public static string ToString(this BigInteger number, int desiredStringLength, int radix, string alphabet)
		{
			var sb = new StringBuilder();
			var bigRadix = new BigInteger(radix);
			BigInteger cvt = number;

			while (cvt > BigInteger.Zero)
			{
				var remainder = (int)(cvt % bigRadix);
				sb.Insert(0, alphabet[remainder]);
				cvt /= bigRadix;
			}

			if (sb.Length > desiredStringLength)
			{
				throw new ArgumentException($"Unable to convert biginteger into {desiredStringLength} characters");
			}

			// pad string w/ leading "zero characters" to reach desired size
			if (sb.Length < desiredStringLength)
			{
				sb.Insert(0, alphabet[0].ToString(), desiredStringLength - sb.Length);
			}

			return sb.ToString();
		}

		public static string ToString(this BigInteger number, int desiredStringLength, string alphabet)
		{
			return ToString(number, desiredStringLength, alphabet.Length, alphabet);
		}
	}
}
