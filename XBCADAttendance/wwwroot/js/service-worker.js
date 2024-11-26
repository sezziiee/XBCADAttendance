self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open('static-cache').then(function (cache) {
            return cache.addAll([
                '/', // Home page
                '/index.html',
                '/styles.css',
                '/script.js',
                '/manifest.json',
                '/icons/instagram (1).png',
                '/icons/instagram.png'
            ]);
        })
    );
});

// Fetch event to handle offline mode and cache QR codes
self.addEventListener('fetch', function (event) {
    const requestUrl = new URL(event.request.url);

    // If the request is for a QR code image
    if (requestUrl.pathname.startsWith('/qrcodes/')) {
        event.respondWith(
            caches.open('qr-code-cache').then(function (cache) {
                return cache.match(event.request).then(function (cachedResponse) {
                    return cachedResponse || fetch(event.request).then(function (networkResponse) {
                        // Cache the QR code for future use
                        cache.put(event.request, networkResponse.clone());
                        return networkResponse;
                    });
                });
            })
        );
    } else {
        // Default caching for static assets
        event.respondWith(
            caches.match(event.request).then(function (response) {
                return response || fetch(event.request);
            })
        );
    }
});

