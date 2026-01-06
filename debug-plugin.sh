#!/bin/bash

echo "🔍 VibeIt Plugin Debug Tool"
echo "================================"

# 1. Check plugin link
echo -e "\n1. Plugin Link Status:"
if [ -f ~/Library/Application\ Support/Logi/LogiPluginService/Plugins/VibeItPlugin.link ]; then
    echo "✅ Plugin link exists"
    echo "   Path: $(cat ~/Library/Application\ Support/Logi/LogiPluginService/Plugins/VibeItPlugin.link)"
else
    echo "❌ Plugin link NOT found"
    echo "   Run: dotnet build src/VibeItPlugin.csproj"
fi

# 2. Check DLL
echo -e "\n2. Plugin DLL Status:"
if [ -f bin/Debug/bin/VibeItPlugin.dll ]; then
    echo "✅ VibeItPlugin.dll exists"
    ls -lh bin/Debug/bin/VibeItPlugin.dll
else
    echo "❌ DLL NOT found - build failed?"
fi

# 3. Check if LogiPluginService is running
echo -e "\n3. Logi Plugin Service Status:"
if pgrep -x "LogiPluginService" > /dev/null; then
    echo "✅ LogiPluginService is running (PID: $(pgrep LogiPluginService))"
else
    echo "❌ LogiPluginService NOT running"
    echo "   Please start Logi Options+"
fi

# 4. Test HTTP server
echo -e "\n4. HTTP Server Status (Port 8766):"
if nc -z localhost 8766 2>/dev/null; then
    echo "✅ HTTP server is listening on port 8766"
    
    echo -e "\n   Testing API..."
    response=$(curl -s -X POST http://localhost:8766/trigger \
        -H "Content-Type: application/json" \
        -d '{"event":"sharp_click"}')
    echo "   Response: $response"
else
    echo "❌ Port 8766 NOT open - plugin may not be loaded"
fi

# 5. Test WebSocket server
echo -e "\n5. WebSocket Server Status (Port 8765):"
if nc -z localhost 8765 2>/dev/null; then
    echo "✅ WebSocket server is listening on port 8765"
else
    echo "❌ Port 8765 NOT open - plugin may not be loaded"
fi

# 6. Show recent logs
echo -e "\n6. Recent Plugin Logs:"
echo "   Opening Console.app..."
echo "   Filter by: process:LogiPluginService OR message:VibeIt"

# Try to get logs (macOS 12+)
if command -v log &> /dev/null; then
    echo -e "\n   Last 10 VibeIt logs:"
    log show --predicate 'process == "LogiPluginService"' --last 5m --info 2>/dev/null | grep -i vibeIt | tail -10 || echo "   (No logs found - check Console.app manually)"
fi

echo -e "\n================================"
echo "🔧 Troubleshooting Tips:"
echo "  1. If plugin link exists but ports are closed:"
echo "     → Restart LogiPluginService: killall LogiPluginService"
echo ""
echo "  2. If DLL doesn't exist:"
echo "     → Rebuild: dotnet build src/VibeItPlugin.csproj"
echo ""
echo "  3. Check Console.app for detailed logs:"
echo "     → open -a Console"
echo ""
echo "  4. Test with HTML tester:"
echo "     → open examples/test-all-events.html"
