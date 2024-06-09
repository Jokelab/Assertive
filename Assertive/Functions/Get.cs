using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    internal class Get : IFunction
    {
        public int ParameterCount => 2;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is DictionaryValue dic)
            {
                return Task.FromResult(dic.GetEntry(values[1]));
            }
            else
            {
                throw new FunctionExecutionException("The first argument should be a dictionary", this);
            }
        }
    }
}
