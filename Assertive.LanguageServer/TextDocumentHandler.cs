using MediatR;
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
        public TextDocumentHandler(ILanguageServerFacade facade)
        {
            _facade = facade;
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


        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            // Perform syntax and semantic analysis here
            var diagnostics = new List<Diagnostic>();
            // Populate diagnostics with errors and warnings

            var parsedDocument = Parser.Parse(request.ContentChanges.First().Text, request.TextDocument.Uri.ToString());

            foreach (var syntaxError in parsedDocument.SyntaxErrors)
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
            // Example: Creating a diagnostic


            // Send diagnostics to the client
            var documentUri = request.TextDocument.Uri;
            var publishDiagnosticsParams = new PublishDiagnosticsParams
            {
                Uri = documentUri,
                Diagnostics = new Container<Diagnostic>(diagnostics),
                Version = request.TextDocument.Version
            };

            _facade.TextDocument.PublishDiagnostics(publishDiagnosticsParams);

            return Unit.Task;
        }


    }
}
