using Assertive.Requests.Http;

namespace Assertive.Requests
{
    public interface IRequestDispatcher
    {
        Task SendRequest(HttpRequest request);
        HttpRequest CreateRequest(HttpRequestMessage request);
    }
}