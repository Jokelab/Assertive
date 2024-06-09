using System.Net.WebSockets;

namespace Assertive.Server
{
    public interface IWebsocketOutputWriter : IOutputWriter
    {
        void SetWebsocket(WebSocket webSocket);
        
    }
}
