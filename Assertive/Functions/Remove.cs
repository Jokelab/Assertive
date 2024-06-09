using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    public class Remove : IFunction
    {
        public int ParameterCount => 2;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is DictionaryValue dic)
            {
                var key = values[1];
                dic.RemoveEntry(key);
            }
            else
            {
                throw new FunctionExecutionException("The first argument should be a dictionary", this);
            }

            return Task.FromResult<Value>(dic);
        }
    }
}
