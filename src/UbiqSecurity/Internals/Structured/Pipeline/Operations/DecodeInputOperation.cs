using System;
using System.Text;
using System.Threading.Tasks;
using Wiry.Base32;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class DecodeInputOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var dataset = context.Dataset;

            if (string.IsNullOrEmpty(dataset.InputEncoding))
            {
                return Task.FromResult(context.CurrentValue);
            }

            byte[] bytes;

            switch (dataset.InputEncoding)
            {
                case "base32":
                    bytes = Base32Encoding.Standard.ToBytes(context.CurrentValue);
                    break;
                case "base64":
                    bytes = Convert.FromBase64String(context.CurrentValue);
                    break;
                default:
                    throw new NotImplementedException("Encoding not supported");
            }

            var val = Encoding.UTF8.GetString(bytes);

            return Task.FromResult(val);
        }
    }
}
