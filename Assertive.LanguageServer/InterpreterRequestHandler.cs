using Assertive.LanguageServer.Models;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
namespace Assertive.LanguageServer
{
    public class InterpreterRequestHandler : IJsonRpcNotificationHandler<InterpreterRequest>
    {
        private readonly Interpreter _interpreter;

        public InterpreterRequestHandler(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public async Task<Unit> Handle(InterpreterRequest request, CancellationToken cancellationToken)
        {
            // Interpretation logic here
            await _interpreter.ExecuteFile(request.FilePath);
            return Unit.Value;
        }
    }

}
