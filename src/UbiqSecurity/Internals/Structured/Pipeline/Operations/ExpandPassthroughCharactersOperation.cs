using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class ExpandPassthroughCharactersOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            if (string.IsNullOrEmpty(context.Dataset.PassthroughCharacterSet) || !context.Data.ContainsKey("PassthroughTemplate"))
            {
                return Task.FromResult(context.CurrentValue);
            }

            var result = context.CurrentValue.FormatToTemplate(context.Data["PassthroughTemplate"], context.Dataset.PassthroughCharacterSet);

            return Task.FromResult(result);
        }
    }
}
