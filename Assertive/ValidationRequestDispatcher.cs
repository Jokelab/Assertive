using Assertive.Requests;
using Assertive.Requests.Http;
using System.Diagnostics;

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
            request.Response = new HttpResponseMessage();
            return Task.CompletedTask;
        }
    }
}
