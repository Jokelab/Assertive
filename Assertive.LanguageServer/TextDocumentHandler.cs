using Assertive.Models;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Assertive.LanguageServer
{
    public class TextDocumentHandler : IDidChangeTextDocumentHandler, IDidSaveTextDocumentHandler, IDidOpenTextDocumentHandler
    {
        private readonly ILanguageServerFacade _facade;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DocumentManager _documentManager;
        public TextDocumentHandler(ILanguageServerFacade facade, IServiceScopeFactory serviceScopeFactory, DocumentManager documentManager)
        {
            _facade = facade;
            _serviceScopeFactory = serviceScopeFactory;
            _documentManager = documentManager;
        }
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Incremental;

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
            if (!_documentManager.HasDocument(request.TextDocument.Uri))
            {
                _documentManager.SetDocument(request.TextDocument.Uri, string.Empty);
            }

            // Apply changes to the document
            _documentManager.ApplyChanges(request.TextDocument.Uri, request.ContentChanges);

            await ValidateDocumentAsync(request.TextDocument.Uri, _documentManager.GetDocumentContent(request.TextDocument.Uri), request.TextDocument.Version);

            return new Unit();
        }

        public async Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            var content = File.ReadAllText(request.TextDocument.Uri.GetFileSystemPath());
            _documentManager.SetDocument(request.TextDocument.Uri, content);
            await ValidateDocumentAsync(request.TextDocument.Uri, content);
            return new Unit();
        }

        public async Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            
            var content = File.ReadAllText(request.TextDocument.Uri.GetFileSystemPath());
            _documentManager.SetDocument(request.TextDocument.Uri, content);
            await ValidateDocumentAsync(request.TextDocument.Uri, content, request.TextDocument.Version);
            return new Unit();
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = TextDocumentSelector.ForLanguage("assertive"),
            };
        }

        TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentOpenRegistrationOptions()
            {
                DocumentSelector = TextDocumentSelector.ForLanguage("assertive"),
            };
        }

        private async Task ValidateDocumentAsync(DocumentUri documentUri, string content, int? version = null)
        {
            var diagnostics = new List<Diagnostic>();
            InterpretationResult interpretationResult = new();

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                var interpreter = scope.ServiceProvider.GetRequiredService<Interpreter>();
                interpreter.InterpreterMode = InterpreterMode.Validate;

                interpretationResult = await interpreter.Execute(
                    content,
                    documentUri.GetFileSystemPath())
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
        
            var publishDiagnosticsParams = new PublishDiagnosticsParams
            {
                Uri = documentUri,
                Diagnostics = new Container<Diagnostic>(diagnostics),
                Version = version
            };

            _facade.TextDocument.PublishDiagnostics(publishDiagnosticsParams);
        }
    }
}
