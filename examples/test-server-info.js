#!/usr/bin/env node

// Test VibeIt Server Info Command
const WebSocket = require('ws');

const ws = new WebSocket('ws://localhost:8765');

ws.on('open', () => {
    console.log('✅ Connected to VibeIt\n');

    // Request server info
    console.log('→ Requesting server info...');
    ws.send(JSON.stringify({ command: 'info' }));
});

ws.on('message', (data) => {
    console.log('← Received server info:\n');

    const info = JSON.parse(data.toString());

    console.log(`Server: ${info.server} v${info.version}`);
    console.log(`Type: ${info.type}`);
    console.log(`Port: ${info.port}`);
    console.log(`Protocols: ${info.protocols.join(', ')}`);
    console.log(`Features: ${info.features.join(', ')}`);
    console.log(`\nTotal Events: ${info.eventCount}`);

    // Group by category
    const categories = {};
    info.events.forEach(event => {
        if (!categories[event.category]) {
            categories[event.category] = [];
        }
        categories[event.category].push(event);
    });

    console.log('\nAvailable Events by Category:');
    for (const [category, events] of Object.entries(categories)) {
        console.log(`\n${category.toUpperCase()}:`);
        events.forEach(e => {
            console.log(`  ${e.code} → ${e.name}`);
        });
    }

    // Close connection
    setTimeout(() => {
        ws.close();
        console.log('\n✅ Test complete');
    }, 100);
});

ws.on('error', (error) => {
    console.error('❌ Connection error:', error.message);
    process.exit(1);
});

ws.on('close', () => {
    console.log('🔌 Disconnected');
});
