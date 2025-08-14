using System.Collections.Generic;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline
{
    internal class StructuredPipeline : IOperation
    {
        public StructuredPipeline()
        {
        }

        public StructuredPipeline(IEnumerable<IOperation> operations)
        {
            Operations = operations;
        }

        protected IEnumerable<IOperation> Operations { get; set; } = new List<IOperation>();

        public async Task<string> InvokeAsync(OperationContext context)
        {
            foreach (var operation in Operations)
            {
                context.CurrentValue = await operation.InvokeAsync(context);
            }

            return context.CurrentValue;
        }
    }
}
