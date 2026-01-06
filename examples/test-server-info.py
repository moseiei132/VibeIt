#!/usr/bin/env python3
"""Test VibeIt Server Info Command"""

import asyncio
import websockets
import json

async def test_server_info():
    uri = 'ws://localhost:8765'
    
    try:
        async with websockets.connect(uri) as ws:
            print('✅ Connected to VibeIt\n')
            
            # Request server info
            print('→ Requesting server info...')
            await ws.send(json.dumps({'command': 'info'}))
            
            # Receive response
            response = await ws.recv()
            info = json.loads(response)
            
            print('← Received server info:\n')
            print(f"Server: {info['server']} v{info['version']}")
            print(f"Type: {info['type']}")
            print(f"Port: {info['port']}")
            print(f"Protocols: {', '.join(info['protocols'])}")
            print(f"Features: {', '.join(info['features'])}")
            print(f"\nTotal Events: {info['eventCount']}")
            
            # Group by category
            categories = {}
            for event in info['events']:
                cat = event['category']
                if cat not in categories:
                    categories[cat] = []
                categories[cat].append(event)
            
            print('\nAvailable Events by Category:')
            for category, events in categories.items():
                print(f"\n{category.upper()}:")
                for e in events:
                    print(f"  {e['code']} → {e['name']}")
            
            print('\n✅ Test complete')
            
    except Exception as e:
        print(f'❌ Error: {e}')

if __name__ == '__main__':
    asyncio.run(test_server_info())
