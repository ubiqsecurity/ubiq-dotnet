using System;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class NullDecryptOperation : IOperation
    {
        public async Task<string> InvokeAsync(OperationContext context)
        {
            if (context.IsEncrypt)
            {
                throw new InvalidOperationException("DecryptOperation not allowed in a encryption pipeline");
            }

            if (context.CurrentValue.Length < context.Dataset.MinInputLength)
            {
                throw new ArgumentException("Input length is less than the dataset's minimum input length.");
            }

            if (context.CurrentValue.Length > context.Dataset.MaxInputLength)
            {
                throw new ArgumentException("Input length is greater than the dataset's maximum input length.");
            }

            await context.FfxCache.GetAsync(context.Dataset, context.KeyNumber);

            return context.CurrentValue;
        }
    }
}
