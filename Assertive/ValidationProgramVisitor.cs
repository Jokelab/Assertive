using Assertive.Functions;
using Assertive.Requests;
using Assertive.Requests.Http;
using Microsoft.Extensions.Logging;

namespace Assertive
{
    public class ValidationProgramVisitor : ProgramVisitor
    {
        public ValidationProgramVisitor(IRequestDispatcher requestDispatcher, FunctionFactory functionFactory, IEnumerable<IOutputWriter> outputWriters, ILogger<ProgramVisitor> logger) : base(requestDispatcher, functionFactory, outputWriters, logger)
        {
        }

        public override Task<HttpRequest> SendRequest(HttpRequestMessage requestMessage)
        {
            var model = new HttpRequest() { Request = requestMessage };
            model.Response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK };
            return Task.FromResult(model);
        }

    }
}
