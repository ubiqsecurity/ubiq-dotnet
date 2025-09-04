using System;
using System.Linq;

namespace UbiqSecurity.Internals.Structured.Helpers
{
    internal static class IntegerHelper
    {
        public static readonly char[] AlphabetArray = Alphabet.ToCharArray();

        private const string Alphabet = "0123456789ABCDEF";

        // bases that dotnet can natively convert between
        private static readonly int[] NativeSupportedBases = new[] { 2, 8, 10, 16 };
        private static readonly int[] NonNativeSupportedBases = new[] { 11, 12, 13, 14, 15 };

        public static long Parse(string value, int fromBase)
        {
            if (NativeSupportedBases.Contains(fromBase))
            {
                return Convert.ToInt64(value, fromBase);
            }

            if (!NonNativeSupportedBases.Contains(fromBase))
            {
                throw new NotImplementedException($"Base {fromBase} not implemented");
            }

            char[] chars = value.ToCharArray();
            bool isNegative = chars[0] == '-';
            int m = chars.Length - (isNegative ? 2 : 1);
            int x;
            long result = 0;

            for (int i = isNegative ? 1 : 0; i < chars.Length; i++)
            {
                x = Alphabet.IndexOf(chars[i]);
                result += x * (long)Math.Pow(fromBase, m--);
            }

            return isNegative ? -result : result;
        }

        public static string ToString(long value, int toBase)
        {
            if (NativeSupportedBases.Contains(toBase))
            {
                return Convert.ToString(value, toBase);
            }

            if (!NonNativeSupportedBases.Contains(toBase))
            {
                throw new NotImplementedException($"Base {toBase} not implemented");
            }

            char[] buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value + 1, toBase)), 1)];

            var i = buffer.Length;
            do
            {
                buffer[--i] = AlphabetArray[value % toBase];
                value /= toBase;
            }
            while (value > 0);

            return new string(buffer, i, buffer.Length - i);
        }
    }
}
