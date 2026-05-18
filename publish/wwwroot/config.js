window.config = window.config || {};

const hostname = window.location.hostname;
const host = window.location.host || hostname;
const httpProtocol = window.location.protocol === 'https:' ? 'https:' : 'http:';
const wsProtocol = httpProtocol === 'https:' ? 'wss:' : 'ws:';
const sameOrigin = `${httpProtocol}//${host}`;

window.config.alarmUrl = `${wsProtocol}//${host}`;
window.config.projectInfoUrl = `${wsProtocol}//${host}`;
window.config.projectUrl = sameOrigin;
window.config.baseUrl = sameOrigin;
window.config.playerUrl = `${sameOrigin}/zlm`;

window.config.hiddenButton = false;
