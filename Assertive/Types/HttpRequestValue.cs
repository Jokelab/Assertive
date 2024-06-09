using Assertive.Requests.Http;

namespace Assertive.Types
{
    internal class HttpRequestValue : Value
    {
        private HttpRequest _request;
        private string? _responseBody;

        public HttpRequestValue(HttpRequest httpRequest)
        {
            _request = httpRequest;
        }

        public HttpRequest GetRequest() => _request;

        public async Task<string?> GetResponseBody()
        {
            if (string.IsNullOrEmpty(_responseBody) && _request.Response != null)
            {
                _responseBody = await _request.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return _responseBody;
        }

        public override string ToString()
        {
            return _request.ToString();
        }
    }
}
