namespace Assertive.Models
{
    public class InterpretationResult
    {
        public List<SyntaxErrorModel> SyntaxErrors { get; set; } = new();
        public List<SemanticErrorModel> SemanticErrors { get; set; } = new();
    }
}
