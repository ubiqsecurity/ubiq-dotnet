using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class TrimPassthroughCharactersOperation : IOperation
    {
        public Task<string> InvokeAsync(OperationContext context)
        {
            var passthroughCharacterSet = context.Dataset.PassthroughCharacterSet;

            if (string.IsNullOrEmpty(passthroughCharacterSet))
            {
                return Task.FromResult(context.CurrentValue);
            }

            var templateChar = (context.IsEncrypt ? context.Dataset.OutputCharacters : context.Dataset.InputCharacters).Substring(0, 1);
            var templateBuilder = new StringBuilder();
            var trimmedBuilder = new StringBuilder();

            foreach (var c in context.CurrentValue.ToCharArray())
            {
                if (passthroughCharacterSet.Contains(c))
                {
                    templateBuilder.Append(c);
                }
                else
                {
                    trimmedBuilder.Append(c);
                    templateBuilder.Append(templateChar);
                }
            }

            context.Data.Add("PassthroughTemplate", templateBuilder.ToString());

            return Task.FromResult(trimmedBuilder.ToString());
        }
    }
}
