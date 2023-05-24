using System;
using System.Text;
using UbiqSecurity.Fpe.Helpers;

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
	}
}
