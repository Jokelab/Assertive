using Assertive.Exceptions;
using Assertive.Types;
using System.Text;

namespace Assertive.Functions
{
    internal class BasicAuth : IFunction
    {
        public int ParameterCount => 2;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is not StringValue strUsername)
            {
                throw new FunctionExecutionException("First argument should be the username string", this);
            }
            if (values[1] is not StringValue strPassword)
            {
                throw new FunctionExecutionException("Second argument should be the password string", this);
            }
            return Task.FromResult<Value>(new StringValue(Convert.ToBase64String(Encoding.ASCII.GetBytes($"{strUsername.Value}:{strPassword.Value}"))));
        }
    }
}
