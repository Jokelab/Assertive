
using Assertive.Models;
using Assertive.Requests;
using Microsoft.Extensions.Logging;

namespace Assertive
{
    public class LogOutputWriter : IOutputWriter
    {
        private readonly ILogger<LogOutputWriter> _logger;
        public LogOutputWriter(ILogger<LogOutputWriter> logger)
        {
            _logger = logger;
        }
        public Task Write(string text)
        {
            _logger.LogInformation(text);
            return Task.CompletedTask;
        }

        public Task RequestStart(Request request)
        {
            _logger.LogInformation("{Id} -> : {Request}", request.Id, request);
            return Task.CompletedTask;
        }

        public Task RequestEnd(Request request)
        {
            _logger.LogInformation("{Id} <- : {Response}", request.Id, request);
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionStart(AnnotatedFunction annotatedFunction)
        {
            _logger.LogInformation($"Start {annotatedFunction.FunctionName} ({annotatedFunction.Annotation})");
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionEnd(AnnotatedFunction annotatedFunction)
        {
            _logger.LogInformation($"End {annotatedFunction.FunctionName} ({annotatedFunction.Annotation}) - Duration: {annotatedFunction.DurationMs}");
            return Task.CompletedTask;
        }

        public Task Assertion(AssertResult assertResult)
        {
            _logger.LogInformation($"Assert  {assertResult.ExpressionText} {(assertResult.Description)} {(assertResult.Passed ? "PASSED" : "FAILED")}");
            return Task.CompletedTask;
        }
    }
}
