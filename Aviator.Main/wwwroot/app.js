// Scroll helpers for ACARS page
window.isAtTop = function (element) {
    return element.scrollTop < 100;
};

window.scrollToTop = function (element) {
    element.scrollTop = 0;
};

const _psCache = {};

window.fetchPlanespottersPhoto = async function (registration) {
    if (_psCache[registration] !== undefined) return _psCache[registration];
    try {
        const res = await fetch(
            `https://api.planespotters.net/pub/photos/reg/${encodeURIComponent(registration)}`
        );
        if (!res.ok) { _psCache[registration] = null; return null; }
        const data = await res.json();
        const photo = data.photos?.[0];
        if (!photo) { _psCache[registration] = null; return null; }
        const result = {
            url: photo.thumbnail_large?.src ?? photo.thumbnail?.src ?? null,
            link: photo.link ?? null,
            photographer: photo.photographer ?? null
        };
        _psCache[registration] = result;
        return result;
    } catch {
        _psCache[registration] = null;
        return null;
    }
};
