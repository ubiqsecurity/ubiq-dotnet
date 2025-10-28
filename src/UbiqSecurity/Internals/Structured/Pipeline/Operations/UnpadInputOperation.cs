using System.Linq;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class UnpadInputOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var dataset = context.Dataset;

            if (string.IsNullOrEmpty(dataset.InputPadCharacter) || dataset.InputPadCharacter.Length > 1)
            {
                return Task.FromResult(context.CurrentValue);
            }

            string val = context.CurrentValue.TrimStart(dataset.InputPadCharacter.First());

            // also unpad passthrough mask
            // code smell that one operation has to know about another
            if (context.Data.ContainsKey("PassthroughTemplate"))
            {
                context.Data["PassthroughTemplate"] = context.Data["PassthroughTemplate"].TrimStart(dataset.InputPadCharacter.First());
            }

            return Task.FromResult(val);
        }
    }
}
