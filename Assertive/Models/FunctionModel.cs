using static AssertiveParser;

namespace Assertive.Models
{
    public class FunctionModel
    {
        public FunctionStatementContext Context { get; }
        public FunctionModel(FunctionStatementContext context)
        {

            Context = context;
        }

        public string Name { get => Context.ID().GetText(); }

    }
}
