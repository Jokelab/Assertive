namespace Assertive.Requests
{
    public abstract class Request
    {
        public ulong Id { get; set; }
        public abstract string RequestType { get; }
    }
}
