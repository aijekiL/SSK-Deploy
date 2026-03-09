// window.config = window.config || {};
// // const base = 'localhost'   //本地调试
// // const base = window.location.hostname; // 自动获取当前主机名
// const base = '219.138.231.151'// 蒲圻
// const baseUrl = `${window.location.protocol}//${window.location.hostname}` // 自动获取当前主机名
// console.log(baseUrl,base, 'baseUrl')
// window.config.alarmUrl = `wss://${base}:8948` // 获取帽子报警websocket
// window.config.projectInfoUrl = `wss://${base}:8060` // 获取工程切换websocket
// // window.config.projectUrl = `https://${base}:8060` //获取项目
// window.config.projectUrl = `${baseUrl}:9991` //获取项目

// window.config.baseUrl = `${baseUrl}:8948` // 获取设备列表

// window.config.playerUrl = `${baseUrl}:8949`


// window.config.hiddenButton = false   //是否隐藏人员登记和后台管理
const base = 'pqyxkjzy.huwork.cn' // 蒲圻
//const base = '219.138.231.151'  //鄂州
//const base = '192.168.0.218'// 大别山
window.config.alarmUrl = `wss://${base}:8948` // 获取帽子报警websocket
window.config.projectInfoUrl = `wss://${base}:8060` // 获取工程切换websocket
// window.config.projectUrl = `https://${base}:8060` //获取项目
window.config.projectUrl = `http://${base}:9991` //获取项目

window.config.baseUrl = `https://${base}:8948` // 获取设备列表

window.config.playerUrl = `https://${base}:8949`


window.config.hiddenButton = false   //是否隐藏人员登记和后台管理