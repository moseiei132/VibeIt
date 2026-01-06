/**
 * VibeIt Node.js Client Example
 * Demonstrates HTTP and WebSocket integration for haptic feedback
 */

const WebSocket = require('ws');
const fetch = require('node-fetch'); // or use built-in fetch in Node 18+

// Configuration
const HTTP_API_URL = 'http://localhost:8766/trigger';
const WEBSOCKET_URL = 'ws://localhost:8765';

// Event byte codes for binary protocol
const EVENT_CODES = {
    stop: 0x00,
    soft_bump: 0x01,
    sharp_click: 0x02,
    double_click: 0x03,
    long_pulse: 0x04
};

class VibeItClient {
    constructor(apiUrl = HTTP_API_URL) {
        this.apiUrl = apiUrl;
        this.isConnected = false;
        this.testConnection();
    }

    async testConnection() {
        try {
            const response = await fetch(this.apiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ event: 'soft_bump' })
            });
            this.isConnected = response.ok;
            console.log('VibeIt connection:', this.isConnected ? '✅ Connected' : '❌ Failed');
        } catch (error) {
            this.isConnected = false;
            console.warn('⚠️ VibeIt not available:', error.message);
        }
    }

    async trigger(eventName) {
        if (!this.isConnected) {
            console.warn('VibeIt not connected');
            return { success: false };
        }

        try {
            const response = await fetch(this.apiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ event: eventName })
            });
            return await response.json();
        } catch (error) {
            console.error('Trigger failed:', error);
            return { success: false, error: error.message };
        }
    }

    // Convenience methods
    softBump() { return this.trigger('soft_bump'); }
    sharpClick() { return this.trigger('sharp_click'); }
    doubleClick() { return this.trigger('double_click'); }
    longPulse() { return this.trigger('long_pulse'); }
    stop() { return this.trigger('stop'); }
}

class VibeItWebSocketClient {
    constructor(wsUrl = WEBSOCKET_URL) {
        this.wsUrl = wsUrl;
        this.ws = null;
    }

    connect() {
        return new Promise((resolve, reject) => {
            this.ws = new WebSocket(this.wsUrl);

            this.ws.on('open', () => {
                console.log(`✅ WebSocket connected to ${this.wsUrl}`);
                resolve();
            });

            this.ws.on('error', (error) => {
                console.error('WebSocket error:', error);
                reject(error);
            });

            this.ws.on('close', () => {
                console.log('🔌 WebSocket closed');
            });
        });
    }

    async triggerBinary(eventName) {
        if (!EVENT_CODES.hasOwnProperty(eventName)) {
            throw new Error(`Invalid event: ${eventName}`);
        }

        const eventByte = EVENT_CODES[eventName];
        this.ws.send(Buffer.from([eventByte]));
        console.log(`→ Sent binary: ${eventName} (0x${eventByte.toString(16).padStart(2, '0')})`);
    }

    async triggerJSON(eventName) {
        const message = JSON.stringify({ event: eventName });
        this.ws.send(message);
        console.log(`→ Sent JSON: ${message}`);
    }

    close() {
        if (this.ws) {
            this.ws.close();
        }
    }
}

// Example Usage Functions

async function exampleHTTPBasic() {
    console.log('\n=== HTTP Basic Example ===');
    const client = new VibeItClient();

    // Wait for connection test
    await new Promise(resolve => setTimeout(resolve, 100));

    if (!client.isConnected) {
        console.log('⚠️ VibeIt not running');
        return;
    }

    // Trigger different events
    console.log('Testing all events...');
    await client.softBump();
    await sleep(200);
    await client.sharpClick();
    await sleep(200);
    await client.doubleClick();
    await sleep(200);
    await client.longPulse();
    console.log('✅ All events sent');
}

async function exampleWebSocketBinary() {
    console.log('\n=== WebSocket Binary Example ===');
    const client = new VibeItWebSocketClient();

    try {
        await client.connect();

        // Send events using binary protocol
        await client.triggerBinary('soft_bump');
        await sleep(200);
        await client.triggerBinary('sharp_click');
        await sleep(200);
        await client.triggerBinary('double_click');

        client.close();
    } catch (error) {
        console.error('WebSocket example failed:', error);
    }
}

async function exampleWebSocketJSON() {
    console.log('\n=== WebSocket JSON Example ===');
    const client = new VibeItWebSocketClient();

    try {
        await client.connect();

        // Send events using JSON protocol
        await client.triggerJSON('sharp_click');
        await sleep(200);
        await client.triggerJSON('long_pulse');

        client.close();
    } catch (error) {
        console.error('WebSocket JSON example failed:', error);
    }
}

async function exampleExpressIntegration() {
    console.log('\n=== Express.js Integration Example ===');
    const client = new VibeItClient();

    // Simulated Express route handlers
    const routes = {
        onButtonClick: async (req, res) => {
            await client.sharpClick();
            res.json({ success: true });
        },

        onFormSubmit: async (req, res) => {
            // ... validate form
            await client.doubleClick(); // Success notification
            res.json({ success: true });
        },

        onError: async (err, req, res, next) => {
            await client.longPulse(); // Error warning
            res.status(500).json({ error: err.message });
        }
    };

    console.log('✅ Express routes configured with haptic feedback');
}

async function exampleSocketIOIntegration() {
    console.log('\n=== Socket.IO Integration Example ===');

    // Simulated Socket.IO setup
    console.log('Setting up Socket.IO haptic feedback...');

    const haptic = new VibeItClient();

    // io.on('connection', (socket) => {
    //     socket.on('user:message', async () => {
    //         await haptic.softBump(); // New message indicator
    //     });
    //     
    //     socket.on('user:joined', async () => {
    //         await haptic.doubleClick(); // User joined notification
    //     });
    //     
    //     socket.on('error', async () => {
    //         await haptic.longPulse(); // Error warning
    //     });
    // });

    console.log('✅ Socket.IO haptic integration ready');
}

// Utility function
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// Run examples
async function main() {
    console.log('🎮 VibeIt Node.js Client Examples\n');

    try {
        await exampleHTTPBasic();
        await exampleWebSocketBinary();
        await exampleWebSocketJSON();
        await exampleExpressIntegration();
        await exampleSocketIOIntegration();

        console.log('\n✨ All examples completed!');
    } catch (error) {
        console.error('Example failed:', error);
    }

    process.exit(0);
}

// Export for use as module
module.exports = {
    VibeItClient,
    VibeItWebSocketClient,
    EVENT_CODES
};

// Run if called directly
if (require.main === module) {
    main();
}
