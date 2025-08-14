using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline
{
    internal interface IOperation
    {
        Task<string> InvokeAsync(OperationContext context);
    }
}
