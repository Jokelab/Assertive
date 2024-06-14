using Microsoft.Extensions.DependencyInjection;

namespace Assertive
{
    public class ProgramVisitorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ProgramVisitorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ProgramVisitor CreateVisitor(bool validation)
        {
            if (validation)
            {
                return _serviceProvider.GetRequiredService<ValidationProgramVisitor>();
            }
            return _serviceProvider.GetRequiredService<ProgramVisitor>();
        }

    }
}
