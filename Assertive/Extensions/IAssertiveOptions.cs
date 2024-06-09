using Assertive.Functions;
using Assertive.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Assertive.Extensions
{
    public interface IAssertiveOptions
    {
        IAssertiveOptions AddOutputWriter<T>() where T : IOutputWriter;
     
        IAssertiveOptions AddOutputWriter(IOutputWriter outputWriter);


        IAssertiveOptions ReplaceRequestDispatcher(IRequestDispatcher requestDispatcher);

        IAssertiveOptions AddBuiltInFunction<T>() where T : IFunction;

    }
}