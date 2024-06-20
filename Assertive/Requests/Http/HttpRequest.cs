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
                var strTask = Response.Content.ReadAsStringAsync();
                strTask.Wait();
                return $"{Response.StatusCode}. Duration: {DurationMs}ms; Content: {strTask.Result}";
            }
            return $"{Request.Method.Method} {Request.RequestUri}";
        }
    }

}
