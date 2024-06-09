namespace Assertive.Models
{
    public class AnnotatedFunction
    {
        public required string Invocation { get; set; }
        public required string FunctionName { get; set; }
        public required string Annotation { get; set; }
        public long DurationMs { get; set; }

    }
}
