#!/usr/bin/env python3
"""
VibeIt Python Client Example
Demonstrates HTTP and WebSocket integration with VibeIt haptic bridge
"""

import requests
import asyncio
import websockets
import json
import time

# Configuration
HTTP_API_URL = 'http://localhost:8766/trigger'
WEBSOCKET_URL = 'ws://localhost:8765'

# Event byte codes for binary protocol
EVENT_CODES = {
    'stop': 0x00,
    'soft_bump': 0x01,
    'sharp_click': 0x02,
    'double_click': 0x03,
    'long_pulse': 0x04
}

class VibeItClient:
    """Simple HTTP-based haptic client"""
    
    def __init__(self, api_url=HTTP_API_URL):
        self.api_url = api_url
        self.session = requests.Session()
        self.is_connected = self._test_connection()
    
    def _test_connection(self):
        """Test if VibeIt is available"""
        try:
            response = self.trigger('soft_bump')
            return response.get('success', False)
        except Exception as e:
            print(f"⚠️ VibeIt not available: {e}")
            return False
    
    def trigger(self, event_name):
        """Trigger a haptic event via HTTP"""
        try:
            response = self.session.post(
                self.api_url,
                json={'event': event_name},
                timeout=1
            )
            return response.json()
        except Exception as e:
            print(f"❌ Failed to trigger {event_name}: {e}")
            return {'success': False, 'error': str(e)}
    
    # Convenience methods
    def soft_bump(self):
        return self.trigger('soft_bump')
    
    def sharp_click(self):
        return self.trigger('sharp_click')
    
    def double_click(self):
        return self.trigger('double_click')
    
    def long_pulse(self):
        return self.trigger('long_pulse')
    
    def stop(self):
        return self.trigger('stop')


class VibeItWebSocketClient:
    """WebSocket-based haptic client for real-time applications"""
    
    def __init__(self, ws_url=WEBSOCKET_URL):
        self.ws_url = ws_url
        self.ws = None
    
    async def connect(self):
        """Connect to WebSocket server"""
        self.ws = await websockets.connect(self.ws_url)
        print(f"✅ Connected to {self.ws_url}")
    
    async def trigger_binary(self, event_name):
        """Send haptic event using binary protocol (single byte)"""
        if event_name not in EVENT_CODES:
            raise ValueError(f"Invalid event: {event_name}")
        
        event_byte = EVENT_CODES[event_name]
        await self.ws.send(bytes([event_byte]))
        print(f"→ Sent binary: {event_name} (0x{event_byte:02X})")
    
    async def trigger_json(self, event_name):
        """Send haptic event using JSON protocol"""
        message = json.dumps({'event': event_name})
        await self.ws.send(message)
        print(f"→ Sent JSON: {message}")
    
    async def close(self):
        """Close WebSocket connection"""
        if self.ws:
            await self.ws.close()
            print("🔌 WebSocket closed")


# Example Usage Functions

def example_http_basic():
    """Basic HTTP usage example"""
    print("\n=== HTTP Basic Example ===")
    client = VibeItClient()
    
    if not client.is_connected:
        print("⚠️ VibeIt not running, please start the plugin")
        return
    
    # Trigger different events
    print("Testing all events...")
    client.soft_bump()
    time.sleep(0.2)
    client.sharp_click()
    time.sleep(0.2)
    client.double_click()
    time.sleep(0.2)
    client.long_pulse()
    print("✅ All events sent")


async def example_websocket_binary():
    """WebSocket binary protocol example"""
    print("\n=== WebSocket Binary Example ===")
    client = VibeItWebSocketClient()
    
    try:
        await client.connect()
        
        # Send events using binary protocol
        await client.trigger_binary('soft_bump')
        await asyncio.sleep(0.2)
        await client.trigger_binary('sharp_click')
        await asyncio.sleep(0.2)
        await client.trigger_binary('double_click')
        
    finally:
        await client.close()


async def example_websocket_json():
    """WebSocket JSON protocol example"""
    print("\n=== WebSocket JSON Example ===")
    client = VibeItWebSocketClient()
    
    try:
        await client.connect()
        
        # Send events using JSON protocol  
        await client.trigger_json('sharp_click')
        await asyncio.sleep(0.2)
        await client.trigger_json('long_pulse')
        
    finally:
        await client.close()


def example_rapid_hover():
    """Simulate rapid menu hover (throttled by VibeIt)"""
    print("\n=== Rapid Hover Simulation ===")
    client = VibeItClient()
    
    if not client.is_connected:
        return
    
    print("Simulating rapid menu hover (20 items, throttled to 10 Hz)...")
    for i in range(20):
        client.soft_bump()
        time.sleep(0.05)  # 50ms between hovers
    print("✅ Hover test complete (should have felt ~10 haptics)")


def example_ui_workflow():
    """Simulate a UI workflow with different haptic events"""
    print("\n=== UI Workflow Example ===")
    client = VibeItClient()
    
    if not client.is_connected:
        return
    
    print("Simulating UI workflow:")
    
    print("  1. Hover over menu item...")
    client.soft_bump()
    time.sleep(0.5)
    
    print("  2. Click button...")
    client.sharp_click()
    time.sleep(0.5)
    
    print("  3. Show notification...")
    client.double_click()
    time.sleep(0.5)
    
    print("  4. Warning message...")
    client.long_pulse()
    time.sleep(0.5)
    
    print("✅ Workflow complete")


if __name__ == '__main__':
    print("🎮 VibeIt Python Client Examples\n")
    
    # Run HTTP examples
    example_http_basic()
    example_rapid_hover()
    example_ui_workflow()
    
    # Run WebSocket examples (async)
    asyncio.run(example_websocket_binary())
    asyncio.run(example_websocket_json())
    
    print("\n✨ All examples completed!")
