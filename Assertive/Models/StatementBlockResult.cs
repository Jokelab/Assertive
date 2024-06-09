using Assertive.Types;

namespace Assertive.Models
{
    public class StatementBlockResult
    {
        public required Value ReturnValue { get; set; }
        public bool ShouldBreak { get; set; }
        public bool ShouldContinue { get; set; }
    }
}
