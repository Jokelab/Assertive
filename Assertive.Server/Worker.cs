using System.Net.WebSockets;
using System.Net;
using System.Text;

namespace Assertive.Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Interpreter _interpreter;
        private readonly IWebsocketOutputWriter _outputWriter;

        public Worker(Interpreter interpreter, IWebsocketOutputWriter outputWriter, ILogger<Worker> logger)
        {
            _interpreter = interpreter;
            _outputWriter = outputWriter;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HttpListener listener = new();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            _logger.LogInformation("Assertive WebSocket server started at ws://localhost:5000/");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for WebSocket connection at: {time}", DateTimeOffset.Now);

                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = webSocketContext.WebSocket;

                    // Receive the file path to start execution
                    byte[] buffer = new byte[1024 * 4];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string filePath = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine($"Received file path: {filePath}");

                    _outputWriter.SetWebsocket(webSocket);
                    await _interpreter.ExecuteFile(filePath);

                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", stoppingToken);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }

        }
    }
}

