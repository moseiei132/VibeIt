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


## 🎮 Available Events (21 Total)

### UI Interactions
| Event | Byte | Use Case | Strategy |
|-------|------|----------|----------|
| `hover` | 0x01 | Menu/UI hover | Throttle (100ms) |
| `click` | 0x02 | Button press | CancelPrevious |
| `double_click` | 0x03 | Special action | CancelPrevious |
| `drag_start` | 0x04 | Begin drag | CancelPrevious |
| `drag_end` | 0x05 | Drop item | CancelPrevious |
| `scroll_tick` | 0x06 | Scroll feedback | Throttle (50ms) |
| `select` | 0x07 | Selection | CancelPrevious |

### Notifications
| Event | Byte | Use Case | Strategy |
|-------|------|----------|----------|
| `success` | 0x08 | Success feedback | CancelPrevious |
| `error` | 0x09 | Error alert | CancelPrevious |
| `warning` | 0x0A | Warning | CancelPrevious |
| `info` | 0x0B | Information | CancelPrevious |
| `completed` | 0x0C | Task done | CancelPrevious |

### Gaming & Interactive
| Event | Byte | Use Case | Strategy |
|-------|------|----------|----------|
| `hit_light` | 0x0D | Light impact | Throttle (80ms) |
| `hit_heavy` | 0x0E | Heavy impact | CancelPrevious |
| `damage` | 0x0F | Take damage | CancelPrevious |
| `pickup` | 0x10 | Collect item | Throttle (100ms) |
| `level_up` | 0x11 | Achievement | CancelPrevious |

### Creative & Special
| Event | Byte | Use Case | Strategy |
|-------|------|----------|----------|
| `pulse` | 0x12 | Pulsing effect | CancelPrevious |
| `wave` | 0x13 | Wave pattern | CancelPrevious |
| `firework` | 0x14 | Celebration | CancelPrevious |
| `heartbeat` | 0x15 | Rhythm (~75 BPM) | Throttle (800ms) |

### System
| Event | Byte | Use Case | Strategy |
|-------|------|----------|----------|
| `stop` | 0x00 | Emergency stop | Immediate |

> 📖 **Full Documentation**: See [EVENTS.md](EVENTS.md) for detailed waveform mappings and examples

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
