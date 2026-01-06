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

                // Register haptic events - UI Interactions
                this.PluginEvents.AddEvent("hover", "Hover", "Light feedback for hovering over UI elements");
                this.PluginEvents.AddEvent("click", "Click", "Crisp click feedback for button presses");
                this.PluginEvents.AddEvent("double_click", "Double Click", "Double-tap pattern for special actions");
                this.PluginEvents.AddEvent("drag_start", "Drag Start", "Feedback when starting to drag");
                this.PluginEvents.AddEvent("drag_end", "Drag End", "Feedback when dropping item");
                this.PluginEvents.AddEvent("scroll_tick", "Scroll Tick", "Subtle tick for scroll events");
                this.PluginEvents.AddEvent("select", "Select", "Smooth feedback for selection");

                // Notifications
                this.PluginEvents.AddEvent("success", "Success", "Happy feedback for successful operations");
                this.PluginEvents.AddEvent("error", "Error", "Alert pattern for errors");
                this.PluginEvents.AddEvent("warning", "Warning", "Attention-grabbing warning");
                this.PluginEvents.AddEvent("info", "Info", "Gentle notification");
                this.PluginEvents.AddEvent("completed", "Completed", "Task completion celebration");

                // Gaming & Interactive
                this.PluginEvents.AddEvent("hit_light", "Light Hit", "Light impact feedback");
                this.PluginEvents.AddEvent("hit_heavy", "Heavy Hit", "Strong impact feedback");
                this.PluginEvents.AddEvent("damage", "Damage", "Taking damage feedback");
                this.PluginEvents.AddEvent("pickup", "Pickup", "Collecting item feedback");
                this.PluginEvents.AddEvent("level_up", "Level Up", "Achievement/level up celebration");

                // Creative & Special
                this.PluginEvents.AddEvent("pulse", "Pulse", "Rhythmic pulsing pattern");
                this.PluginEvents.AddEvent("wave", "Wave", "Smooth wave pattern");
                this.PluginEvents.AddEvent("firework", "Firework", "Burst pattern for celebrations");
                this.PluginEvents.AddEvent("heartbeat", "Heartbeat", "Rhythmic heartbeat pattern");

                // System
                this.PluginEvents.AddEvent("stop", "Stop", "Emergency stop - halt all haptic feedback");

                PluginLog.Info("Registered 21 haptic events");

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
