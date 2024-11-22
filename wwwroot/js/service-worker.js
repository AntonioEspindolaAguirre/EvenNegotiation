const CACHE_NAME = 'v1_admin_shell_cache';
const urlsToCache = [
    '/Admin/Index',
    '/Admin/Eventos',
    '/assets/vendors/feather/feather.css',
    '/assets/vendors/mdi/css/materialdesignicons.min.css',
    '/assets/vendors/font-awesome/css/font-awesome.min.css',
    '/assets/css/style.css',
    '/assets/vendors/js/vendor.bundle.base.js',
    '/img/favicon.png',
    '/img/24px.png'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            console.log('Cache abierta');
            return cache.addAll(urlsToCache);
        })
    );
});

self.addEventListener('fetch', event => {
    const dynamicRoutes = ['/Admin/GetEventos', '/Admin/GetUsuarios'];
    if (dynamicRoutes.some(route => event.request.url.includes(route))) {
        return event.respondWith(fetch(event.request));
    }

    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});
