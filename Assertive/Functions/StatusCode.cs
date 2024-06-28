using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    /// <summary>
    /// Returns the numeric statuscode of the HTTP response
    /// </summary>
    internal class StatusCode : IFunction
    {
        public int ParameterCount => 1;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is not HttpRequestValue httpRequestValue)
            {
                throw new FunctionExecutionException("First function argument should be a http request", this);
            }

            return Task.FromResult<Value>(new NumericValue((int)httpRequestValue.GetRequest().Response!.StatusCode));
        }
    }
}
