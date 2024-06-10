using Assertive.Extensions;
using Assertive.InterpreterServer;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var outputWriter = new WebsocketOutputWriter();
        builder.Services.AddTransient(typeof(IWebsocketOutputWriter), sp => outputWriter);
        builder.Services.AddAssertive(opt => opt.AddOutputWriter(outputWriter));
        builder.Services.AddHostedService<InterpreterWorker>();

        var host = builder.Build();
        host.Run();
    }
}