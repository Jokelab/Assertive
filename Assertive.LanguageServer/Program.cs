using Assertive.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Server;


var server = await LanguageServer.From(options =>
               options
                   .WithInput(Console.OpenStandardInput())
                   .WithOutput(Console.OpenStandardOutput())
                   .WithHandler<TextDocumentHandler>()
           );

await server.WaitForExit;