using Antlr4.Runtime;

namespace Assertive.Models
{
    public class SemanticErrorModel
    {
        public required string ErrorCode { get; set; }
        public required string Message { get; set; }
        public required ParserRuleContext Context { get; set; }
        public required string? FilePath { get; set; }
    }
}
