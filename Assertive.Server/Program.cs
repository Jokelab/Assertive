using Assertive.Extensions;
using Assertive.Server;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var outputWriter = new WebsocketOutputWriter();
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddTransient(typeof(IWebsocketOutputWriter), sp => outputWriter);
        builder.Services.AddAssertive(opt => opt.AddOutputWriter(outputWriter));

        var host = builder.Build();
        host.Run();
    }
}