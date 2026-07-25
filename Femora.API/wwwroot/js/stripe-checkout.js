// stripe-checkout.js
// Usage example (vanilla JS):
// const payload = { courseId: null, successUrl: 'https://your-site/success', cancelUrl: 'https://your-site/cancel' };
// startCheckout('/api/payments/checkout', payload, 'pk_test_...');

function startCheckout(apiUrl, payload, publishableKey) {
    // POST to server to create Stripe session
    return fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(payload),
        credentials: 'include'
    })
    .then(async resp => {
        if (!resp.ok) throw new Error('Create checkout session failed');
        return resp.json();
    })
    .then(result => {
        // If server returned a hosted session URL, navigate directly
        if (result.sessionUrl) {
            window.location.href = result.sessionUrl;
            return;
        }

        // Otherwise, use Stripe.js client redirect with sessionId
        if (publishableKey && result.sessionId) {
            const stripe = Stripe(publishableKey);
            stripe.redirectToCheckout({ sessionId: result.sessionId });
            return;
        }

        // Fallback: if sessionId present but no publishable key, try server-provided URL
        console.error('Missing publishable key for Stripe.js redirect. Result:', result);
        alert('Unable to start Stripe checkout.');
    })
    .catch(err => {
        console.error(err);
        alert('Failed to initiate payment. Please try again.');
        throw err;
    });
}

// Expose globally
window.startCheckout = startCheckout;
