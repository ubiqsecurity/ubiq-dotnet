using System.Linq;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class PadInputOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var dataset = context.Dataset;

            if (!dataset.InputPadLength.HasValue || string.IsNullOrEmpty(dataset.InputPadCharacter))
            {
                return Task.FromResult(context.CurrentValue);
            }

            string val = context.CurrentValue.PadLeft(dataset.InputPadLength.Value, dataset.InputPadCharacter.First());

            // also pad passthrough mask
            // code smell that one operation has to know about another
            if (context.Data.ContainsKey("PassthroughTemplate"))
            {
                context.Data["PassthroughTemplate"] = context.Data["PassthroughTemplate"].PadLeft(dataset.InputPadLength.Value, dataset.InputPadCharacter.First());
            }

            return Task.FromResult(val);
        }
    }
}
