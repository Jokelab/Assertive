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

        public string Name { get => Context.functionName.Text; }

        public bool IsAssertFunction { get => Context.assertFunction != null; }

    }
}
