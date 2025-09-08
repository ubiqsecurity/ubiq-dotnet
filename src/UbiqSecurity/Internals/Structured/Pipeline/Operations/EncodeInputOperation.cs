using System;
using System.Text;
using System.Threading.Tasks;

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

            if (dataset.InputEncoding != "base64")
            {
                throw new NotImplementedException("Encoding not supported");
            }

            var bytes = Encoding.UTF8.GetBytes(context.CurrentValue);

            var val = Convert.ToBase64String(bytes);

            return Task.FromResult(val);
        }
    }
}
