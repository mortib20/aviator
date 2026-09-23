'use strict';

// Minutes behind UTC (JS convention) — MainLayout converts it to a UTC offset
window.getTimezoneOffset = () => new Date().getTimezoneOffset();

// Scroll helpers for ACARS page
window.isAtTop = function (element) {
    return element.scrollTop < 100;
};

window.scrollToTop = function (element) {
    element.scrollTo({ top: 0, behavior: 'smooth' });
};

// Registrations come in over the air — keep the cache bounded on long sessions
const _psCache = new Map();
const PS_CACHE_MAX = 1000;

window.fetchPlanespottersPhoto = async function (registration) {
    if (_psCache.has(registration)) return _psCache.get(registration);

    let result = null;
    try {
        const res = await fetch(
            `https://api.planespotters.net/pub/photos/reg/${encodeURIComponent(registration)}`
        );
        if (res.ok) {
            const photo = (await res.json()).photos?.[0];
            if (photo) {
                result = {
                    url: photo.thumbnail_large?.src ?? photo.thumbnail?.src ?? null,
                    link: photo.link ?? null,
                    photographer: photo.photographer ?? null
                };
            }
        }
    } catch {
        // Network error / blocked — fall back to placeholder
    }

    if (_psCache.size >= PS_CACHE_MAX) _psCache.delete(_psCache.keys().next().value);
    _psCache.set(registration, result);
    return result;
};
