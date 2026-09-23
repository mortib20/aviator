'use strict';

const AviatorMap = (() => {
    let _map = null;
    let _aircraftLayer = null;
    let _metarLayer = null;
    const _tracks  = {};  // icao → L.polyline (pre-built, not on map yet)
    const _shown   = {};  // icao → bool

    // Registrations, flight data and METAR text arrive over the air — never
    // interpolate them into tooltip/popup HTML unescaped
    function esc(v) {
        return String(v ?? '').replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        })[c]);
    }

    function fmtUtc(iso) {
        return new Date(iso).toISOString().replace('T', ' ').slice(0, 19) + 'Z';
    }

    function fmtLocal(iso) {
        return new Date(iso).toLocaleString(undefined, {
            month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit'
        });
    }

    // ── Base layers ──────────────────────────────────────────────────────────

    const OSM_URL  = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    const OSM_ATTR = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

    function baseLayers() {
        return {
            // Same OSM tiles, darkened via CSS filter (.av-tiles-dark) — no extra tile provider needed
            'Dark':  L.tileLayer(OSM_URL, { attribution: OSM_ATTR, maxZoom: 19, className: 'av-tiles-dark' }),
            'Light': L.tileLayer(OSM_URL, { attribution: OSM_ATTR, maxZoom: 19 })
        };
    }

    function preferredBase(layers) {
        let name = 'Dark';
        try { name = localStorage.getItem('av-map-base') || name; } catch { /* storage blocked */ }
        return layers[name] || layers['Dark'];
    }

    function rememberBase(map) {
        map.on('baselayerchange', e => {
            try { localStorage.setItem('av-map-base', e.name); } catch { /* storage blocked */ }
        });
    }

    // ── Frame-type colours (match CSS badges) ────────────────────────────────

    function frameColor(frameType) {
        switch ((frameType || '').toLowerCase()) {
            case 'vdl2':    return '#1f6feb';  // blue
            case 'hfdl':    return '#e67700';  // amber  — distinct from VDL2
            case 'acars':   return '#2da44e';  // green
            case 'aerol':   return '#a371f7';  // purple
            case 'iridium': return '#db6d28';  // orange-red
            default:        return '#8b949e';
        }
    }

    // ── Init ─────────────────────────────────────────────────────────────────

    function init(elementId) {
        if (_map) { _map.remove(); _map = null; }

        _map = L.map(elementId, {
            center: [50, 10],
            zoom: 5,
            preferCanvas: true,
            zoomControl: true
        });

        const bases = baseLayers();
        preferredBase(bases).addTo(_map);
        rememberBase(_map);

        _aircraftLayer = L.layerGroup().addTo(_map);
        _metarLayer    = L.layerGroup().addTo(_map);

        // Collapsed on small screens so the control doesn't cover half the map
        L.control.layers(bases, {
            'Aircraft':       _aircraftLayer,
            'METAR Stations': _metarLayer
        }, { collapsed: window.innerWidth < 768, position: 'topright' }).addTo(_map);
        L.control.scale({ imperial: false }).addTo(_map);

        setTimeout(() => _map && _map.invalidateSize(), 150);
    }

    // ── Load data from API ────────────────────────────────────────────────────

    // fit: only zoom to the data on explicit loads, not on auto-refresh
    async function loadData(fromIso, toIso, fit = true) {
        if (!_map) return { aircraftCount: 0, stationsCount: 0 };
        try {
            const url = `/api/map/data?from=${encodeURIComponent(fromIso)}&to=${encodeURIComponent(toIso)}`;
            const resp = await fetch(url);
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const data = await resp.json();

            // Keep tracks the user opened visible across refreshes
            const reopen = Object.keys(_shown).filter(k => _shown[k]);
            clearTracks();
            renderAircraft(data.aircraft || []);
            renderMetarStations(data.metarStations || []);
            reopen.forEach(icao => { if (_tracks[icao]) { _tracks[icao].addTo(_map); _shown[icao] = true; } });
            if (fit) fitToData(data.aircraft || [], data.metarStations || []);

            return {
                aircraftCount: (data.aircraft || []).length,
                stationsCount: (data.metarStations || []).length
            };
        } catch (err) {
            // Rethrow so the Blazor side can show "Load failed" instead of "No data"
            console.error('[AviatorMap]', err);
            throw err;
        }
    }

    // ── Aircraft rendering ────────────────────────────────────────────────────

    function renderAircraft(aircraft) {
        _aircraftLayer.clearLayers();

        aircraft.forEach(a => {
            const color   = frameColor(a.frameType);
            const heading = computeHeading(a.track);
            const icon    = aircraftIcon(heading, color);
            const label   = esc(a.registration || a.icao.toUpperCase());
            const alt     = a.lastAltFt != null ? `${a.lastAltFt.toLocaleString()} ft` : '';
            const ftBadge = a.frameType
                ? `<span style="color:${color};font-size:.75em;opacity:.9"> ${esc(a.frameType)}</span>`
                : '';
            const detail  = a.registration
                ? `<br><a href="/aircraft/${encodeURIComponent(a.registration)}" class="av-popup-link">Details →</a>`
                : '';
            const points  = a.track ? a.track.length : 0;

            const marker = L.marker([a.lastLat, a.lastLon], { icon, title: a.registration || a.icao })
                .bindTooltip(
                    `<b>${label}</b>${ftBadge}${alt ? '<br>' + alt : ''}`
                    + `<br><span style="opacity:.6;font-size:.8em">${fmtLocal(a.lastSeen)}</span>`,
                    { direction: 'top', offset: [0, -12], className: 'av-tooltip' }
                )
                .bindPopup(
                    `<div class="av-popup-title">${label}${ftBadge}</div>`
                    // icao falls back to the registration server-side when no hex is known
                    + (a.icao !== a.registration
                        ? `<div>ICAO <span class="font-monospace">${esc(a.icao.toUpperCase())}</span></div>` : '')
                    + (alt ? `<div>${alt}</div>` : '')
                    + `<div>${points} position${points === 1 ? '' : 's'}</div>`
                    + `<div style="opacity:.5;font-size:.75em;margin-top:4px">${fmtUtc(a.lastSeen)}</div>`
                    + detail,
                    { maxWidth: 240, className: 'av-popup' }
                )
                .on('click', () => toggleTrack(a.icao));

            _aircraftLayer.addLayer(marker);

            // Pre-build track in the aircraft's own colour — added to map on click
            if (a.track && a.track.length >= 2) {
                _tracks[a.icao] = L.polyline(
                    a.track.map(p => [p.lat, p.lon]),
                    { color, weight: 2, opacity: 0.8, dashArray: '5 4' }
                );
            }
        });
    }

    function toggleTrack(icao) {
        const poly = _tracks[icao];
        if (!poly) return;

        if (_shown[icao]) {
            _map.removeLayer(poly);
            _shown[icao] = false;
        } else {
            poly.addTo(_map);
            _shown[icao] = true;
        }
    }

    function clearTracks() {
        Object.keys(_shown).forEach(icao => {
            if (_shown[icao] && _tracks[icao]) _map.removeLayer(_tracks[icao]);
        });
        Object.keys(_tracks).forEach(k => delete _tracks[k]);
        Object.keys(_shown).forEach(k => delete _shown[k]);
    }

    function computeHeading(track) {
        if (!track || track.length < 2) return 0;
        const prev = track[track.length - 2];
        const last = track[track.length - 1];
        const dLon = (last.lon - prev.lon) * Math.PI / 180;
        const φ1 = prev.lat * Math.PI / 180;
        const φ2 = last.lat * Math.PI / 180;
        const y = Math.sin(dLon) * Math.cos(φ2);
        const x = Math.cos(φ1) * Math.sin(φ2) - Math.sin(φ1) * Math.cos(φ2) * Math.cos(dLon);
        return ((Math.atan2(y, x) * 180 / Math.PI) + 360) % 360;
    }

    function aircraftIcon(heading, color) {
        const svg = `<svg viewBox="0 0 24 24" width="20" height="20"
            style="transform:rotate(${heading}deg);display:block"
            xmlns="http://www.w3.org/2000/svg">
            <polygon points="12,3 20,21 12,16 4,21"
                fill="${color}" stroke="#0d1117" stroke-width="1.5"/>
        </svg>`;
        return L.divIcon({ html: svg, iconSize: [20,20], iconAnchor: [10,10], className: 'av-aircraft-icon' });
    }

    // ── METAR station rendering ───────────────────────────────────────────────

    function renderMetarStations(stations) {
        _metarLayer.clearLayers();
        stations.forEach(s => {
            L.marker([s.lat, s.lon], { icon: metarIcon(s.temperature), zIndexOffset: -100 })
                .bindTooltip(esc(s.icao), { permanent: false, className: 'av-tooltip av-metar-hover' })
                .bindPopup(buildMetarPopup(s), { maxWidth: 260, className: 'av-popup' })
                .addTo(_metarLayer);
        });
    }

    function metarIcon(temp) {
        const t   = temp != null ? Math.round(temp) : null;
        const str = t != null ? `${t}°` : '?';
        const col = tempColor(temp);
        return L.divIcon({
            html: `<div class="av-metar-badge" style="border-color:${col};color:${col}">${esc(str)}</div>`,
            iconSize: [40, 22], iconAnchor: [20, 11], className: ''
        });
    }

    function buildMetarPopup(s) {
        const rows = [];
        rows.push(`<div class="av-popup-title">${esc(s.icao)} <span style="font-weight:400;opacity:.7">${esc(s.name)}</span></div>`);
        if (s.temperature != null) {
            const tc = tempColor(s.temperature);
            rows.push(`<div><span style="color:${tc}">&#x1F321; ${s.temperature}°C</span>`
                + (s.dewPoint != null ? ` / ${s.dewPoint}°C dp` : '') + '</div>');
        }
        if (s.windSpeedKt != null) {
            const dir = s.windDir != null ? `${degreesToCardinal(s.windDir)} ${s.windDir}°` : 'VRB';
            rows.push(`<div>&#x1F32C; ${dir} ${s.windSpeedKt} kt`
                + (s.windGustKt ? ` G${s.windGustKt} kt` : '') + '</div>');
        }
        if (s.cavok) {
            rows.push('<div><span style="color:#3fb950">CAVOK</span></div>');
        } else if (s.visibilityMeters != null) {
            rows.push(`<div>&#x1F441; ${formatVis(s.visibilityMeters)}</div>`);
        }
        if (s.qnh != null) rows.push(`<div>&#x2B55; ${s.qnh} hPa</div>`);
        if (s.trend) rows.push(`<div style="opacity:.65;font-size:.82em">${esc(s.trend)}</div>`);
        rows.push(`<div style="opacity:.5;font-size:.75em;margin-top:4px">${fmtUtc(s.timestamp)}</div>`);
        return rows.join('');
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    function fitToData(aircraft, stations) {
        const pts = [];
        aircraft.forEach(a => pts.push([a.lastLat, a.lastLon]));
        stations.forEach(s => pts.push([s.lat, s.lon]));
        if (pts.length === 0) return;
        if (pts.length === 1) { _map.setView(pts[0], 9); return; }
        _map.fitBounds(pts, { padding: [50, 50], maxZoom: 12, animate: true });
    }

    function tempColor(temp) {
        if (temp == null) return '#8b949e';
        if (temp < -10) return '#a5d8ff';
        if (temp <   0) return '#74c0fc';
        if (temp <  10) return '#a9e34b';
        if (temp <  20) return '#51cf66';
        if (temp <  30) return '#ffa94d';
        return '#ff6b6b';
    }

    function formatVis(meters) {
        if (meters >= 9999) return '10+ km';
        if (meters >= 1000) { const km = meters / 1000; return (km % 1 === 0 ? km : km.toFixed(1)) + ' km'; }
        return meters + ' m';
    }

    function degreesToCardinal(deg) {
        const dirs = ['N','NNE','NE','ENE','E','ESE','SE','SSE','S','SSW','SW','WSW','W','WNW','NW','NNW'];
        return dirs[Math.round(deg / 22.5) % 16];
    }

    function destroy() {
        clearTracks();
        if (_map) { _map.remove(); _map = null; }
        _aircraftLayer = null;
        _metarLayer = null;
    }

    // ── Single-aircraft track map (detail page) ───────────────────────────────

    let _detailMap = null;

    function trackPointTooltip(p) {
        const alt = p.altFt != null ? `${p.altFt.toLocaleString()} ft<br>` : '';
        return `${alt}<span style="opacity:.6;font-size:.8em">${fmtLocal(p.ts)}</span>`;
    }

    function showTrack(elementId, points, frameType) {
        destroyTrack();
        if (!points || points.length === 0) return;

        _detailMap = L.map(elementId, { preferCanvas: true });
        const bases = baseLayers();
        preferredBase(bases).addTo(_detailMap);
        rememberBase(_detailMap);
        L.control.layers(bases, null, { collapsed: true }).addTo(_detailMap);

        const color   = frameColor(frameType);
        const latlngs = points.map(p => [p.lat, p.lon]);

        if (points.length >= 2) {
            L.polyline(latlngs, { color, weight: 2, opacity: 0.8, dashArray: '5 4' }).addTo(_detailMap);
            points.slice(0, -1).forEach(p =>
                L.circleMarker([p.lat, p.lon], { radius: 3, stroke: false, fillColor: color, fillOpacity: 0.85 })
                    .bindTooltip(trackPointTooltip(p), { className: 'av-tooltip' })
                    .addTo(_detailMap));
        }

        const last = points[points.length - 1];
        L.marker([last.lat, last.lon], { icon: aircraftIcon(computeHeading(points), color) })
            .bindTooltip(trackPointTooltip(last), { direction: 'top', offset: [0, -12], className: 'av-tooltip' })
            .addTo(_detailMap);

        if (points.length === 1) _detailMap.setView(latlngs[0], 8);
        else _detailMap.fitBounds(latlngs, { padding: [30, 30], maxZoom: 10 });

        setTimeout(() => _detailMap && _detailMap.invalidateSize(), 150);
    }

    function destroyTrack() {
        if (_detailMap) { _detailMap.remove(); _detailMap = null; }
    }

    return { init, loadData, destroy, showTrack, destroyTrack };
})();

window.AviatorMap = AviatorMap;
