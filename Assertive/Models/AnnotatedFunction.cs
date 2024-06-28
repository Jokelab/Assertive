namespace Assertive.Models
{
    public class AnnotatedFunction
    {
        public required string Invocation { get; set; }
        public required string FunctionName { get; set; }
        public required string Annotation { get; set; }
        public long TotalDurationMs { get; set; }

        public long TotalRequests { get; set; }
        public long TotalRequestDurationMs { get; set; }
        public long AvgRequestDurationMs => TotalRequests > 0 ? TotalRequestDurationMs / TotalRequests : 0;

    }
}
