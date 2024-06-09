using Assertive.Requests.Http;
using System.Diagnostics;

namespace Assertive.Requests
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly HttpClient _client;
        private ulong _currentId;
        public RequestDispatcher(HttpClient httpClient)
        {
            _client = httpClient;
            _client.Timeout = TimeSpan.FromMinutes(10);
        }

        private ulong GetNextId()
        {
            return Interlocked.Increment(ref _currentId);
        }

        public HttpRequest CreateRequest(HttpRequestMessage request)
        {
            return new HttpRequest() { Id = GetNextId(), Request = request };
        }

        public async Task SendRequest(HttpRequest requestModel)
        {
            var sw = new Stopwatch();
            sw.Start();
            requestModel.Response = await _client.SendAsync(requestModel.Request).ConfigureAwait(false);
            sw.Stop();
            requestModel.DurationMs = (long)sw.Elapsed.TotalMilliseconds;
        }


    }
}
