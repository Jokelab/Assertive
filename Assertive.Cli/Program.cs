using Assertive;
using Assertive.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
              .SetBasePath(AppContext.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
              .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddSystemdConsole();
            })
            .AddAssertive(opt =>
            {
                opt.AddOutputWriter<LogOutputWriter>();
            }
            )
            .BuildServiceProvider();

        var interpreter = serviceProvider.GetService<Interpreter>();
        if (interpreter == null)
            throw new Exception("Interpreter service not found");

        await interpreter.ExecuteFile(args[0]).ConfigureAwait(false);
    }
}