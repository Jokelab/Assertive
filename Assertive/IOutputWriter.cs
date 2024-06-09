using Assertive.Models;
using Assertive.Requests;

namespace Assertive
{
    public interface IOutputWriter
    {
        Task Write(string text);

        Task RequestStart(Request request);

        Task RequestEnd(Request request);

        Task AnnotatedFunctionStart(AnnotatedFunction function);
        Task AnnotatedFunctionEnd(AnnotatedFunction function);

        Task Assertion(AssertResult assertResult);
    }
}