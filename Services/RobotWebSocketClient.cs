using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RobotControlClient.Models;

namespace RobotControlClient.Services
{
    public class RobotWebSocketClient : IDisposable
    {
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly string _wsUrl;
        private bool _isConnected;

        // Events for different message types
        public event EventHandler<WebSocketMessage>? OnStatusUpdate;
        public event EventHandler<WebSocketMessage>? OnTaskUpdate;
        public event EventHandler<WebSocketMessage>? OnTaskCompleted;
        public event EventHandler<WebSocketMessage>? OnTaskFailed;
        public event EventHandler<WebSocketMessage>? OnError;
        public event EventHandler<WebSocketMessage>? OnHeartbeat;
        public event EventHandler? OnConnected;
        public event EventHandler? OnDisconnected;

        public bool IsConnected => _isConnected;

        public RobotWebSocketClient(string baseUrl = "ws://localhost:8000")
        {
            _wsUrl = $"{baseUrl}/robot/ws";
        }

        public async Task ConnectAsync()
        {
            try
            {
                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                await _webSocket.ConnectAsync(new Uri(_wsUrl), _cancellationTokenSource.Token);
                _isConnected = true;

                OnConnected?.Invoke(this, EventArgs.Empty);

                // Start listening for messages
                _ = Task.Run(async () => await ListenForMessages(), _cancellationTokenSource.Token);

                Console.WriteLine("WebSocket connected successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to WebSocket: {ex.Message}");
                _isConnected = false;
                throw;
            }
        }

        private async Task ListenForMessages()
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            
            try
            {
                while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource!.Token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageText = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                        ProcessMessage(messageText);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        break;
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                _isConnected = false;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in WebSocket listener: {ex.Message}");
                _isConnected = false;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ProcessMessage(string messageText)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<WebSocketMessage>(messageText);
                if (message == null) return;

                Console.WriteLine($"Received WebSocket message: {message.Type}");

                switch (message.Type?.ToLower())
                {
                    case "status":
                        OnStatusUpdate?.Invoke(this, message);
                        break;
                    case "task_update":
                        OnTaskUpdate?.Invoke(this, message);
                        break;
                    case "task_completed":
                        OnTaskCompleted?.Invoke(this, message);
                        break;
                    case "task_failed":
                        OnTaskFailed?.Invoke(this, message);
                        break;
                    case "error":
                        OnError?.Invoke(this, message);
                        break;
                    case "heartbeat":
                        OnHeartbeat?.Invoke(this, message);
                        break;
                    default:
                        Console.WriteLine($"Unknown message type: {message.Type}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse WebSocket message: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        "Client disconnecting", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during WebSocket disconnect: {ex.Message}");
                }
            }

            _isConnected = false;
            _cancellationTokenSource?.Cancel();
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_isConnected)
            {
                DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
            }
            
            _cancellationTokenSource?.Dispose();
            _webSocket?.Dispose();
        }
    }
}