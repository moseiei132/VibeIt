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

                    // Process the received message (may send response)
                    await this.ProcessWebSocketMessage(webSocket, buffer, result.Count, result.MessageType);
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

        private async Task ProcessWebSocketMessage(WebSocket webSocket, Byte[] buffer, Int32 count, WebSocketMessageType messageType)
        {
            try
            {
                String eventName = null;

                // Binary protocol: single byte (0x00-0x15)
                if (messageType == WebSocketMessageType.Binary && count == 1)
                {
                    eventName = this.MapByteToEventName(buffer[0]);
                    PluginLog.Info($"Received binary event: 0x{buffer[0]:X2} -> {eventName}");
                }
                // JSON protocol: {"event": "event_name"} or {"command": "info"}
                else if (messageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, count);
                    
                    // Check if it's a command (info request)
                    var command = this.ParseJsonCommand(json);
                    if (command == "info" || command == "version")
                    {
                        await this.SendServerInfo(webSocket);
                        return;
                    }
                    
                    // Otherwise, parse as event
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

        private async Task SendServerInfo(WebSocket webSocket)
        {
            try
            {
                var info = new
                {
                    server = "VibeIt",
                    version = "1.0.0",
                    type = "WebSocket",
                    port = this._port,
                    protocols = new[] { "binary", "json" },
                    events = new[]
                    {
                        new { name = "hover", code = "0x01", category = "ui" },
                        new { name = "click", code = "0x02", category = "ui" },
                        new { name = "double_click", code = "0x03", category = "ui" },
                        new { name = "drag_start", code = "0x04", category = "ui" },
                        new { name = "drag_end", code = "0x05", category = "ui" },
                        new { name = "scroll_tick", code = "0x06", category = "ui" },
                        new { name = "select", code = "0x07", category = "ui" },
                        new { name = "success", code = "0x08", category = "notification" },
                        new { name = "error", code = "0x09", category = "notification" },
                        new { name = "warning", code = "0x0A", category = "notification" },
                        new { name = "info", code = "0x0B", category = "notification" },
                        new { name = "completed", code = "0x0C", category = "notification" },
                        new { name = "hit_light", code = "0x0D", category = "gaming" },
                        new { name = "hit_heavy", code = "0x0E", category = "gaming" },
                        new { name = "damage", code = "0x0F", category = "gaming" },
                        new { name = "pickup", code = "0x10", category = "gaming" },
                        new { name = "level_up", code = "0x11", category = "gaming" },
                        new { name = "pulse", code = "0x12", category = "creative" },
                        new { name = "wave", code = "0x13", category = "creative" },
                        new { name = "firework", code = "0x14", category = "creative" },
                        new { name = "heartbeat", code = "0x15", category = "creative" },
                        new { name = "stop", code = "0x00", category = "system" }
                    },
                    eventCount = 21,
                    features = new[] { "throttle", "cancel_previous", "immediate" }
                };

                var jsonResponse = JsonSerializer.Serialize(info);
                var bytes = Encoding.UTF8.GetBytes(jsonResponse);
                await webSocket.SendAsync(
                    new ArraySegment<Byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                PluginLog.Info("Sent server info to client");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error sending server info: {ex.Message}");
            }
        }

        private String MapByteToEventName(Byte eventByte)
        {
            return eventByte switch
            {
                0x00 => "stop",
                0x01 => "hover",
                0x02 => "click",
                0x03 => "double_click",
                0x04 => "drag_start",
                0x05 => "drag_end",
                0x06 => "scroll_tick",
                0x07 => "select",
                0x08 => "success",
                0x09 => "error",
                0x0A => "warning",
                0x0B => "info",
                0x0C => "completed",
                0x0D => "hit_light",
                0x0E => "hit_heavy",
                0x0F => "damage",
                0x10 => "pickup",
                0x11 => "level_up",
                0x12 => "pulse",
                0x13 => "wave",
                0x14 => "firework",
                0x15 => "heartbeat",
                _ => null
            };
        }

        private String ParseJsonCommand(String json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("command", out var cmdProperty))
                {
                    return cmdProperty.GetString();
                }
            }
            catch (JsonException)
            {
                // Not a command, might be an event
            }
            return null;
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
