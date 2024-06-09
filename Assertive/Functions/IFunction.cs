using Assertive.Types;

namespace Assertive.Functions
{
    public interface IFunction
    {
        int ParameterCount { get; }
        Task<Value> Execute(List<Value> values, FunctionContext context);
    }
}