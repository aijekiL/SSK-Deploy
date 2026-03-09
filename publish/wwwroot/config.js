window.config = window.config || {};

const hostname = window.location.hostname;
const host = window.location.host || hostname;
const isLocalhost = hostname === 'localhost' || hostname === '127.0.0.1';

const httpProtocol = 'http:';
const wsProtocol = 'ws:';

const httpBase = `${httpProtocol}//${host}`;
const projectHost = `${hostname}:9991`;
const baseHost = `${hostname}:5010`;
const localPlayerHost = `${hostname}:8081`;
const securePlayerHost = `${hostname}:8949`;

window.config.alarmUrl = `${wsProtocol}//${baseHost}`;
window.config.projectInfoUrl = `${wsProtocol}//${projectHost}`;
window.config.projectUrl = `${httpProtocol}//${projectHost}`;
window.config.baseUrl = `${httpProtocol}//${baseHost}`;
window.config.playerUrl = isLocalhost
  ? `${httpProtocol}//${localPlayerHost}`
  : `https://${securePlayerHost}`;

window.config.hiddenButton = false;
