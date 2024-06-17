using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
