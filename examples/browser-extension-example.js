// VibeIt Browser Extension Example
// For Chrome/Firefox extensions to trigger haptic feedback

class VibeItClient {
    constructor(apiUrl = 'http://localhost:8766/trigger') {
        this.apiUrl = apiUrl;
        this.isConnected = false;
        this.testConnection();
    }
    
    async testConnection() {
        try {
            const response = await fetch(this.apiUrl, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({event: 'soft_bump'})
            });
            this.isConnected = response.ok;
            console.log('VibeIt connection:', this.isConnected ? '✅ Connected' : '❌ Failed');
        } catch (error) {
            this.isConnected = false;
            console.warn('VibeIt not available:', error.message);
        }
    }
    
    async trigger(eventName) {
        if (!this.isConnected) {
            console.warn('VibeIt not connected, skipping haptic');
            return false;
        }
        
        try {
            const response = await fetch(this.apiUrl, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({event: eventName})
            });
            
            const data = await response.json();
            return data.success;
        } catch (error) {
            console.error('VibeIt trigger failed:', error);
            this.isConnected = false;
            return false;
        }
    }
    
    // Convenience methods
    softBump() { return this.trigger('soft_bump'); }
    sharpClick() { return this.trigger('sharp_click'); }
    doubleClick() { return this.trigger('double_click'); }
    longPulse() { return this.trigger('long_pulse'); }
    stop() { return this.trigger('stop'); }
}

// Example Usage:

// Initialize client
const haptic = new VibeItClient();

// 1. Button Click Feedback
document.querySelectorAll('button').forEach(button => {
    button.addEventListener('click', () => {
        haptic.sharpClick();
    });
});

// 2. Menu Hover Feedback (with rapid hover support)
document.querySelectorAll('.menu-item, [role="menuitem"]').forEach(item => {
    item.addEventListener('mouseenter', () => {
        haptic.softBump(); // Automatically throttled to 100ms
    });
});

// 3. Form Submission Success
document.querySelector('form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    // ... your form logic
    await haptic.doubleClick(); // Success notification
});

// 4. Error Alerts
function showError(message) {
    haptic.longPulse(); // Warning vibration
    alert(message);
}

// 5. Notification Badge
function showNotification(count) {
    if (count > 0) {
        haptic.doubleClick();
    }
}

// 6. Tab Switch Feedback
chrome.tabs?.onActivated?.addListener(() => {
    haptic.softBump();
});

// 7. Dropdown Menu
document.querySelector('select')?.addEventListener('change', () => {
    haptic.sharpClick();
});

// 8. Checkbox/Radio Toggle
document.querySelectorAll('input[type="checkbox"], input[type="radio"]').forEach(input => {
    input.addEventListener('change', () => {
        haptic.sharpClick();
    });
});

// 9. Accordion Expand/Collapse
document.querySelectorAll('.accordion-header').forEach(header => {
    header.addEventListener('click', () => {
        haptic.sharpClick();
    });
});

// 10. Drag and Drop
document.addEventListener('dragstart', () => haptic.softBump());
document.addEventListener('drop', () => haptic.sharpClick());

console.log('VibeIt haptic feedback integration loaded! 🎮');
