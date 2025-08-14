using System.Numerics;

namespace UbiqSecurity.Internals.Structured.Helpers
{
    internal static class BigIntegerHelper
    {
        public static BigInteger Mod(BigInteger value, BigInteger m)
        {
            BigInteger biggie = value % m;
            return biggie.Sign >= 0 ? biggie : biggie + m;
        }

        public static BigInteger Parse(string numberString, string alphabet)
        {
            return Parse(numberString, alphabet.Length, alphabet);
        }

        public static BigInteger Parse(string numberString, int radix, string alphabet)
        {
            var number = BigInteger.Zero;
            var digit = BigInteger.One;
            var bigRadix = new BigInteger(radix);

            for (var i = numberString.Length - 1; i >= 0; i--)
            {
                var character = numberString[i];
                var alphabetIndex = alphabet.IndexOf(character);
                number += digit * new BigInteger(alphabetIndex);
                digit *= bigRadix;
            }

            return number;
        }
    }
}
