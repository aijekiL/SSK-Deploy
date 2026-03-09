window.config = window.config || {};

// 自动获取当前主机名，适配各种系统环境
let base = window.location.hostname;

// 统一处理逻辑，当为主机名为空或本地地址时，使用location.host（包含端口号 ?
if (!base || base === 'localhost' || base === '127.0.0.1') {
  base = window.location.host;
}

const baseUrl = `${window.location.protocol}//${base}`; // 自动获取当前主机 ?

window.config.alarmUrl = `ws://${base}/websocket/message`; // 获取帽子报警websocket
window.config.projectInfoUrl = `ws://${base}/api/ProjectInfo/GetMonitorProjectInfo`; // 获取工程信息websocket
window.config.projectUrl = `http://${base}`; //获取项目
// 开发环境下直接指向后端服务
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
  window.config.projectUrl = 'http://localhost:9991';
  window.config.baseUrl = 'http://localhost:5010'; // 开发环境下指向后端服务
} else {
  window.config.baseUrl = `http://${base}`; // 获取设备列表
}

window.config.playerUrl = `http://${base}`;

window.config.hiddenButton = false; //是否隐藏人员登记和后台管