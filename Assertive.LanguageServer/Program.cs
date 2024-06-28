using Assertive.Extensions;
using Assertive.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;


var server = await LanguageServer.From(options =>
                options.WithInput(Console.OpenStandardInput())
                   .WithOutput(Console.OpenStandardOutput())
                   .OnStarted((ILanguageServer server, CancellationToken token) => { 
                       server.SendNotification("assertive/started"); 
                       return Task.CompletedTask; }
                   )
                   .WithHandler<TextDocumentHandler>()
                   .WithHandler<InterpreterRequestHandler>()
                   .WithServices(ConfigureServices)
           );

await server.WaitForExit;

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<DocumentManager>();
    services.AddAssertive(opt => opt.AddOutputWriter<LanguageServerOutputWriter>());
}