using Microsoft.Extensions.DependencyInjection;

namespace Assertive.Functions
{
    public class FunctionFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public FunctionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IFunction? CreateFunction(string name)
        {
            return _serviceProvider.GetKeyedService<IFunction>(name);
        }

        public bool BuiltInFunctionExists(string name)
        {
            return CreateFunction(name) != null;
        }
    }
}
