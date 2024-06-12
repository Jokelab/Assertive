using Assertive.Functions;
using Assertive.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Assertive.Extensions
{
    internal class AssertiveOptions : IAssertiveOptions
    {
        private readonly IServiceCollection _services;
        public AssertiveOptions(IServiceCollection services)
        {
            _services = services;
        }
        public IServiceCollection Services => _services;

        public IAssertiveOptions AddOutputWriter<T>() where T : IOutputWriter
        {
            _services.AddTransient(typeof(IOutputWriter), typeof(T));
            return this;
        }
        public IAssertiveOptions AddOutputWriter(IOutputWriter outputWriter)
        {
            _services.AddTransient(typeof(IOutputWriter), (IServiceProvider sp) => { return outputWriter; });
            return this;
        }

        public IAssertiveOptions ReplaceRequestDispatcher(IRequestDispatcher requestDispatcher)
        {
            _services.Replace(new ServiceDescriptor(typeof(IRequestDispatcher), requestDispatcher));
            return this;
        }

        public IAssertiveOptions AddBuiltInFunction<T>() where T : IFunction
        {
            _services.AddScoped(serviceType: typeof(IFunction), implementationType: typeof(T));
            return this;
        }
    }
}
