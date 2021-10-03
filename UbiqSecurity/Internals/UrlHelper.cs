using System.Net;

namespace UbiqSecurity.Internals
{
	internal static class UrlHelper
	{
		internal static string GenerateAccessKeyUrl(IUbiqCredentials credentials)
		{
			return WebUtility.UrlEncode(credentials.AccessKeyId);
		}
		
		internal static string GenerateFfsUrl(string ffsName, IUbiqCredentials credentials)
		{
			return $"papi={WebUtility.UrlEncode(credentials.AccessKeyId)}&ffs_name={WebUtility.UrlEncode(ffsName)}";
		}

		internal static string GenerateFpeUrl(string ffsName, int? keyNumber, IUbiqCredentials credentials)
		{
			return $"papi={WebUtility.UrlEncode(credentials.AccessKeyId)}&ffs_name={WebUtility.UrlEncode(ffsName)}&keyNumber={(keyNumber.HasValue ? keyNumber.Value.ToString() : string.Empty)}";
		}
	}
}
