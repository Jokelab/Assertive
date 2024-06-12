using Assertive.Extensions;
using Assertive.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Server;


var server = await LanguageServer.From(options =>
               options
                   .WithInput(Console.OpenStandardInput())
                   .WithOutput(Console.OpenStandardOutput())
                   .WithHandler<TextDocumentHandler>()
                   .WithServices(ConfigureServices)
           );

await server.WaitForExit;

static void ConfigureServices(IServiceCollection services)
{
    services.AddAssertive();
}