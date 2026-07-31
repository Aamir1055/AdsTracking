(function() {
    'use strict';

    var API = '/api/dashboard';

    function formatTime(seconds) {
        if (!seconds || seconds === 0) return '0s';
        if (seconds < 60) return seconds + 's';
        var m = Math.floor(seconds / 60);
        var s = seconds % 60;
        return m + 'm ' + s + 's';
    }

    function formatDate(dateStr) {
        if (!dateStr) return '-';

        // Dashboard timestamps are stored in UTC. Keep the fallback for existing API data
        // that may not yet contain a UTC offset, then display the viewer's local time.
        var value = String(dateStr);
        if (!/(?:Z|[+-]\d{2}:?\d{2})$/i.test(value)) value += 'Z';

        var d = new Date(value);
        if (Number.isNaN(d.getTime())) return '-';

        var day = String(d.getDate()).padStart(2, '0');
        var month = String(d.getMonth() + 1).padStart(2, '0');
        var year = d.getFullYear();
        var hours = String(d.getHours()).padStart(2, '0');
        var mins = String(d.getMinutes()).padStart(2, '0');
        return day + '/' + month + '/' + year + ' ' + hours + ':' + mins;
    }

    function shortId(id) {
        if (!id) return '-';
        return id.substring(0, 8) + '...';
    }

    async function loadDashboard() {
        try {
            var resp = await fetch(API + '/insights');
            if (!resp.ok) throw new Error('API error: ' + resp.status);
            var data = await resp.json();
            renderDashboard(data);
        } catch (e) {
            console.error('Dashboard load failed:', e);
        }
    }

    function renderDashboard(data) {
        // KPIs
        document.getElementById('total-visitors').textContent = data.totalVisitors;
        document.getElementById('total-pageviews').textContent = data.totalPageViews;
        document.getElementById('avg-time').textContent = formatTime(data.avgTimeOnPage);
        document.getElementById('telegram-clicks').textContent = data.telegramClicks;

        var convRate = data.totalVisitors > 0
            ? Math.round((data.telegramClicks / data.totalVisitors) * 100) : 0;

        // Telegram join insights
        document.getElementById('telegram-members').textContent = data.telegramMembers;

        // Funnel — Visited → Clicked Join Telegram → Members Joined
        var maxVal = Math.max(data.totalVisitors, 1);
        document.getElementById('funnel-visitors-count').textContent = data.totalVisitors;
        document.getElementById('funnel-cta-count').textContent = data.telegramClicks;
        document.getElementById('funnel-telegram-count').textContent = data.telegramMembers;

        document.getElementById('funnel-visitors-pct').textContent = '100%';
        document.getElementById('funnel-cta-pct').textContent = maxVal > 0 ? Math.round((data.telegramClicks / maxVal) * 100) + '%' : '-';
        document.getElementById('funnel-telegram-pct').textContent = maxVal > 0 ? Math.round((data.telegramMembers / maxVal) * 100) + '%' : '-';

        // Color funnel circles based on value
        setFunnelCircle('funnel-visitors', data.totalVisitors, maxVal);
        setFunnelCircle('funnel-cta', data.telegramClicks, maxVal);
        setFunnelCircle('funnel-telegram', data.telegramMembers, maxVal);

        // Campaign table
        var campBody = document.getElementById('campaigns-body');
        if (data.campaigns && data.campaigns.length > 0) {
            campBody.innerHTML = data.campaigns.map(function(c) {
                var conv = c.visitors > 0 ? Math.round((c.telegramClicks / c.visitors) * 100) : 0;
                return '<tr><td>' + esc(c.utmSource) + '</td><td>' + esc(c.utmMedium) +
                    '</td><td>' + esc(c.utmCampaign) + '</td><td>' + c.visitors +
                    '</td><td>' + c.telegramClicks + '</td><td>' + conv + '%</td></tr>';
            }).join('');
        } else {
            campBody.innerHTML = '<tr><td colspan="6" class="empty-state">No campaign data yet</td></tr>';
        }

        // Visitors table
        var visBody = document.getElementById('visitors-body');
        if (data.recentVisitors && data.recentVisitors.length > 0) {
            visBody.innerHTML = data.recentVisitors.map(function(v) {
                var tgClick = v.clickedTelegram ? '<span class="badge badge-green">Yes</span>' : '<span class="badge badge-gray">No</span>';
                return '<tr><td class="visitor-id">' + shortId(v.visitorId) + '</td>' +
                    '<td>' + esc(v.ipAddress || '-') + '</td>' +
                    '<td>' + esc(v.utmSource) + ' / ' + esc(v.utmCampaign) + '</td>' +
                    '<td>' + formatDate(v.firstSeen) + '</td>' +
                    '<td>' + v.pageViews + '</td>' +
                    '<td>' + formatTime(v.totalTime) + '</td>' +
                    '<td>' + tgClick + '</td></tr>';
            }).join('');
        } else {
            visBody.innerHTML = '<tr><td colspan="7" class="empty-state">No visitors yet</td></tr>';
        }

    }

    function setFunnelCircle(id, value, max) {
        var el = document.getElementById(id);
        var pct = max > 0 ? Math.min(value / max, 1) : 0;
        var colors = ['#a6ff00', '#7acc00', '#4f9900'];
        var idx = { 'funnel-visitors': 0, 'funnel-cta': 1, 'funnel-telegram': 2 };
        var color = colors[idx[id]] || '#a6ff00';
        el.style.borderColor = color;
        el.style.background = pct > 0 ? 'rgba(166, 255, 0, 0.06)' : '#0f0f0f';
        var size = 65 + (pct * 25);
        el.style.width = size + 'px';
        el.style.height = size + 'px';
    }

    function esc(str) {
        if (!str) return '';
        var d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    // Init
    document.getElementById('refresh-btn').addEventListener('click', function() {
        window.location.reload();
    });
    loadDashboard();

    // Auto-refresh every 30 seconds
    setInterval(loadDashboard, 30000);
})();
