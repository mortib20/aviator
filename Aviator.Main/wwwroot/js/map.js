'use strict';

const AviatorMap = (() => {
    let _map = null;
    let _aircraftLayer = null;
    let _metarLayer = null;
    const _tracks = {};       // icao → L.polyline (pre-built, not on map)
    const _shownTracks = {};  // icao → bool

    // ── Init ────────────────────────────────────────────────────────────────

    function init(elementId) {
        if (_map) { _map.remove(); _map = null; }

        _map = L.map(elementId, {
            center: [50, 10],
            zoom: 5,
            preferCanvas: true,
            zoomControl: true
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
            maxZoom: 19
        }).addTo(_map);

        _aircraftLayer = L.layerGroup().addTo(_map);
        _metarLayer    = L.layerGroup().addTo(_map);

        L.control.layers(null, {
            'Aircraft':        _aircraftLayer,
            'METAR Stations':  _metarLayer
        }, { collapsed: false, position: 'topright' }).addTo(_map);

        // Leaflet needs the container to have a known size
        setTimeout(() => _map && _map.invalidateSize(), 150);
    }

    // ── Load data from API ───────────────────────────────────────────────────

    async function loadData(fromIso, toIso) {
        if (!_map) return { aircraftCount: 0, stationsCount: 0 };

        try {
            const url = `/api/map/data?from=${encodeURIComponent(fromIso)}&to=${encodeURIComponent(toIso)}`;
            const resp = await fetch(url);
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const data = await resp.json();

            clearTracks();
            renderAircraft(data.aircraft  || []);
            renderMetarStations(data.metarStations || []);
            fitToData(data.aircraft || [], data.metarStations || []);

            return {
                aircraftCount: (data.aircraft  || []).length,
                stationsCount: (data.metarStations || []).length
            };
        } catch (err) {
            console.error('[AviatorMap]', err);
            return { aircraftCount: 0, stationsCount: 0 };
        }
    }

    // ── Aircraft rendering ───────────────────────────────────────────────────

    function renderAircraft(aircraft) {
        _aircraftLayer.clearLayers();

        aircraft.forEach(a => {
            const heading = computeHeading(a.track);
            const icon    = aircraftIcon(heading);
            const label   = a.registration || a.icao;
            const alt     = a.lastAltFt != null ? `${a.lastAltFt.toLocaleString()} ft` : '';
            const ts      = new Date(a.lastSeen).toUTCString().replace(' GMT', ' UTC');

            const marker = L.marker([a.lastLat, a.lastLon], { icon, title: label })
                .bindTooltip(
                    `<b>${label}</b>${alt ? '<br>' + alt : ''}<br><span style="opacity:.7;font-size:.8em">${ts}</span>`,
                    { direction: 'top', offset: [0, -12], className: 'av-tooltip' }
                )
                .on('click', () => toggleTrack(a));

            _aircraftLayer.addLayer(marker);

            // Pre-build track polyline (not added to map yet)
            if (a.track && a.track.length >= 2) {
                _tracks[a.icao] = L.polyline(
                    a.track.map(p => [p.lat, p.lon]),
                    { color: '#58a6ff', weight: 2, opacity: 0.75, dashArray: '5 4' }
                );
            }
        });
    }

    function toggleTrack(aircraft) {
        const poly = _tracks[aircraft.icao];
        if (!poly) return;

        if (_shownTracks[aircraft.icao]) {
            _map.removeLayer(poly);
            _shownTracks[aircraft.icao] = false;
        } else {
            poly.addTo(_map);
            _shownTracks[aircraft.icao] = true;
        }
    }

    function clearTracks() {
        Object.entries(_shownTracks).forEach(([icao, shown]) => {
            if (shown && _tracks[icao]) _map.removeLayer(_tracks[icao]);
        });
        Object.keys(_tracks).forEach(k => delete _tracks[k]);
        Object.keys(_shownTracks).forEach(k => delete _shownTracks[k]);
    }

    function computeHeading(track) {
        if (!track || track.length < 2) return 0;
        const prev = track[track.length - 2];
        const last = track[track.length - 1];
        const dLon = (last.lon - prev.lon) * Math.PI / 180;
        const lat1 = prev.lat * Math.PI / 180;
        const lat2 = last.lat * Math.PI / 180;
        const y = Math.sin(dLon) * Math.cos(lat2);
        const x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
        return ((Math.atan2(y, x) * 180 / Math.PI) + 360) % 360;
    }

    function aircraftIcon(heading) {
        // Triangle arrow pointing north; rotate to heading
        const svg = `<svg viewBox="0 0 24 24" width="20" height="20"
            style="transform:rotate(${heading}deg);display:block"
            xmlns="http://www.w3.org/2000/svg">
            <polygon points="12,3 20,21 12,16 4,21"
                fill="#58a6ff" stroke="#0d1117" stroke-width="1.5"/>
        </svg>`;
        return L.divIcon({
            html: svg,
            iconSize:   [20, 20],
            iconAnchor: [10, 10],
            className:  'av-aircraft-icon'
        });
    }

    // ── METAR station rendering ──────────────────────────────────────────────

    function renderMetarStations(stations) {
        _metarLayer.clearLayers();

        stations.forEach(s => {
            const icon = metarIcon(s.temperature);

            const marker = L.marker([s.lat, s.lon], { icon, zIndexOffset: -100 })
                .bindTooltip(s.icao, { permanent: false, className: 'av-tooltip av-metar-hover' })
                .bindPopup(buildMetarPopup(s), { maxWidth: 260, className: 'av-popup' });

            _metarLayer.addLayer(marker);
        });
    }

    function metarIcon(temp) {
        const t   = temp != null ? Math.round(temp) : null;
        const str = t != null ? (t >= 0 ? `${t}°` : `${t}°`) : '?';
        const col = tempColor(temp);

        const html = `<div class="av-metar-badge" style="border-color:${col};color:${col}">${str}</div>`;
        return L.divIcon({
            html,
            iconSize:   [40, 22],
            iconAnchor: [20, 11],
            className:  ''
        });
    }

    function buildMetarPopup(s) {
        const rows = [];

        rows.push(`<div class="av-popup-title">${s.icao} <span style="font-weight:400;opacity:.7">${s.name}</span></div>`);

        if (s.temperature != null) {
            const tc = tempColor(s.temperature);
            rows.push(`<div><span style="color:${tc}">&#x1F321; ${s.temperature}°C</span>`
                + (s.dewPoint != null ? ` / ${s.dewPoint}°C dp` : '') + '</div>');
        }

        if (s.windSpeedKt != null) {
            const dir = s.windDir != null
                ? `${degreesToCardinal(s.windDir)} ${s.windDir}°`
                : 'VRB';
            rows.push(`<div>&#x1F32C; ${dir} ${s.windSpeedKt} kt`
                + (s.windGustKt ? ` G${s.windGustKt} kt` : '') + '</div>');
        }

        if (s.cavok) {
            rows.push('<div><span style="color:#3fb950">CAVOK</span></div>');
        } else if (s.visibilityMeters != null) {
            rows.push(`<div>&#x1F441; ${formatVis(s.visibilityMeters)}</div>`);
        }

        if (s.qnh != null) {
            rows.push(`<div>&#x2B55; ${s.qnh} hPa</div>`);
        }

        if (s.trend) {
            rows.push(`<div style="opacity:.65;font-size:.82em">${s.trend}</div>`);
        }

        const ts = new Date(s.timestamp).toUTCString().replace(' GMT', ' UTC');
        rows.push(`<div style="opacity:.5;font-size:.75em;margin-top:4px">${ts}</div>`);

        return rows.join('');
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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
        if (meters >= 1000) {
            const km = meters / 1000;
            return (km % 1 === 0 ? km : km.toFixed(1)) + ' km';
        }
        return meters + ' m';
    }

    function degreesToCardinal(deg) {
        const dirs = ['N','NNE','NE','ENE','E','ESE','SE','SSE',
                      'S','SSW','SW','WSW','W','WNW','NW','NNW'];
        return dirs[Math.round(deg / 22.5) % 16];
    }

    // ── Public API ───────────────────────────────────────────────────────────

    return { init, loadData };
})();

window.AviatorMap = AviatorMap;
