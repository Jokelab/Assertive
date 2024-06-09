namespace Assertive.Models
{
    public class AssertResult
    {
        public bool Passed { get; set; }
        public required string Description { get; set; }
        public required string ExpressionText { get; set; }
    }
}
