using System;
using System.Text;
using System.Threading.Tasks;
using Wiry.Base32;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class EncodeInputOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var dataset = context.Dataset;

            if (string.IsNullOrEmpty(dataset.InputEncoding))
            {
                return Task.FromResult(context.CurrentValue);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(context.CurrentValue);
            string val;

            switch (dataset.InputEncoding)
            {
                case "base32":
                    val = Base32Encoding.Standard.GetString(bytes);
                    break;
                case "base64":
                    val = Convert.ToBase64String(bytes);
                    break;
                default:
                    throw new NotImplementedException("Encoding not supported");
            }

            return Task.FromResult(val);
        }
    }
}
