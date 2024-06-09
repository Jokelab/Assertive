using Assertive.Functions;

namespace Assertive.Exceptions
{

    internal class FunctionExecutionException : Exception
    {
        public IFunction FunctionInstance { get; private set; }
        public FunctionExecutionException(string message, IFunction functionInstance) : base(message)
        {
            FunctionInstance = functionInstance;
        }
    }
}
