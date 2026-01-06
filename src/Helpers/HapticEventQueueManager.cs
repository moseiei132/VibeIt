namespace Loupedeck.VibeItPlugin.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public enum QueueStrategy
    {
        CancelPrevious,  // Stop current haptic, play new one immediately
        Throttle,        // Limit to X events per second
        Immediate        // No queuing, execute right away
    }

    public class HapticEventQueueManager : IDisposable
    {
        private readonly Action<String> _triggerHapticEvent;
        private readonly Dictionary<String, QueueStrategy> _eventStrategies;
        private readonly Dictionary<String, Int32> _throttleIntervals; // in milliseconds
        private readonly ConcurrentDictionary<String, DateTime> _lastExecutionTimes;
        private readonly ConcurrentDictionary<String, CancellationTokenSource> _activeTasks;
        private Boolean _disposed = false;

        public HapticEventQueueManager(Action<String> triggerHapticEvent)
        {
            this._triggerHapticEvent = triggerHapticEvent ?? throw new ArgumentNullException(nameof(triggerHapticEvent));
            this._eventStrategies = new Dictionary<String, QueueStrategy>();
            this._throttleIntervals = new Dictionary<String, Int32>();
            this._lastExecutionTimes = new ConcurrentDictionary<String, DateTime>();
            this._activeTasks = new ConcurrentDictionary<String, CancellationTokenSource>();

            // Configure default strategies
            // UI Interactions - mostly throttled for smooth feel
            this.ConfigureEventStrategy("hover", QueueStrategy.Throttle, 100);         // 10 Hz
            this.ConfigureEventStrategy("click", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("double_click", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("drag_start", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("drag_end", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("scroll_tick", QueueStrategy.Throttle, 50);    // 20 Hz for responsive scroll
            this.ConfigureEventStrategy("select", QueueStrategy.CancelPrevious);

            // Notifications - cancel previous for immediate feedback
            this.ConfigureEventStrategy("success", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("error", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("warning", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("info", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("completed", QueueStrategy.CancelPrevious);

            // Gaming & Interactive - responsive
            this.ConfigureEventStrategy("hit_light", QueueStrategy.Throttle, 80);      // 12.5 Hz for rapid hits
            this.ConfigureEventStrategy("hit_heavy", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("damage", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("pickup", QueueStrategy.Throttle, 100);        // 10 Hz for rapid pickups
            this.ConfigureEventStrategy("level_up", QueueStrategy.CancelPrevious);

            // Creative & Special - let them play out
            this.ConfigureEventStrategy("pulse", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("wave", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("firework", QueueStrategy.CancelPrevious);
            this.ConfigureEventStrategy("heartbeat", QueueStrategy.Throttle, 800);     // ~75 BPM heartbeat

            // System
            this.ConfigureEventStrategy("stop", QueueStrategy.Immediate);               // Emergency stop
        }

        public void ConfigureEventStrategy(String eventName, QueueStrategy strategy, Int32 throttleMs = 100)
        {
            this._eventStrategies[eventName] = strategy;
            if (strategy == QueueStrategy.Throttle)
            {
                this._throttleIntervals[eventName] = throttleMs;
            }
        }

        public void EnqueueEvent(String eventName)
        {
            if (String.IsNullOrEmpty(eventName))
            {
                PluginLog.Warning($"Attempted to enqueue null or empty event name");
                return;
            }

            // Get strategy for this event, default to CancelPrevious if not configured
            var strategy = this._eventStrategies.TryGetValue(eventName, out var s) ? s : QueueStrategy.CancelPrevious;

            switch (strategy)
            {
                case QueueStrategy.Immediate:
                    this.ExecuteImmediately(eventName);
                    break;

                case QueueStrategy.CancelPrevious:
                    this.ExecuteWithCancellation(eventName);
                    break;

                case QueueStrategy.Throttle:
                    this.ExecuteWithThrottle(eventName);
                    break;
            }
        }

        private void ExecuteImmediately(String eventName)
        {
            try
            {
                PluginLog.Info($"Executing immediate event: {eventName}");
                this._triggerHapticEvent(eventName);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error executing immediate event {eventName}: {ex.Message}");
            }
        }

        private void ExecuteWithCancellation(String eventName)
        {
            // Cancel any existing task for this event
            if (this._activeTasks.TryRemove(eventName, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
                PluginLog.Verbose($"Cancelled previous haptic event: {eventName}");
            }

            // Create new cancellation token and execute
            var cts = new CancellationTokenSource();
            this._activeTasks[eventName] = cts;

            Task.Run(() =>
            {
                try
                {
                    if (!cts.Token.IsCancellationRequested)
                    {
                        PluginLog.Info($"Executing cancel-previous event: {eventName}");
                        this._triggerHapticEvent(eventName);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Error executing cancel-previous event {eventName}: {ex.Message}");
                }
                finally
                {
                    // Clean up after execution
                    this._activeTasks.TryRemove(eventName, out _);
                    cts.Dispose();
                }
            }, cts.Token);
        }

        private void ExecuteWithThrottle(String eventName)
        {
            var now = DateTime.UtcNow;
            var interval = this._throttleIntervals.TryGetValue(eventName, out var ms) ? ms : 100;

            // Check if enough time has passed since last execution
            if (this._lastExecutionTimes.TryGetValue(eventName, out var lastTime))
            {
                var timeSinceLastExecution = (now - lastTime).TotalMilliseconds;
                if (timeSinceLastExecution < interval)
                {
                    PluginLog.Verbose($"Throttled event {eventName}: {timeSinceLastExecution}ms < {interval}ms");
                    return; // Skip this event
                }
            }

            // Update last execution time and execute
            this._lastExecutionTimes[eventName] = now;

            try
            {
                PluginLog.Info($"Executing throttled event: {eventName}");
                this._triggerHapticEvent(eventName);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error executing throttled event {eventName}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            // Cancel all active tasks
            foreach (var kvp in this._activeTasks)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
            this._activeTasks.Clear();

            this._disposed = true;
            PluginLog.Info("HapticEventQueueManager disposed");
        }
    }
}
