using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class TrimPassthroughSuffixOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var suffixLength = context.Dataset.PassthroughSuffixLength;

            if (suffixLength == 0)
            {
                return Task.FromResult(context.CurrentValue);
            }

            context.Data.Add("Suffix", context.CurrentValue.Substring(context.CurrentValue.Length - suffixLength));

            var result = context.CurrentValue.Substring(0, context.CurrentValue.Length - suffixLength);

            return Task.FromResult(result);
        }
    }
}
