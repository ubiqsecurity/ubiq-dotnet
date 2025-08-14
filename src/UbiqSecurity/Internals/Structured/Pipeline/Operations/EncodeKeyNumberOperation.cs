using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class EncodeKeyNumberOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var result = context.CurrentValue.EncodeKeyNumber(context.Dataset.OutputCharacters, context.Dataset.MsbEncodingBits.Value, context.KeyNumber.Value);

            return Task.FromResult(result);
        }
    }
}
