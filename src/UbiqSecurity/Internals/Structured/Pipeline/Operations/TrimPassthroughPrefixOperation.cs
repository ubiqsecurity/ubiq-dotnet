using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class TrimPassthroughPrefixOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var prefixLength = context.Dataset.PassthroughPrefixLength;

            if (prefixLength == 0)
            {
                return Task.FromResult(context.CurrentValue);
            }

            context.Data.Add("Prefix", context.CurrentValue.Substring(0, prefixLength));

            return Task.FromResult(context.CurrentValue.Substring(prefixLength));
        }
    }
}
