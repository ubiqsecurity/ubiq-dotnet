using System;
using System.Text;
using System.Threading.Tasks;

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

            if (dataset.InputEncoding != "base64")
            {
                throw new NotImplementedException("Encoding not supported");
            }

            var bytes = Convert.FromBase64String(context.CurrentValue);

            var val = Encoding.UTF8.GetString(bytes);

            return Task.FromResult(val);
        }
    }
}
