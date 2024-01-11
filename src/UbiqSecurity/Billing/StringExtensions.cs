using System;
using Newtonsoft.Json.Linq;

namespace UbiqSecurity.Billing
{
	internal static class StringExtensions
	{
		public static bool TryParseJson(this string source, out JToken obj)
		{
			obj = null;

			if (string.IsNullOrWhiteSpace(source))
			{
				return false;
			}

			source = source.Trim();

			if (!(source.StartsWith("{", StringComparison.InvariantCulture) && source.EndsWith("}", StringComparison.InvariantCulture))
				&& !(source.StartsWith("[", StringComparison.InvariantCulture) && source.EndsWith("]", StringComparison.InvariantCulture)))
			{
				return false;
			}

			try
			{
				obj = JToken.Parse(source);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
