# VibeIt - Universal Haptic Bridge for Logitech MX Master 4

VibeIt is a lightweight Logitech plugin that provides haptic feedback control for the **Logitech MX Master 4 mouse** via WebSocket.

## 🎯 Features

- ✅ **WebSocket Server**: Fast, real-time haptic control (Binary + JSON protocols)
- ✅ **Lightweight**: Simple, minimal overhead implementation
- ✅ **Cross-Platform**: Works with JavaScript, Python, Node.js, C#, and more
- ✅ **Smart Event Queue**: Configurable strategies (Throttle, CancelPrevious, Immediate)
- ✅ **5 Preset Haptic Events**: Optimized for common UI interactions

## 📡 WebSocket Connection

**Server**: `ws://localhost:8765`

### Binary Protocol (single byte)
```javascript
const ws = new WebSocket('ws://localhost:8765');
ws.send(new Uint8Array([0x02])); // Sharp Click
```

### JSON Protocol
```javascript
const ws = new WebSocket('ws://localhost:8765');
ws.send(JSON.stringify({event: "sharp_click"}));
```


## 🎮 Available Events

| Event Name | Byte Code | Description | Use Case | Queue Strategy |
|------------|-----------|-------------|----------|----------------|
| `soft_bump` | `0x01` | สั่นเบาๆ | UI hover, menu navigation | Throttle (100ms) |
| `sharp_click` | `0x02` | คลิกกริ๊บ | Button press, success | Cancel-Previous |
| `double_click` | `0x03` | สั่น 2 ครั้ง | Alert, notification | Cancel-Previous |
| `long_pulse` | `0x04` | สั่นยาว | Warning, error | Cancel-Previous |
| `stop` | `0x00` | หยุดทันที | Emergency stop | Immediate |

## 🚀 Quick Start Examples

### JavaScript (Browser)

```javascript
// Connect to WebSocket
const ws = new WebSocket('ws://localhost:8765');

ws.onopen = () => {
  console.log('✅ Connected to VibeIt');
  
  // Send JSON protocol
  ws.send(JSON.stringify({event: 'sharp_click'}));
  
  // Or send Binary protocol
  ws.send(new Uint8Array([0x02])); // 0x02 = sharp_click
};

// Usage with UI events
document.querySelector('.button').addEventListener('click', () => {
  ws.send(JSON.stringify({event: 'sharp_click'}));
});

// Hover effect (automatically throttled to 100ms)
document.querySelectorAll('.menu-item').forEach(item => {
  item.addEventListener('mouseenter', () => {
    ws.send(JSON.stringify({event: 'soft_bump'}));
  });
});
```

### Python

```python
import asyncio
import websockets
import json

async def trigger_haptic(event_name):
    async with websockets.connect('ws://localhost:8765') as ws:
        # JSON protocol
        await ws.send(json.dumps({'event': event_name}))

# Usage
asyncio.run(trigger_haptic('sharp_click'))
```

### Node.js

```javascript
const WebSocket = require('ws');
const ws = new WebSocket('ws://localhost:8765');

ws.on('open', () => {
  // JSON protocol
  ws.send(JSON.stringify({event: 'sharp_click'}));
  
  // Binary protocol  
  ws.send(Buffer.from([0x02]));
});
```

## 🔧 Event Queue Strategies

VibeIt uses intelligent queue management to handle rapid events smoothly:

- **Throttle** (`soft_bump`): Limits to 10 haptic events per second for smooth hover effects
- **Cancel-Previous** (`sharp_click`, `double_click`, `long_pulse`): Cancels ongoing haptic to play new one immediately  
- **Immediate** (`stop`): Executes right away without queuing

This prevents overwhelming the haptic motor and provides responsive feedback!

## 📦 Installation

1. Install **Logi Options+** (version 1.95+)
2. Download VibeIt plugin from releases
3. Copy to Logi Plugin directory:
   - **macOS**: `~/Library/Application Support/Logi/LogiPluginService/Plugins/`
   - **Windows**: `%LocalAppData%\Logi\LogiPluginService\Plugins\`
4. Restart Logi Options+

## 🧪 Testing

Check `/examples` directory for:
- `test-all-events.html` - Interactive browser-based tester
- `browser-extension-example.js` - Chrome/Firefox extension snippet
- `python-client.py` - Python example with all protocols
- `nodejs-client.js` - Node.js integration example

## ❓ Troubleshooting

**Plugin not loading?**
- Check Logi Options+ version (requires 1.95+ for haptic support)
- Verify MX Master 4 is connected and recognized

**WebSocket/HTTP not responding?**
- Check if ports 8765 and 8766 are available
- Look at plugin logs in Logi Options+ settings

**Haptic not triggering?**
- Ensure MX Master 4 has haptic support enabled in Logi Options+
- Test with the HTML tester first to verify plugin is working

## 📄 License

MIT License - Copyright © 2026 dulyawat

## 🙏 Credits

Built with [Logitech Actions SDK](https://logitech.github.io/actions-sdk-docs/)
