using Antlr4.Runtime;

namespace Assertive.Models
{
    public class SyntaxErrorModel
    {
        public int Line { get; set; }
        public int CharPosition { get; set; }
        public string? SourceName { get; set; }
        public string? Message { get; set; }
        public RecognitionException? RecognitionException { get; set; }

        public override string ToString()
        {
            return string.Format("line:{0} col:{1} src:{2} msg:{3}", Line, CharPosition, SourceName, Message);
        }
    }
}
