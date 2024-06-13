using Assertive.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Assertive.LanguageServer
{
    public class TextDocumentHandler : IDidChangeTextDocumentHandler
    {
        private readonly ILanguageServerFacade _facade;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public TextDocumentHandler(ILanguageServerFacade facade, IServiceScopeFactory serviceScopeFactory)
        {
            _facade = facade;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = TextDocumentSelector.ForLanguage("assertive"),
                SyncKind = Change
            };
        }


        public async Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            // Perform syntax and semantic analysis here
            var diagnostics = new List<Diagnostic>();
            InterpretationResult interpretationResult = new();

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                var interpreter = scope.ServiceProvider.GetRequiredService<Interpreter>();
                interpretationResult = await interpreter.Execute(
                    request.ContentChanges.First().Text,
                    request.TextDocument.Uri.GetFileSystemPath())
                    .ConfigureAwait(false);
            }

            foreach (var syntaxError in interpretationResult.SyntaxErrors)
            {
                var startPos = new Position(syntaxError.OffendingSymbol.Line - 1, syntaxError.OffendingSymbol.Column);
                var endPos = new Position(syntaxError.OffendingSymbol.Line - 1, syntaxError.OffendingSymbol.Column + (syntaxError.OffendingSymbol.StopIndex - syntaxError.OffendingSymbol.StartIndex));
                diagnostics.Add(new Diagnostic
                {
                    Source = syntaxError.OffendingSymbol!.Text,
                    Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(startPos, endPos),
                    Severity = DiagnosticSeverity.Error,
                    Message = syntaxError.Message!
                });
            }

            if (interpretationResult.SyntaxErrors.Count == 0)
            {
                //syntax OK
                foreach (var semanticError in interpretationResult.SemanticErrors)
                {
                    var startPos = new Position(semanticError.Context.Start.Line - 1, semanticError.Context.Start.Column);
                    var endPos = new Position(semanticError.Context.Stop.Line - 1, semanticError.Context.Stop.Column);
                    diagnostics.Add(new Diagnostic
                    {
                        Source = semanticError.Context.GetText(),
                        Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(startPos, endPos),
                        Severity = DiagnosticSeverity.Error,
                        Message = semanticError.Message
                    });
                }
            }

            // Send diagnostics to the client
            var documentUri = request.TextDocument.Uri;
            var publishDiagnosticsParams = new PublishDiagnosticsParams
            {
                Uri = documentUri,
                Diagnostics = new Container<Diagnostic>(diagnostics),
                Version = request.TextDocument.Version
            };

            _facade.TextDocument.PublishDiagnostics(publishDiagnosticsParams);

            return new Unit();
        }


    }
}
