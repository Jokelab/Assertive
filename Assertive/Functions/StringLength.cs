using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    internal class StringLength : IFunction
    {
        public int ParameterCount => 1;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is not StringValue strValue)
            {
                throw new FunctionExecutionException("Function argument should be string", this);
            }

            if (string.IsNullOrEmpty(strValue.Value))
            {
                return Task.FromResult<Value>(new NumericValue(0));
            }
            return Task.FromResult<Value>(new NumericValue(strValue.Value.Length));
        }
    }
}
