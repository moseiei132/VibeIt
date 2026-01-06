namespace Loupedeck.VibeItPlugin
{
    using System;
    using Loupedeck.VibeItPlugin.Helpers;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class VibeItPlugin : Plugin
    {
        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        private HapticEventQueueManager _eventQueueManager;
        private WebSocketServer _webSocketServer;

        // Initializes a new instance of the plugin class.
        public VibeItPlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        // This method is called when the plugin is loaded.
        public override void Load()
        {
            try
            {
                PluginLog.Info("Loading VibeIt Universal Haptic Bridge...");

                // Register haptic events
                this.PluginEvents.AddEvent("soft_bump", "Soft Bump", "Light vibration for hover interactions");
                this.PluginEvents.AddEvent("sharp_click", "Sharp Click", "Crisp click feedback for button presses");
                this.PluginEvents.AddEvent("double_click", "Double Click", "Double-tap vibration pattern for alerts");
                this.PluginEvents.AddEvent("long_pulse", "Long Pulse", "Long vibration for warnings and errors");
                this.PluginEvents.AddEvent("stop", "Stop", "Emergency stop - halt all haptic feedback");

                PluginLog.Info("Registered 5 haptic events");

                // Initialize Event Queue Manager with haptic trigger callback
                this._eventQueueManager = new HapticEventQueueManager(this.TriggerHapticEvent);
                PluginLog.Info("Initialized HapticEventQueueManager");

                // Start WebSocket server (port 8765)
                this._webSocketServer = new WebSocketServer(8765, this._eventQueueManager.EnqueueEvent);
                this._webSocketServer.Start();

                PluginLog.Info("✅ VibeIt Universal Haptic Bridge loaded successfully!");
                PluginLog.Info("   WebSocket server: ws://localhost:8765");
                PluginLog.Info("   Supports: Binary (0x00-0x04) and JSON protocols");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load VibeIt plugin: {ex.Message}");
                PluginLog.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        // This method is called when the plugin is unloaded.
        public override void Unload()
        {
            try
            {
                PluginLog.Info("Unloading VibeIt Universal Haptic Bridge...");

                // Stop servers
                this._webSocketServer?.Dispose();
                this._eventQueueManager?.Dispose();

                PluginLog.Info("VibeIt plugin unloaded successfully");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error during plugin unload: {ex.Message}");
            }
        }

        private void TriggerHapticEvent(String eventName)
        {
            try
            {
                // Special handling for "stop" event
                if (eventName == "stop")
                {
                    PluginLog.Info("Emergency stop requested - halting haptic feedback");
                    // Note: Logitech SDK doesn't have explicit stop API, but we handle it in queue manager
                    return;
                }

                // Trigger the haptic event through Logitech SDK
                this.PluginEvents.RaiseEvent(eventName);
                PluginLog.Verbose($"Triggered haptic event: {eventName}");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error triggering haptic event '{eventName}': {ex.Message}");
            }
        }
    }
}
