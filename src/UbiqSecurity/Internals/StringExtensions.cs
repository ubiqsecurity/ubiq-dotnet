using System;
using System.Collections.Generic;
using System.Text;
using UbiqSecurity.Internals.Structured.Helpers;

namespace UbiqSecurity.Internals
{
    internal static class StringExtensions
    {
        internal static string ReplaceCharacter(this string str, char replacement, int position)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (position < 0 || position >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Invalid string position {position}");
            }

            StringBuilder builder = new StringBuilder(str);

            builder[position] = replacement;

            return builder.ToString();
        }

        internal static string ConvertRadix(this string originalValue, string inputCharacters, string outputCharacters)
        {
            var number = BigIntegerHelper.Parse(originalValue, inputCharacters);

            var output = number.ToString(originalValue.Length, outputCharacters);

            return output;
        }

        internal static string EncodeKeyNumber(this string str, string alphabet, long msbEncodingBits, int keyNumber)
        {
            var charBuf = str[0];

            var ctValue = alphabet.IndexOf(charBuf);

            ctValue += keyNumber << (int)msbEncodingBits;

            var ch = alphabet[ctValue];

            return str.ReplaceCharacter(ch, 0);
        }

        internal static string DecodeKeyNumber(this string str, string alphabet, long msbEncodingBits, out int keyNumber)
        {
            char charBuf = str[0];

            int encodedValue = alphabet.IndexOf(charBuf);

            keyNumber = encodedValue >> (int)msbEncodingBits;

            char ch = alphabet[encodedValue - (keyNumber << (int)msbEncodingBits)];

            return str.ReplaceCharacter(ch, 0);
        }

        internal static string TrimEnd(this string input, int characters)
        {
            return input.Substring(0, input.Length - characters);
        }

        internal static string TrimStart(this string input, int characters)
        {
            return input.Substring(characters);
        }

        internal static string FormatToTemplate(this string input, string template, string passthroughCharacters)
        {
            char[] templateCharacters = template.ToCharArray();
            CharEnumerator inputEnumerator = input.GetEnumerator();
            HashSet<char> passthroughCharacterSet = new HashSet<char>(passthroughCharacters.ToCharArray());

            for (int i = 0; i < templateCharacters.Length; i++)
            {
                var ch = templateCharacters[i];
                if (passthroughCharacterSet.Contains(ch))
                {
                    continue;
                }

                var hasNext = inputEnumerator.MoveNext();
                if (!hasNext)
                {
                    throw new ArgumentException("Input length does not match template");
                }

                templateCharacters[i] = inputEnumerator.Current;
            }

            if (inputEnumerator.MoveNext())
            {
                throw new ArgumentException("Input length does not match template");
            }

            return new string(templateCharacters);
        }
    }
}
