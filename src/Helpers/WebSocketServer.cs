namespace Loupedeck.VibeItPlugin.Helpers
{
    using System;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebSocketServer : IDisposable
    {
        private readonly Int32 _port;
        private readonly Action<String> _onEventReceived;
        private HttpListener _httpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenerTask;
        private Boolean _isRunning = false;

        public WebSocketServer(Int32 port, Action<String> onEventReceived)
        {
            this._port = port;
            this._onEventReceived = onEventReceived ?? throw new ArgumentNullException(nameof(onEventReceived));
        }

        public void Start()
        {
            if (this._isRunning)
            {
                PluginLog.Warning("WebSocket server is already running");
                return;
            }

            try
            {
                this._httpListener = new HttpListener();
                this._httpListener.Prefixes.Add($"http://localhost:{this._port}/");
                this._httpListener.Start();

                this._cancellationTokenSource = new CancellationTokenSource();
                this._listenerTask = Task.Run(() => this.ListenForConnections(this._cancellationTokenSource.Token));
                
                this._isRunning = true;
                PluginLog.Info($"WebSocket server started on port {this._port}");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to start WebSocket server: {ex.Message}");
                throw;
            }
        }

        private async Task ListenForConnections(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await this._httpListener.GetContextAsync();
                    
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = Task.Run(() => this.HandleWebSocketConnection(context), cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    // Expected when stopping the server
                    break;
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"WebSocket listener error: {ex.Message}");
                }
            }
        }

        private async Task HandleWebSocketConnection(HttpListenerContext context)
        {
            WebSocket webSocket = null;
            try
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                webSocket = wsContext.WebSocket;
                PluginLog.Info($"WebSocket client connected from {context.Request.RemoteEndPoint}");

                var buffer = new Byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<Byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        PluginLog.Info("WebSocket client disconnected normally");
                        break;
                    }

                    // Process the received message
                    this.ProcessWebSocketMessage(buffer, result.Count, result.MessageType);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"WebSocket connection error: {ex.Message}");
            }
            finally
            {
                webSocket?.Dispose();
            }
        }

        private void ProcessWebSocketMessage(Byte[] buffer, Int32 count, WebSocketMessageType messageType)
        {
            try
            {
                String eventName = null;

                // Binary protocol: single byte (0x00-0x04)
                if (messageType == WebSocketMessageType.Binary && count == 1)
                {
                    eventName = this.MapByteToEventName(buffer[0]);
                    PluginLog.Info($"Received binary event: 0x{buffer[0]:X2} -> {eventName}");
                }
                // JSON protocol: {"event": "event_name"}
                else if (messageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, count);
                    eventName = this.ParseJsonEvent(json);
                    PluginLog.Info($"Received JSON event: {json} -> {eventName}");
                }
                else
                {
                    PluginLog.Warning($"Unsupported WebSocket message: type={messageType}, length={count}");
                    return;
                }

                if (!String.IsNullOrEmpty(eventName))
                {
                    this._onEventReceived(eventName);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error processing WebSocket message: {ex.Message}");
            }
        }

        private String MapByteToEventName(Byte eventByte)
        {
            return eventByte switch
            {
                0x00 => "stop",
                0x01 => "soft_bump",
                0x02 => "sharp_click",
                0x03 => "double_click",
                0x04 => "long_pulse",
                _ => null
            };
        }

        private String ParseJsonEvent(String json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("event", out var eventProperty))
                {
                    return eventProperty.GetString();
                }
            }
            catch (JsonException ex)
            {
                PluginLog.Warning($"Invalid JSON format: {ex.Message}");
            }
            return null;
        }

        public void Stop()
        {
            if (!this._isRunning)
            {
                return;
            }

            try
            {
                this._cancellationTokenSource?.Cancel();
                this._httpListener?.Stop();
                this._listenerTask?.Wait(TimeSpan.FromSeconds(2));
                
                this._isRunning = false;
                PluginLog.Info("WebSocket server stopped");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error stopping WebSocket server: {ex.Message}");
            }
        }

        public void Dispose()
        {
            this.Stop();
            this._cancellationTokenSource?.Dispose();
            this._httpListener?.Close();
            PluginLog.Info("WebSocket server disposed");
        }
    }
}
