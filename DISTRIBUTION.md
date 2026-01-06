# VibeIt Plugin Distribution Guide

## 📦 Creating Distribution Package

### Prerequisites
- Logitech Plugin Tool (`logiplugintool`) installed
- Plugin built in **Release** mode

### Step 1: Build Release Version
```bash
dotnet build src/VibeItPlugin.csproj -c Release
```

This creates the release build in `bin/Release/`

### Step 2: Create .lplug4 Package
```bash
# Create build folder if it doesn't exist
mkdir -p build

# Pack plugin to build folder
logiplugintool pack ./bin/Release/ ./build/VibeIt_1.0.lplug4
```

### Step 3: Verify Package
```bash
logiplugintool verify ./build/VibeIt_1.0.lplug4
```

### Step 4: Test Installation
Double-click `build/VibeIt_1.0.lplug4` to install and test

## 🚀 Publishing to Marketplace

### Before Submission Checklist
- [x] Plugin tested with MX Master 4 hardware
- [x] All 21 haptic events working correctly
- [x] WebSocket server tested (binary + JSON protocols)
- [x] Server info command tested
- [x] Icon present in `metadata/` folder (Icon256x256.png)
- [x] README.md with clear documentation
- [x] GitHub repository published
- [x] MIT license included

### Metadata Requirements
- ✅ **Name**: VibeIt
- ✅ **Version**: 1.0
- ✅ **License**: MIT
- ✅ **Support URL**: https://github.com/moseiei132/VibeIt/issues
- ✅ **Homepage**: https://github.com/moseiei132/VibeIt
- ✅ **Min Version**: 6.2.1 (for haptic support)

### Submit Plugin
1. Go to https://marketplace.logitech.com/contribute
2. Upload `VibeIt_1.0.lplug4`
3. Fill in marketplace details
4. Submit for review

## 📋 Package Contents

The .lplug4 file includes:
- `bin/` - Plugin DLL and dependencies
- `metadata/` - LoupedeckPackage.yaml, Icon
- `events/` - Event definitions and waveform mappings

## 🛠️ logiplugintool Installation

If you don't have logiplugintool installed:

### macOS
```bash
# Download from Logitech SDK
# Or check if it's included with Logi Options+
which logiplugintool
```

### Alternative: Manual ZIP Creation
If logiplugintool is not available, you can create manually:
1. Ensure proper folder structure
2. ZIP the Release folder
3. Rename .zip to .lplug4

**Note**: Using logiplugintool is recommended as it validates the package format.

## 📝 Marketplace Guidelines

Follow [Marketplace Approval Guidelines](https://logitech.github.io/actions-sdk-docs/csharp/Marketplace-Approval-Guidelines/):
- Clear, descriptive plugin name
- Professional icon (256x256 PNG)
- Comprehensive description
- Working support/homepage URLs
- Compatible license (GPL not allowed)
- Proper versioning
- No malicious code
- Respect user privacy

## 🔍 Testing Checklist

Before uploading:
- [ ] Install .lplug4 on clean system
- [ ] Test all 21 events
- [ ] Test WebSocket connection
- [ ] Test info command
- [ ] Check plugin appears in Logi Options+
- [ ] Verify no errors in logs
- [ ] Test with HTML tester
- [ ] Verify haptic feedback feels appropriate

## 📊 Version History

### v1.0 (Initial Release)
- 21 haptic events across 4 categories
- WebSocket server (port 8765)
- Binary (0x00-0x15) and JSON protocols
- Server info command
- Smart event queue (throttle, cancel-previous, immediate)
- Cross-platform client examples
