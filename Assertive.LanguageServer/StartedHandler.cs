using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Assertive.LanguageServer
{
    internal class StartedHandler : IOnLanguageServerStarted
    {
        private readonly ILanguageServerFacade _facade;
        public StartedHandler (ILanguageServerFacade facade)
        {
            _facade = facade;
        }
        public Task OnStarted(ILanguageServer server, CancellationToken cancellationToken)
        {
            _facade.SendNotification("assertive/Started");
            return Task.CompletedTask;
        }
    }
}
