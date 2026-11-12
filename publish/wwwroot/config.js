window.config = window.config || {};

const hostname = window.location.hostname;
const host = window.location.host || hostname;
const pageProtocol = window.location.protocol || 'http:';
const wsProtocol = pageProtocol === 'https:' ? 'wss:' : 'ws:';

const sameOriginHttp = `${pageProtocol}//${host}`;
const sameOriginWs = `${wsProtocol}//${host}`;

// Keep all browser traffic on the page origin. This avoids mixed-content blocking
// when the board is opened as https://<server-ip>:3000.
window.config.alarmUrl = sameOriginWs;
window.config.projectInfoUrl = sameOriginWs;
window.config.projectUrl = sameOriginHttp;
window.config.baseUrl = sameOriginHttp;
window.config.playerUrl = `${sameOriginHttp}/zlm`;

window.config.hiddenButton = false;
