using System.Net.WebSockets;

namespace Assertive.InterpreterServer
{
    public interface IWebsocketOutputWriter : IOutputWriter
    {
        void SetWebsocket(WebSocket webSocket);
        
    }
}
