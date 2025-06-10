using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.WebService
{
    // custom httpclient delegatinghandler that generates and adds our ubiq http message signature (digest + signature headers)
    internal class HttpSignatureDelegatingHandler : DelegatingHandler
    {
        private readonly IUbiqCredentials _credentials;

        public HttpSignatureDelegatingHandler(IUbiqCredentials credentials)
            : base(new HttpClientHandler())
        {
            _credentials = credentials;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //return await base.SendAsync(request, cancellationToken);

            if (!request.Headers.Contains(HttpRequestHeader.Date.ToString()))
            {
                request.Headers.Add(HttpRequestHeader.Date.ToString(), DateTime.UtcNow.ToString("r"));
            }

            if (!request.Headers.Contains(HttpRequestHeader.Host.ToString()))
            {
                request.Headers.Add(HttpRequestHeader.Host.ToString(),request.RequestUri.Host);
            }

            // tricky: StringContent does not set Content-Length, so do that explicitly
            // content.Headers.ContentLength = contentLength;
            var digestValue = BuildDigestValue(request.Content, out int contentLength);
            if (request.Headers.Contains("Digest"))
            {
                request.Headers.Remove("Digest");
            }
            request.Headers.Add("Digest", digestValue);

            if (request.Content != null)
            {
                // tricky: StringContent does not set Content-Length, so do that explicitly
                request.Content.Headers.ContentLength = contentLength;
            }

            if (request.Headers.Contains("Signature"))
            {
                request.Headers.Remove("Signature");
            }
            var signature = BuildSignature(request, _credentials);
            request.Headers.Add("Signature", signature);

            return  await base.SendAsync(request, cancellationToken);
        }

        private static string UnixTimeAsString()
        {
            var utcNow = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)utcNow).ToUnixTimeSeconds();

            return unixTime.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildDigestValue(HttpContent httpContent, out int contentLength)
        {
            contentLength = 0;

            using (var contentStream = new MemoryStream())
            {

                httpContent?.CopyToAsync(contentStream).Wait();
                contentStream.Position = 0;     // rewind

                // return content length, in bytes
                contentLength = (int)contentStream.Length;

                // calculate SHA-512 hash
                byte[] hashBytes = null;
                using (var sha512 = SHA512.Create())
                {
                    hashBytes = sha512.ComputeHash(contentStream);
                }

                // return SHA-512 result as Base64 string
                return $"SHA-512={Convert.ToBase64String(hashBytes)}";
            }
        }

        private static string BuildSignature(HttpRequestMessage httpRequestMessage, IUbiqCredentials credentials)
        {
            var unixTimeString = UnixTimeAsString();
            var requestTarget = $"{httpRequestMessage.Method.ToString().ToLowerInvariant()} {httpRequestMessage.RequestUri.PathAndQuery}";
            var secretSigningKeyBytes = Encoding.UTF8.GetBytes(credentials.SecretSigningKey);

            using (var hashStream = new MemoryStream())
            {
                WriteHashableBytes(hashStream, "(created)", unixTimeString);
                WriteHashableBytes(hashStream, "(request-target)", requestTarget);
                if (httpRequestMessage.Content != null)
                {
                    WriteHashableBytes(hashStream, "Content-Length", httpRequestMessage.Content.Headers.GetValues("Content-Length").First());
                    WriteHashableBytes(hashStream, "Content-Type", httpRequestMessage.Content.Headers.GetValues("Content-Type").First());
                }
                WriteHashableBytes(hashStream, "Date", httpRequestMessage.Headers.GetValues("Date").First());
                WriteHashableBytes(hashStream, "Digest", httpRequestMessage.Headers.GetValues("Digest").First());
                WriteHashableBytes(hashStream, "Host", httpRequestMessage.Headers.Host);

                hashStream.Position = 0; // rewind

                using (var hmac = new HMACSHA512(secretSigningKeyBytes))
                {
                    // Compute the hash of the input data
                    var hashBytes = hmac.ComputeHash(hashStream);

                    // assemble final signature string
                    var signature = new StringBuilder();

                    signature.AppendFormat(CultureInfo.InvariantCulture, "keyId=\"{0}\"", credentials.AccessKeyId);
                    signature.Append(", algorithm=\"hmac-sha512\"");
                    signature.AppendFormat(CultureInfo.InvariantCulture, ", created={0}", unixTimeString);
                    if (httpRequestMessage.Content != null)
                    {
                        signature.Append(", headers=\"(created) (request-target) content-length content-type date digest host\"");
                    }
                    else
                    {
                        signature.Append(", headers=\"(created) (request-target) date digest host\"");
                    }

                    signature.AppendFormat(CultureInfo.InvariantCulture, ", signature=\"{0}\"", Convert.ToBase64String(hashBytes));
                    return signature.ToString();
                }
            }
        }

        private static void WriteHashableBytes(Stream stream, string name, string value)
        {
            // build Unicode string
            var hashableString = $"{name.ToLowerInvariant()}: {value}\n";

            // convert to UTF-8 bytes
            var hashableBytes = Encoding.UTF8.GetBytes(hashableString);

            // write bytes to caller-provided Stream
            stream.Write(hashableBytes, 0, hashableBytes.Length);
        }

    }
}
