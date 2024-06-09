using Antlr4.Runtime;

namespace Assertive.Exceptions
{
    internal class InterpretationException : Exception
    {
        public readonly ParserRuleContext Context;
        public InterpretationException(string message, ParserRuleContext ctx, string? filePath, Exception? innerException = null)
            : base($"{message}. Source: {ctx.GetText()} Line: {ctx.Start.Line} Col: {ctx.Start.Column} File: {filePath}", innerException)
        {
            Context = ctx;
        }
    }
}
