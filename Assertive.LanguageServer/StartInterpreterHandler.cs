using Assertive.LanguageServer.Models;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
namespace Assertive.LanguageServer
{
    public class StartInterpreterHandler : IJsonRpcNotificationHandler<InterpretationRequest>
    {
        private readonly Interpreter _interpreter;

        public StartInterpreterHandler(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public async Task<Unit> Handle(InterpretationRequest request, CancellationToken cancellationToken)
        {
            // Interpretation logic here
            await _interpreter.ExecuteFile(request.FilePath);
            return Unit.Value;
        }
    }

}
