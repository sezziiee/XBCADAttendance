self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open('static-cache').then(function (cache) {
            return cache.addAll([
                '/',
                '/index.html',
                '/styles.css',
                '/Admin.css',
                '/Lecturer.css',
                '/navbar.css',
                '/script.js',
                '/manifest.json',
                '/img/Attendance_resized.png',
                '/img/Attendance.png'
            ]);
        })
    );
});

self.addEventListener('fetch', function (event) {
    event.respondWith(
        caches.match(event.request).then(function (response) {
            return response || fetch(event.request);
        })
    );
});
