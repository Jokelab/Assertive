using Assertive.Functions;
using Assertive.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace Assertive.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAssertive(this IServiceCollection services, Action<IAssertiveOptions>? configureOptions = null)
        {
            services.AddHttpClient();
            services.AddTransient<IRequestDispatcher, RequestDispatcher>();
            var assertiveOptions = new AssertiveOptions(services);
            if (configureOptions != null)
            {
                configureOptions(assertiveOptions);
            }
            services.AddTransient<IFileSystemService, FileSystemService>();
            services.AddTransient<Interpreter>();
            services.AddTransient<ProgramVisitor>();
            
            services.AddTransient<ProgramVisitorFactory>();

            services.AddTransient<SyntaxErrorListener>();
            services.AddTransient<FunctionStatementVisitor>();
            services.AddSingleton<FunctionFactory>();

            RegisterBuiltInFunctions(assertiveOptions);

            //special services for semantic validation purposes
            services.AddSingleton<ValidationRequestDispatcher>();
            services.AddTransient(sp => {
                return new ValidationProgramVisitor(new ValidationRequestDispatcher(),
                    sp.GetRequiredService<FunctionFactory>(),
                    [], //no output writers 
                    sp.GetRequiredService<ILogger<ProgramVisitor>>());
                    }
            );

            return services;
        }

        private static void RegisterBuiltInFunctions(IAssertiveOptions options)
        {
            options.AddBuiltInFunction<Add>();
            options.AddBuiltInFunction<Remove>();
            options.AddBuiltInFunction<Count>();
            options.AddBuiltInFunction<Get>();
            options.AddBuiltInFunction<Duration>();
            options.AddBuiltInFunction<FileToString>();
            options.AddBuiltInFunction<FileToStream>();
            options.AddBuiltInFunction<JsonPath>();
            options.AddBuiltInFunction<StringLength>();
            options.AddBuiltInFunction<StatusCode>();
        }
    }
}