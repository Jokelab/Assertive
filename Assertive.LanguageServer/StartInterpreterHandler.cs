using Assertive.LanguageServer.Models;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
namespace Assertive.LanguageServer
{
    public class StartInterpreterHandler : IJsonRpcNotificationHandler<InterpretationRequest>
    {
        private readonly IServiceProvider _serviceProvider;

        public StartInterpreterHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }



        public async Task<Unit> Handle(InterpretationRequest request, CancellationToken cancellationToken)
        {
            var interpreterService = _serviceProvider.GetRequiredService<Interpreter>();
            // Interpretation logic here
            await interpreterService.ExecuteFile(request.FilePath);
            return Unit.Value;
        }
    }

}
