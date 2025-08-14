using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class DecodeKeyNumberOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            string result = context.CurrentValue.DecodeKeyNumber(context.Dataset.OutputCharacters, context.Dataset.MsbEncodingBits.Value, out int keyNumber);

            context.KeyNumber = keyNumber;

            return Task.FromResult(result);
        }
    }
}
