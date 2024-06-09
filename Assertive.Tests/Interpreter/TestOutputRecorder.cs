using Assertive.Models;
using Assertive.Requests;
using System.Text;

namespace Assertive.Tests.Interpreter
{
    public class TestOutputRecorder : IOutputWriter
    {
        public StringBuilder RecordedOutput { get; set; } = new();
        public List<Request> Requests { get; set; } = new();
        public List<AnnotatedFunction> AnnotatedFunctions { get; set; } = new();
        public List<AssertResult> Assertions { get; set; } = new();
        public Task Write(string text)
        {
            RecordedOutput.Append(text);
            return Task.CompletedTask;
        }

        public string Output => RecordedOutput.ToString();

        public Task RequestStart(Request request)
        {
            Requests.Add(request);
            return Task.CompletedTask;
        }

        public Task RequestEnd(Request request)
        {
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionStart(AnnotatedFunction function)
        {
            AnnotatedFunctions.Add(function);
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionEnd(AnnotatedFunction function)
        {
            AnnotatedFunctions.Add(function);
            return Task.CompletedTask;
        }

        public Task Assertion(AssertResult assertResult)
        {
            Assertions.Add(assertResult);
            return Task.CompletedTask;
        }
    }
}
