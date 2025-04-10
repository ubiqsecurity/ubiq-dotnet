using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace UbiqSecurity.Internals.WebService
{
    internal static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            if (nvc == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (string key in nvc.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                string[] values = nvc.GetValues(key);
                if (values == null)
                {
                    continue;
                }

                foreach (string value in values)
                {
                    sb.Append(sb.Length == 0 ? string.Empty : "&");
                    sb.Append($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}");
                }
            }

            return sb.ToString();
        }
    }
}
