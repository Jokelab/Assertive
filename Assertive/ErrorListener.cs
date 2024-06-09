using Antlr4.Runtime;
using Assertive.Models;

namespace Assertive
{
    public class ErrorListener : BaseErrorListener
    {
        public List<SyntaxErrorModel> SyntaxErrors = new();
        public override void SyntaxError(
            TextWriter output, IRecognizer recognizer,
            IToken offendingSymbol, int line,
            int charPositionInLine, string msg,
            RecognitionException e)
        {
            SyntaxErrors.Add(new SyntaxErrorModel() { Line = line, CharPosition = charPositionInLine, SourceName = recognizer.InputStream.SourceName, Message = msg, RecognitionException = e });
        }
    }
}
