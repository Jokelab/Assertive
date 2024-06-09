using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Assertive.Models;

namespace Assertive
{
    public class FunctionStatementVisitor : AssertiveParserBaseVisitor<List<FunctionModel>>
    {
        private readonly List<FunctionModel> _functions = [];

        public override List<FunctionModel> Visit(IParseTree tree)
        {
            base.Visit(tree);
            return _functions;
        }

        public override List<FunctionModel> VisitFunctionStatement([NotNull] AssertiveParser.FunctionStatementContext context)
        {
            _functions.Add(new FunctionModel(context));
            return _functions;
        }
    }
}
