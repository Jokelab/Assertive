using Assertive.Functions;
using Assertive.Requests;
using Microsoft.Extensions.Logging;

namespace Assertive
{
    public class ValidationProgramVisitor : ProgramVisitor
    {
        public ValidationProgramVisitor(IRequestDispatcher requestDispatcher, FunctionFactory functionFactory, IEnumerable<IOutputWriter> outputWriters, ILogger<ProgramVisitor> logger) : base(requestDispatcher, functionFactory, outputWriters, logger)
        {
        }

    }
}
