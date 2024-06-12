using Antlr4.Runtime;
using Assertive.Models;

namespace Assertive
{
    public static class Parser
    {
        public static ParsedDocument Parse(string program, string? path)
        {
            var inputStream = new AntlrInputStream(program);
            var lexer = new AssertiveLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new AssertiveParser(commonTokenStream);
            parser.RemoveErrorListeners();
            var errorListener = new SyntaxErrorListener();
            parser.AddErrorListener(errorListener);

            var programContext = parser.program();

            return new ParsedDocument(programContext, path, errorListener.SyntaxErrors);
        }
    }
}
