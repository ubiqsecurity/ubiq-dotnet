using System;
using System.Linq;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class PadInputOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var dataset = context.Dataset;

            if (string.IsNullOrEmpty(dataset.InputPadCharacter))
            {
                return Task.FromResult(context.CurrentValue);
            }

            if (dataset.InputPadCharacter.Length > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "context.Dataset.InputPadCharacter must be 1 character long");
            }

            if (context.CurrentValue.Contains(dataset.InputPadCharacter))
            {
                throw new ArgumentException(nameof(context.CurrentValue), $"Input may not contain input_pad_character '{dataset.InputPadCharacter}'");
            }

            string val = context.CurrentValue.PadLeft(dataset.MinInputLength, dataset.InputPadCharacter.First());

            // also pad the passthrough mask
            // code smell that one operation has to know about another
            if (context.Data.ContainsKey("PassthroughTemplate"))
            {
                context.Data["PassthroughTemplate"] = context.Data["PassthroughTemplate"].PadLeft(dataset.MinInputLength, dataset.InputPadCharacter.First());
            }

            return Task.FromResult(val);
        }
    }
}
