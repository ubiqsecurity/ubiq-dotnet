using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class ExpandPassthroughSuffixOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            if (!context.Data.ContainsKey("Suffix"))
            {
                return Task.FromResult(context.CurrentValue);
            }

            var result = string.Concat(context.CurrentValue, context.Data["Suffix"]);

            return Task.FromResult(result);
        }
    }
}
