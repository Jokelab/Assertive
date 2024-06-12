namespace Assertive.Models
{
    public class ParsedDocument
    {
        public ParsedDocument(AssertiveParser.ProgramContext context, string? path, List<SyntaxErrorModel> syntaxErrors)
        {
            Context = context;
            Path = path;
            SyntaxErrors = syntaxErrors;
        }
        public AssertiveParser.ProgramContext Context { get; }
        public string? Path { get; }

        public List<SyntaxErrorModel> SyntaxErrors { get; }
    }
}
