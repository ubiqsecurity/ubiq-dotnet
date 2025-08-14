using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class ConvertRadixOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            if (context.IsEncrypt)
            {
                return Task.FromResult(context.CurrentValue.ConvertRadix(context.Dataset.InputCharacters, context.Dataset.OutputCharacters));
            }

            return Task.FromResult(context.CurrentValue.ConvertRadix(context.Dataset.OutputCharacters, context.Dataset.InputCharacters));
        }
    }
}
