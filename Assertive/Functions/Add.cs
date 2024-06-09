using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    /// <summary>
    /// Add a key value pair to a dictionary
    /// </summary>
    internal class Add : IFunction
    {
        public int ParameterCount => 3;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is DictionaryValue dic)
            {
                dic.AddEntry(new DictionaryEntry() { Key = values[1], Value = values[2] });
            }
            else
            {
                throw new FunctionExecutionException("The first argument should be a dictionary", this);
            }

            return Task.FromResult<Value>(dic);
        }
    }
}
