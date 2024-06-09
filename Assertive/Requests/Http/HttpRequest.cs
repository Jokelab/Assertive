using Assertive.Requests;

namespace Assertive.Requests.Http
{
    public class HttpRequest : Request
    {
        public long DurationMs { get; set; }

        public required HttpRequestMessage Request { get; set; }
        public HttpResponseMessage? Response { get; set; }

        public override string RequestType { get => nameof(HttpRequest); }

        public override string ToString()
        {

            if (Response != null)
            {
                return $"{Response.StatusCode}. Duration: {DurationMs}ms";
            }
            return $"{Request.Method.Method} {Request.RequestUri}";
        }
    }

}
