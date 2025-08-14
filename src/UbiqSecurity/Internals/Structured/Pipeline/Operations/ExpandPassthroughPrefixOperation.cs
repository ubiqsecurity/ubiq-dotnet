using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class ExpandPassthroughPrefixOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            if (context.Dataset.PassthroughPrefixLength == 0 || !context.Data.ContainsKey("Prefix"))
            {
                return Task.FromResult(context.CurrentValue);
            }

            var result = string.Concat(context.Data["Prefix"], context.CurrentValue);

            return Task.FromResult(result);
        }
    }
}
