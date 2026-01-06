# VibeIt - Haptic Events Quick Reference

## 🎯 21 Haptic Events

### UI Interactions (7 events)
| Event | Byte | Waveform | Strategy | Use Case |
|-------|------|----------|----------|----------|
| `hover` | 0x01 | subtle_collision | Throttle (100ms) | Menu hover, UI element hover |
| `click` | 0x02 | sharp_state_change | CancelPrevious | Button press, link click |
| `double_click` | 0x03 | knock | CancelPrevious | Special actions, shortcuts |
| `drag_start` | 0x04 | damp_state_change | CancelPrevious | Begin drag operation |
| `drag_end` | 0x05 | sharp_collision | CancelPrevious | Drop item |
| `scroll_tick` | 0x06 | subtle_collision | Throttle (50ms) | Scroll wheel feedback |
| `select` | 0x07 | damp_state_change | CancelPrevious | Selection feedback |

### Notifications (5 events)
| Event | Byte | Waveform | Strategy | Use Case |
|-------|------|----------|----------|----------|
| `success` | 0x08 | happy_alert | CancelPrevious | Operation successful |
| `error` | 0x09 | angry_alert | CancelPrevious | Error occurred |
| `warning` | 0x0A | angry_alert | CancelPrevious | Warning message |
| `info` | 0x0B | damp_collision | CancelPrevious | Information notification |
| `completed` | 0x0C | completed | CancelPrevious | Task completed |

### Gaming & Interactive (5 events)
| Event | Byte | Waveform | Strategy | Use Case |
|-------|------|----------|----------|----------|
| `hit_light` | 0x0D | subtle_collision | Throttle (80ms) | Light sword hit, tap |
| `hit_heavy` | 0x0E | sharp_collision | CancelPrevious | Heavy weapon hit |
| `damage` | 0x0F | mad | CancelPrevious | Taking damage |
| `pickup` | 0x10 | jingle | Throttle (100ms) | Collect item, coin |
| `level_up` | 0x11 | firework | CancelPrevious | Achievement unlocked |

### Creative & Special (4 events)
| Event | Byte | Waveform | Strategy | Use Case |
|-------|------|----------|----------|----------|
| `pulse` | 0x12 | wave | CancelPrevious | Breathing effect, pulse |
| `wave` | 0x13 | wave | CancelPrevious | Smooth wave pattern |
| `firework` | 0x14 | firework | CancelPrevious | Celebration, burst |
| `heartbeat` | 0x15 | knock | Throttle (800ms) | ~75 BPM rhythm |

### System (1 event)
| Event | Byte | Waveform | Strategy | Use Case |
|-------|------|----------|----------|----------|
| `stop` | 0x00 | - | Immediate | Emergency stop |

## 📊 Queue Strategies Explained

- **Throttle**: Limits events to a specific rate (prevents overwhelming)
- **CancelPrevious**: Stops current haptic and plays new one (responsive)
- **Immediate**: No queuing, executes right away (emergency)

## 🎮 Usage Examples

### WebSocket Binary Protocol
```javascript
ws.send(new Uint8Array([0x02])); // click
ws.send(new Uint8Array([0x08])); // success
ws.send(new Uint8Array([0x14])); // firework
```

### WebSocket JSON Protocol
```javascript
ws.send(JSON.stringify({event: 'click'}));
ws.send(JSON.stringify({event: 'success'}));
ws.send(JSON.stringify({event: 'firework'}));
```

## 🎨 Use Case Examples

**Web App UI:**
- Hover: `hover` (0x01)
- Click: `click` (0x02)
- Form Submit Success: `success` (0x08)
- Form Error: `error` (0x09)

**Game:**
- Light Attack: `hit_light` (0x0D)
- Heavy Attack: `hit_heavy` (0x0E)
- Take Damage: `damage` (0x0F)
- Collect Coin: `pickup` (0x10)
- Level Up: `level_up` (0x11)

**Creative App:**
- Brush Stroke: `scroll_tick` (0x06)
- Select Layer: `select` (0x07)
- Save Complete: `completed` (0x0C)

**Music App:**
- Beat: `heartbeat` (0x15) @ 75 BPM
- Drop: `firework` (0x14)
- Fade: `wave` (0x13)
