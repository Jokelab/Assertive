using Assertive.Models;
using Assertive.Requests;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Assertive.Server
{
    public class WebsocketOutputWriter : IWebsocketOutputWriter
    {
        private WebSocket? _currentSocket;
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly ConcurrentQueue<string> _messageQueue = new();
        private bool _isProcessing;

        public void SetWebsocket(WebSocket webSocket)
        {
            _currentSocket = webSocket;
        }

        public async Task RequestEnd(Request response)
        {
            var json = JsonConvert.SerializeObject(response);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        public async Task RequestStart(Request request)
        {
            var json = JsonConvert.SerializeObject(request);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        public async Task Write(string text)
        {
            var json = JsonConvert.SerializeObject(text);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        public async Task AnnotatedFunctionStart(AnnotatedFunction annotatedFunction)
        {
            var json = JsonConvert.SerializeObject(annotatedFunction);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        public async Task Assertion(AssertResult assertResult)
        {
            var json = JsonConvert.SerializeObject(assertResult);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        public async Task AnnotatedFunctionEnd(AnnotatedFunction annotatedFunction)
        {
            var json = JsonConvert.SerializeObject(annotatedFunction);
            _messageQueue.Enqueue(json);
            await ProcessQueue();
        }

        private async Task ProcessQueue()
        {
            if (_isProcessing)
            {
                return;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_isProcessing)
                {
                    return;
                }

                _isProcessing = true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            while (true)
            {
                await _semaphoreSlim.WaitAsync();
                string? message = null;
                try
                {
                    if (_messageQueue.TryDequeue(out message))
                    {
                        // Got a message, release the semaphore for the next iteration
                        _semaphoreSlim.Release();
                    }
                    else
                    {
                        // No more messages, set _isProcessing to false and exit the loop
                        _isProcessing = false;
                        _semaphoreSlim.Release();
                        break;
                    }
                }
                catch
                {
                    _semaphoreSlim.Release();
                    throw;
                }

                if (message != null)
                {
                    await WriteToWebSocket(message);
                }
            }
        }

        private async Task WriteToWebSocket(string message)
        {
            byte[] outputBuffer = Encoding.UTF8.GetBytes(message);
            if (_currentSocket != null && _currentSocket.State == WebSocketState.Open)
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    await _currentSocket.SendAsync(new ArraySegment<byte>(outputBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
        }


    }
}
