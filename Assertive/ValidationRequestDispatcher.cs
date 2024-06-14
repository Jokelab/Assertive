using Assertive.Requests;
using Assertive.Requests.Http;

namespace Assertive
{
    internal class ValidationRequestDispatcher : IRequestDispatcher
    {
        public HttpRequest CreateRequest(HttpRequestMessage request)
        {
            return new HttpRequest() { Request  = request };
        }

        public Task SendRequest(HttpRequest request)
        {
            return Task.CompletedTask;
        }
    }
}
