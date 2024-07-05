using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    internal class Count : IFunction
    {
        public int ParameterCount => 1;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is DictionaryValue dic)
            {
                return Task.FromResult<Value>(new NumericValue(dic.GetEntries().Count));
            }
            else if (values[0] is ListValue list)
            {
                return Task.FromResult<Value>(new NumericValue(list.ListValues.Count));
            }
            else
            {
                throw new FunctionExecutionException("The first argument should be a dictionary or list", this);
            }
        }
    }
}
