# Mesh便携指挥箱系统与电厂内网系统接口对接文档

版本：V1.0  
日期：2026-06-01  
适用系统：Mesh便携指挥箱 Web 系统 / CAPS 后端服务  
后端服务：RuoYi Spring Boot，默认端口 `5010`

## 1. 文档目的

本文档用于指导电厂内网系统与 Mesh 便携指挥箱系统进行对接，支持以下两类集成方式：

1. 电厂内网系统通过菜单、按钮或业务页面跳转到指挥箱 Web 页面。
2. 电厂内网系统通过 HTTP API 获取或写入设备、人员、实时数据、告警、气体检测、视频通话等业务数据。

本文档包含网络要求、认证要求、接口规范、字段说明、示例报文、错误码、联调检查项和安全建议。

## 2. 对接范围

### 2.1 点击跳转

电厂内网系统提供入口，用户点击后进入指挥箱系统页面。

典型场景：

- 从电厂综合平台跳转到指挥箱大屏。
- 从人员详情页跳转到对应人员或设备详情。
- 从告警列表跳转到指挥箱告警处置页面。
- 从视频会商页面跳转到指挥箱视频通话页面。

### 2.2 接口对接

电厂内网系统通过接口读取或写入数据。

典型场景：

- 查询安全帽、手表、声光报警器等设备绑定关系。
- 查询设备实时定位、电量、佩戴状态、气体检测数据。
- 查询设备告警信息。
- 更新人员与设备绑定关系。
- 推送气体检测数据。
- 发起或关闭 WebRTC 视频通话。
- 控制声光报警器开启或关闭。

## 3. 系统部署与网络要求

### 3.1 基础地址

以下地址以实际部署环境为准：

| 名称 | 示例 | 说明 |
| --- | --- | --- |
| 指挥箱 Web 入口 | `http://{CAPS_WEB_HOST}/` | 浏览器访问入口，通常由 Nginx 提供 |
| 指挥箱后端 API | `http://{CAPS_API_HOST}:5010` | Spring Boot 后端服务 |
| WebSocket 地址 | `ws://{CAPS_API_HOST}:5010/websocket/message` | 设备或前端消息通道 |
| WebRTC 服务 | `https://{MEDIA_HOST}:8949` | 视频通话信令/播放地址，按现场配置 |
| RTMP 服务 | `rtmp://{MEDIA_HOST}:1935` | 视频流地址，按现场配置 |

当前后端配置中：

- `server.port = 5010`
- `server.servlet.context-path = /`
- `rtmp.url` 默认来源于环境变量 `RTMP_URL`，未配置时为 `192.168.144.213`
- `webrtc.url` 默认来源于环境变量 `WEBRTC_URL`

### 3.2 网络连通要求

电厂内网系统访问指挥箱系统前，应确认：

| 方向 | 协议/端口 | 用途 | 是否必须 |
| --- | --- | --- | --- |
| 电厂内网用户浏览器 -> 指挥箱 Web | TCP 80/443 或现场端口 | 页面跳转访问 | 是 |
| 电厂内网系统 -> 指挥箱 API | TCP 5010 或代理端口 | 接口调用 | 是 |
| 电厂内网用户浏览器 -> WebSocket | TCP 5010 或代理端口 | 实时消息 | 按功能需要 |
| 电厂内网用户浏览器 -> WebRTC | TCP/UDP，现场端口 | 视频通话 | 按功能需要 |
| 电厂内网系统 -> RTMP/WebRTC | TCP/UDP，现场端口 | 视频流集成 | 按功能需要 |

不建议直接开放 MySQL、Redis、内部管理端口给电厂内网系统。

### 3.3 推荐网络架构

推荐采用以下方式之一：

1. 指挥箱设备接入电厂内网，分配固定 IP。
2. 电厂内网通过静态路由访问指挥箱所在网段。
3. 在电厂内网部署 Nginx/API 网关，反向代理到指挥箱后端。
4. 若电厂安全策略不允许主动访问指挥箱，则由指挥箱主动调用电厂接口推送数据。

正式生产环境建议使用 HTTPS 和固定域名，例如：

```text
https://caps.powerplant.local/
https://caps-api.powerplant.local/
```

## 4. 对接方式说明

### 4.1 普通页面跳转

电厂系统菜单配置：

```text
http://{CAPS_WEB_HOST}/
```

若由 Nginx 统一代理：

```text
https://{POWER_INTRANET_DOMAIN}/caps/
```

### 4.2 携带参数跳转

用于从电厂系统跳转到指定业务页面。

示例：

```text
https://{CAPS_WEB_HOST}/#/device/detail?deviceId=CAPS-001
https://{CAPS_WEB_HOST}/#/person/detail?personId=P10001
https://{CAPS_WEB_HOST}/#/alarm/list?deviceId=CAPS-001
```

具体前端路由以实际前端项目为准。若当前前端没有对应路由，需要新增页面路由和参数解析逻辑。

### 4.3 单点登录跳转，建议新增

正式集成建议不要在 URL 中传递账号密码。推荐使用一次性登录令牌。

流程：

```text
电厂系统生成一次性 token
    -> 浏览器跳转到指挥箱 SSO 地址
    -> 指挥箱后端校验 token
    -> 校验通过后生成本系统登录态
    -> 跳转到目标页面
```

建议接口：

```http
GET /openapi/sso/login?token={ssoToken}&redirect={encodedPath}
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| token | string | 是 | 电厂系统生成的一次性登录令牌 |
| redirect | string | 否 | 登录成功后的相对跳转路径 |

安全要求：

- token 有效期建议不超过 60 秒。
- token 只能使用一次。
- token 应绑定来源系统、用户标识、过期时间和签名。
- redirect 只允许系统内部相对路径，禁止跳转外部 URL。

## 5. 接口通用规范

### 5.1 请求协议

| 项目 | 说明 |
| --- | --- |
| 协议 | HTTP/HTTPS |
| 数据格式 | JSON |
| 字符集 | UTF-8 |
| 时间格式 | `yyyy-MM-dd HH:mm:ss`，或 Unix 秒级时间戳 |
| 请求头 | `Content-Type: application/json;charset=UTF-8` |

### 5.2 认证方式

当前系统存在两类认证方式：

| 类型 | 请求头 | 适用接口 |
| --- | --- | --- |
| 后台登录 Token | `Authorization: Bearer {token}` | `/system/**` 管理接口 |
| 接口密钥 | 当前过滤器使用 `Authorization: {apiSecret}` | `/system/device/*` 部分接口 |

当前配置文件中存在：

```yaml
api:
  secret: 'X5k2B9n6P8m7Z1q4'
```

注意：

- `CapsDeviceController.updatePersonIdAndDeviceInfo` 方法参数声明了 `X-API-Secret`，但当前 `SecretAuthFilter` 实际读取的是 `Authorization` 请求头。
- 正式对接建议统一改造为 `X-API-Key` 或 `X-API-Secret`，并避免和后台登录 `Authorization: Bearer ...` 混用。
- 对外接口建议新建 `/openapi/caps/**` 路径，独立鉴权、限流、日志和白名单控制。

### 5.3 推荐开放接口签名，正式环境建议

建议电厂系统调用指挥箱开放接口时统一携带：

```http
X-API-Key: {appKey}
X-Timestamp: {unixMillis}
X-Nonce: {randomString}
X-Signature: {signature}
```

签名原文建议：

```text
HTTP_METHOD + "\n" +
REQUEST_PATH + "\n" +
X-Timestamp + "\n" +
X-Nonce + "\n" +
SHA256(body)
```

签名算法：

```text
HMAC-SHA256(signText, appSecret)
```

重放保护：

- `X-Timestamp` 与服务器时间差不得超过 5 分钟。
- `X-Nonce` 在有效期内只能使用一次。
- 所有调用记录入库或写入审计日志。

### 5.4 普通返回结构

RuoYi 默认 `AjaxResult`：

```json
{
  "code": 200,
  "msg": "操作成功",
  "data": {}
}
```

分页返回 `TableDataInfo`：

```json
{
  "code": 200,
  "msg": "查询成功",
  "total": 1,
  "rows": []
}
```

部分设备接口使用 `AjaxResultNew`：

```json
{
  "status": true,
  "msg": "success",
  "msg_code": "GetSucc",
  "data": {}
}
```

### 5.5 通用状态码

| HTTP 状态码 | 业务含义 | 处理建议 |
| --- | --- | --- |
| 200 | 请求成功 | 继续解析业务字段 |
| 400 | 参数错误 | 检查请求参数 |
| 401 | 未认证或密钥错误 | 检查 Token、接口密钥或签名 |
| 403 | 无权限 | 检查账号权限、IP 白名单 |
| 404 | 接口不存在 | 检查路径和代理配置 |
| 500 | 服务异常 | 查看后端日志 |

## 6. 现有接口清单

以下为当前代码中已经存在的接口。若直接用于电厂内网对接，应先确认安全策略和是否需要封装为 `/openapi/caps/**`。

| 序号 | 接口 | 方法 | 说明 | 当前建议 |
| --- | --- | --- | --- | --- |
| 1 | `/login` | POST | 后台登录获取 token | 后台管理使用 |
| 2 | `/system/device/list` | GET | 查询设备列表 | 可封装为开放接口 |
| 3 | `/system/device/{id}` | GET | 查询设备详情 | 可封装为开放接口 |
| 4 | `/system/device/watchLatest` | GET | 查询人员最新手表体征数据 | 可封装为开放接口 |
| 5 | `/system/device/updatePersonIdAndDeviceInfo` | POST | 更新人员与设备绑定 | 可用于对接，需统一密钥头 |
| 6 | `/system/report/list` | GET | 查询设备实时数据列表 | 可封装为开放接口 |
| 7 | `/system/report/{id}` | GET | 查询实时数据详情 | 可封装为开放接口 |
| 8 | `/system/device/alarm/list` | GET | 查询告警列表 | 可封装为开放接口 |
| 9 | `/system/device/alarm/{id}` | GET | 查询告警详情 | 可封装为开放接口 |
| 10 | `/caps/api/gas/report` | POST | 气体检测数据上报 | 可直接联调，建议加鉴权 |
| 11 | `/caps/api/openHuaheDevice/{personId}` | GET | 打开人员关联声光报警器 | 可封装为控制接口 |
| 12 | `/caps/api/closeHuaheDevice/{personId}` | GET | 关闭人员关联声光报警器 | 可封装为控制接口 |
| 13 | `/caps/api/openDialog` | POST | 发起视频通话 | 可封装为控制接口 |
| 14 | `/caps/api/closeDialog` | POST | 关闭视频通话 | 可封装为控制接口 |

## 7. 登录接口

### 7.1 后台登录

```http
POST /login
Content-Type: application/json
```

请求体：

```json
{
  "username": "admin",
  "password": "password",
  "code": "1",
  "uuid": "captcha-uuid"
}
```

响应示例：

```json
{
  "code": 200,
  "msg": "操作成功",
  "token": "eyJhbGciOiJIUzUxMiJ9..."
}
```

后续请求头：

```http
Authorization: Bearer eyJhbGciOiJIUzUxMiJ9...
```

说明：

- 当前 token 默认有效期为 30 分钟。
- 正式开放给电厂系统时，不建议让第三方直接使用后台管理员账号调用接口。
- 推荐为电厂系统创建专用接口账号或改造独立开放接口鉴权。

## 8. 设备接口

### 8.1 查询设备列表

```http
GET /system/device/list?pageNum=1&pageSize=10&deviceId=CAPS-001&personId=P10001
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| pageNum | number | 否 | 页码，默认由前端或 PageHelper 处理 |
| pageSize | number | 否 | 每页条数 |
| deviceId | string | 否 | 设备 ID |
| personId | string | 否 | 人员 ID |
| userId | string | 否 | 设备端用户 ID |
| userName | string | 否 | 用户姓名 |
| mobile | string | 否 | 手机号 |

响应示例：

```json
{
  "code": 200,
  "msg": "查询成功",
  "total": 1,
  "rows": [
    {
      "id": 1,
      "deviceId": "CAPS-001",
      "personId": "P10001",
      "userId": "U10001",
      "userName": "张三",
      "mobile": "13800000000",
      "createTime": "2026-06-01 09:30:00",
      "updateTime": "2026-06-01 10:00:00"
    }
  ]
}
```

### 8.2 查询设备详情

```http
GET /system/device/{id}
```

路径参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| id | number | 是 | 设备表主键 ID |

响应示例：

```json
{
  "code": 200,
  "msg": "操作成功",
  "data": {
    "id": 1,
    "deviceId": "CAPS-001",
    "personId": "P10001",
    "userId": "U10001",
    "userName": "张三",
    "mobile": "13800000000"
  }
}
```

### 8.3 更新人员与设备绑定关系

```http
POST /system/device/updatePersonIdAndDeviceInfo
Content-Type: application/json
Authorization: {apiSecret}
```

请求体：

```json
{
  "personId": "P10001",
  "deviceIds": ["CAPS-001", "WATCH-001"],
  "audibleIds": ["ALARM-001"]
}
```

请求字段：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| personId | string | 是 | 电厂人员 ID 或统一人员编码 |
| deviceIds | array[string] | 否 | 安全帽、手表等设备 ID 列表 |
| audibleIds | array[string] | 否 | 声光报警器设备 ID 列表 |

响应示例：

```json
{
  "code": 200,
  "msg": "更新成功"
}
```

当前实现注意事项：

- 当前过滤器读取请求头 `Authorization` 作为密钥。
- Controller 方法中声明了 `X-API-Secret`，但未实际校验该值。
- 建议联调前统一为 `X-API-Secret: {apiSecret}` 并调整过滤器配置。

### 8.4 查询人员最新手表体征数据

```http
GET /system/device/watchLatest?personId=P10001&excludeDeviceId=CAPS-001
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| personId | string | 是 | 人员 ID |
| excludeDeviceId | string | 否 | 排除设备 ID，通常用于排除安全帽设备，仅取手表数据 |

响应示例：

```json
{
  "code": 200,
  "msg": "操作成功",
  "data": {
    "deviceId": "WATCH-001",
    "heartRate": "78",
    "spo2": "98",
    "systolic": "120",
    "diastolic": "80",
    "bloodPressure": "120/80",
    "updateTime": "2026-06-01 10:10:20"
  }
}
```

## 9. 实时数据接口

### 9.1 查询设备实时数据列表

```http
GET /system/report/list?pageNum=1&pageSize=10&deviceId=CAPS-001
Authorization: Bearer {token}
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| pageNum | number | 否 | 页码 |
| pageSize | number | 否 | 每页条数 |
| deviceId | string | 否 | 设备 ID |
| userId | string | 否 | 设备端用户 ID |

响应示例：

```json
{
  "code": 200,
  "msg": "查询成功",
  "total": 1,
  "rows": [
    {
      "id": 10,
      "deviceId": "CAPS-001",
      "userId": "U10001",
      "xPoint": "39.904200",
      "yPoint": "116.407396",
      "batL": "88",
      "batV": "4.1",
      "charging": "1",
      "gps": "1",
      "gpsLevel": "5",
      "inUse": "1",
      "isWeared": "2",
      "netStrenth": "5",
      "netType": "1",
      "tempdata": "36.5",
      "o2": "20.9",
      "co": "0",
      "h2s": "0",
      "ch4": "0",
      "updateTime": "2026-06-01 10:10:20"
    }
  ]
}
```

### 9.2 实时数据字段说明

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| id | number | 数据主键 |
| deviceId | string | 设备 ID |
| userId | string | 设备端用户 ID |
| altitude | string | 相对开机时高度，单位米 |
| batL | string | 当前电量百分比 |
| batV | string | 当前电压 |
| btSignalInfo | string | 蓝牙设备信息 |
| cSpeed | string | 当前速度 |
| charging | string | 充电状态，`0` 正在充电，`1` 未充电 |
| gps | string | 定位类型，`0` 基站，`1` GPS，`2` Wi-Fi，其他值为其他类型 |
| gpsLevel | string | GPS 信号强度 |
| inUse | string | 设备状态，`1` 在线，`2` 监控，`3` 视频通话 |
| isWeared | string | 佩戴状态，`0` 默认，`1` 未佩戴，`2` 已佩戴 |
| netStrenth | string | 网络信号强度，0-5 |
| netType | string | 网络类型，`1` 4G，`2` Wi-Fi |
| phoneNumber | string | SIM 卡号 |
| simDataNum | string | 当月已用流量，单位 MB |
| simStatus | string | SIM 卡状态，`0` 未知，`1` 有卡，`2` 无卡 |
| tcardStatus | string | 存储卡状态，`0` 未知，`1` 有卡，`2` 无卡 |
| tempdata | string | 主板温度 |
| xPoint | string | 纬度，高德坐标 |
| yPoint | string | 经度，高德坐标 |
| ch4 | string | 甲烷浓度 |
| o2 | string | 氧气浓度 |
| co | string | 一氧化碳浓度 |
| h2s | string | 硫化氢浓度 |
| createTime | string | 创建时间 |
| updateTime | string | 更新时间 |
| remark | string | 扩展字段，可能保存手表体征 JSON |

## 10. 告警接口

### 10.1 查询告警列表

```http
GET /system/device/alarm/list?pageNum=1&pageSize=10&deviceId=CAPS-001&type=1
Authorization: Bearer {token}
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| pageNum | number | 否 | 页码 |
| pageSize | number | 否 | 每页条数 |
| deviceId | string | 否 | 设备 ID |
| type | number | 否 | 告警类型 |
| typeInfo | string | 否 | 告警信息 |

响应示例：

```json
{
  "code": 200,
  "msg": "查询成功",
  "total": 1,
  "rows": [
    {
      "id": 100,
      "deviceId": "CAPS-001",
      "type": 1,
      "typeInfo": "SOS告警",
      "xPoint": "39.904200",
      "yPoint": "116.407396",
      "createTime": "2026-06-01 10:10:20"
    }
  ]
}
```

### 10.2 查询告警详情

```http
GET /system/device/alarm/{id}
Authorization: Bearer {token}
```

响应示例：

```json
{
  "code": 200,
  "msg": "操作成功",
  "data": {
    "id": 100,
    "deviceId": "CAPS-001",
    "type": 1,
    "typeInfo": "SOS告警",
    "xPoint": "39.904200",
    "yPoint": "116.407396",
    "createTime": "2026-06-01 10:10:20"
  }
}
```

## 11. 气体检测数据上报接口

### 11.1 上报气体检测数据

```http
POST /caps/api/gas/report
Content-Type: application/json
```

请求体：

```json
{
  "sn": "GAS-001",
  "list": [
    {
      "dict_id": "a19001",
      "val": 20.9,
      "flag": 0,
      "unit": "%",
      "time": 1780299020
    },
    {
      "dict_id": "a21005",
      "val": 2.5,
      "flag": 0,
      "unit": "ppm",
      "time": 1780299020
    }
  ]
}
```

请求字段：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| sn | string | 否 | 气体设备 SN。当前实现中暂未按 SN 匹配设备 |
| list | array | 是 | 气体检测项列表 |
| list[].dict_id | string | 是 | 气体指标编码 |
| list[].val | number | 是 | 指标值 |
| list[].flag | number | 否 | 状态标记，按设备协议传递 |
| list[].unit | string | 否 | 单位 |
| list[].time | number | 否 | 秒级 Unix 时间戳 |

气体指标编码：

| 气体 | dict_id | 字段 |
| --- | --- | --- |
| 氧气 O2 | `a19001` | `o2` |
| 一氧化碳 CO | `a21005` | `co` |
| 硫化氢 H2S | `a21028` | `h2s` |
| 甲烷 CH4 | `qt00001` | `ch4` |

成功响应：

```json
{
  "status": true,
  "msg": "gas report received",
  "msg_code": "GetSucc",
  "data": 2
}
```

失败响应：

```json
{
  "status": false,
  "msg": "no supported gas dict_id found",
  "msg_code": "GetErr"
}
```

当前实现说明：

- `list` 不能为空。
- 至少包含一个支持的 `dict_id`。
- 当前实现会把气体值同步更新到系统中已有的设备实时数据记录。
- 若需严格按 `sn` 绑定某个气体设备或人员，需要新增气体设备与人员/安全帽的映射表。

## 12. 声光报警器控制接口

### 12.1 打开人员关联声光报警器

```http
GET /caps/api/openHuaheDevice/{personId}
```

路径参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| personId | string | 是 | 人员 ID |

响应示例：

```json
{
  "status": true,
  "msg": "success",
  "msg_code": "GetSucc"
}
```

### 12.2 关闭人员关联声光报警器

```http
GET /caps/api/closeHuaheDevice/{personId}
```

响应示例：

```json
{
  "status": true,
  "msg": "success",
  "msg_code": "GetSucc"
}
```

安全建议：

- 控制类接口必须加鉴权和 IP 白名单。
- 建议记录操作人、来源系统、来源 IP、人员 ID、设备 ID、操作时间和结果。
- 建议对重复控制请求做幂等处理。

## 13. 视频通话控制接口

### 13.1 发起视频通话

```http
POST /caps/api/openDialog
Content-Type: application/x-www-form-urlencoded
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| stream | string | 是 | 发起方视频流 ID |
| code | string | 是 | 发起方编码，可为用户编码或终端编码 |
| deviceId | string | 是 | 被呼叫设备 ID |

请求示例：

```bash
curl -X POST "http://{CAPS_API_HOST}:5010/caps/api/openDialog" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "stream=DISPATCH-001" \
  -d "code=USER-001" \
  -d "deviceId=CAPS-001"
```

成功响应：

```json
{
  "status": true,
  "msg": "openDialog success",
  "msg_code": "GetSucc"
}
```

失败响应：

```json
{
  "status": false,
  "msg": "device websocket session not found",
  "msg_code": "GetErr"
}
```

说明：

- 被呼叫设备必须在线，并且存在 WebSocket 会话。
- 系统会向设备下发 `server_push_webrtc_call` 消息。
- 房间号规则为 `{deviceId}-{code}-{stream}`。

### 13.2 关闭视频通话

```http
POST /caps/api/closeDialog
Content-Type: application/x-www-form-urlencoded
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| deviceId | string | 是 | 被呼叫设备 ID |
| code | string | 是 | 发起方编码 |
| stream | string | 是 | 发起方视频流 ID |

请求示例：

```bash
curl -X POST "http://{CAPS_API_HOST}:5010/caps/api/closeDialog" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "deviceId=CAPS-001" \
  -d "code=USER-001" \
  -d "stream=DISPATCH-001"
```

成功响应：

```json
{
  "status": true,
  "msg": "closeDialog success",
  "msg_code": "GetSucc"
}
```

## 14. 设备配置接口，设备端使用

该接口当前主要面向安全帽或终端设备，不建议直接作为电厂内网业务接口。

### 14.1 获取设备配置

```http
GET /caps/api/index.php?device_id=CAPS-001&ca_ver=1.0
```

响应结构示例：

```json
{
  "status": true,
  "msg": "success",
  "msg_code": "GetSucc",
  "data": {
    "account_id": "11444",
    "config": "{...}",
    "notice_switch": "{...}",
    "upgrade": {
      "check_upgrade": 0,
      "new_apk_version": 0,
      "new_apk_url": "",
      "file_md5": ""
    },
    "time": 1780299020
  }
}
```

### 14.2 检查推流状态

```http
POST /caps/api/index.php
Content-Type: application/x-www-form-urlencoded
```

请求参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| ctl | string | 是 | 固定为 `device` |
| act | string | 是 | 固定为 `check_stream` |
| user_id | string | 是 | 设备端用户 ID |
| ca_ver | string | 否 | 版本 |

响应示例：

```json
{
  "status": true,
  "msg": "正在推流",
  "msg_code": "GetSucc",
  "data": {
    "push": true,
    "actual_push": true,
    "stream_state": "PUSHING",
    "reason": "actual_push",
    "grace_remaining_ms": 0
  }
}
```

## 15. 建议新增的正式开放接口

为避免直接暴露后台管理接口，建议新增 `CapsOpenApiController`，统一路径：

```text
/openapi/caps/**
```

推荐接口清单：

| 接口 | 方法 | 说明 |
| --- | --- | --- |
| `/openapi/caps/devices` | GET | 查询设备列表 |
| `/openapi/caps/devices/{deviceId}` | GET | 按设备 ID 查询设备详情 |
| `/openapi/caps/persons/{personId}/devices` | GET | 查询人员绑定设备 |
| `/openapi/caps/persons/{personId}/devices` | PUT | 更新人员绑定设备 |
| `/openapi/caps/reports/latest` | GET | 查询设备最新实时数据 |
| `/openapi/caps/persons/{personId}/vitals/latest` | GET | 查询人员最新体征数据 |
| `/openapi/caps/alarms` | GET | 查询告警列表 |
| `/openapi/caps/gas/reports` | POST | 上报气体检测数据 |
| `/openapi/caps/audible-alarm/open` | POST | 打开声光报警器 |
| `/openapi/caps/audible-alarm/close` | POST | 关闭声光报警器 |
| `/openapi/caps/video/calls` | POST | 发起视频通话 |
| `/openapi/caps/video/calls/close` | POST | 关闭视频通话 |

开放接口建议统一返回：

```json
{
  "success": true,
  "code": "0",
  "message": "success",
  "data": {},
  "traceId": "202606011010200001"
}
```

失败返回：

```json
{
  "success": false,
  "code": "CAPS_PARAM_ERROR",
  "message": "deviceId不能为空",
  "data": null,
  "traceId": "202606011010200002"
}
```

## 16. 安全要求

正式接入电厂内网时，建议满足以下要求：

1. 只开放必要接口和端口。
2. 接口入口统一走 Nginx/API 网关。
3. 开启 HTTPS，内部测试环境至少限制访问网段。
4. 配置电厂系统服务器 IP 白名单。
5. 控制类接口必须启用接口签名。
6. 禁止在 URL 中传输密码、永久 token 或密钥。
7. 所有接口调用记录审计日志。
8. 对查询接口做分页限制，避免一次性拉取过大数据。
9. 对控制接口做频率限制和幂等处理。
10. 对外接口与后台管理接口分离。
11. 账号、密钥、证书定期轮换。
12. 异常信息不得返回数据库 SQL、服务器路径、堆栈信息。

## 17. Nginx 代理建议

示例配置：

```nginx
server {
    listen 443 ssl;
    server_name caps-api.powerplant.local;

    ssl_certificate     /etc/nginx/certs/caps-api.crt;
    ssl_certificate_key /etc/nginx/certs/caps-api.key;

    location /openapi/ {
        proxy_pass http://127.0.0.1:5010/openapi/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }

    location /websocket/ {
        proxy_pass http://127.0.0.1:5010/websocket/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }
}
```

## 18. 联调步骤

### 18.1 网络联调

1. 电厂系统服务器访问 `http://{CAPS_API_HOST}:5010/`。
2. 电厂用户浏览器访问指挥箱 Web 首页。
3. 确认 DNS、路由、防火墙、NAT、网关策略。
4. 确认 WebSocket 是否能正常连接。
5. 如果涉及视频，确认 WebRTC/RTMP 所需端口可达。

### 18.2 接口联调

1. 获取接口密钥或测试账号。
2. 调用查询设备列表接口。
3. 调用查询实时数据接口。
4. 调用查询告警列表接口。
5. 调用人员设备绑定更新接口。
6. 调用气体检测上报接口。
7. 若现场有设备在线，测试声光报警器控制。
8. 若现场有视频设备在线，测试发起和关闭视频通话。

### 18.3 页面跳转联调

1. 从电厂内网系统配置普通跳转入口。
2. 验证浏览器能打开指挥箱 Web 首页。
3. 配置携带 `deviceId` 或 `personId` 的业务跳转。
4. 若需要免登录，联调 SSO token。
5. 验证退出登录、会话超时、无权限访问等场景。

## 19. 验收标准

| 项目 | 验收要求 |
| --- | --- |
| 网络访问 | 电厂内网用户可访问指挥箱 Web |
| 接口认证 | 未携带有效认证信息时接口拒绝访问 |
| 设备查询 | 可按设备 ID、人员 ID 查询设备 |
| 实时数据 | 可查询定位、电量、佩戴状态、气体数据 |
| 告警数据 | 可查询告警列表和详情 |
| 数据上报 | 气体检测数据可写入并反映到实时数据 |
| 控制接口 | 声光报警器和视频通话控制结果明确 |
| 日志审计 | 能追踪来源 IP、调用方、接口、参数摘要和结果 |
| 异常处理 | 参数错误、认证失败、设备离线等场景返回明确 |
| 安全策略 | 白名单、HTTPS、密钥、权限策略满足电厂要求 |

## 20. 待确认事项

正式实施前需要与电厂系统方确认：

| 序号 | 事项 | 说明 |
| --- | --- | --- |
| 1 | 访问方式 | 电厂系统访问指挥箱，还是指挥箱主动推送到电厂系统 |
| 2 | 网络地址 | 指挥箱固定 IP、域名、端口、网段 |
| 3 | 认证方式 | JWT、接口密钥、HMAC 签名、双向证书 |
| 4 | 人员主键 | 使用电厂人员 ID、工号，还是本系统 personId |
| 5 | 设备主键 | 使用 deviceId、SN，还是资产编号 |
| 6 | 数据频率 | 实时数据查询频率、上报频率 |
| 7 | 告警类型 | 告警编码、等级、处置状态是否需要统一字典 |
| 8 | 视频要求 | 是否需要嵌入电厂系统页面播放视频 |
| 9 | SSO | 是否需要免登录跳转 |
| 10 | 日志留存 | 审计日志保留周期和字段 |

## 21. 附录：curl 示例

### 21.1 查询设备列表

```bash
curl "http://{CAPS_API_HOST}:5010/system/device/list?pageNum=1&pageSize=10" \
  -H "Authorization: {apiSecret}"
```

### 21.2 更新人员设备绑定

```bash
curl -X POST "http://{CAPS_API_HOST}:5010/system/device/updatePersonIdAndDeviceInfo" \
  -H "Content-Type: application/json" \
  -H "Authorization: {apiSecret}" \
  -d '{
    "personId": "P10001",
    "deviceIds": ["CAPS-001", "WATCH-001"],
    "audibleIds": ["ALARM-001"]
  }'
```

### 21.3 查询实时数据

```bash
curl "http://{CAPS_API_HOST}:5010/system/report/list?pageNum=1&pageSize=10&deviceId=CAPS-001" \
  -H "Authorization: Bearer {token}"
```

### 21.4 上报气体检测数据

```bash
curl -X POST "http://{CAPS_API_HOST}:5010/caps/api/gas/report" \
  -H "Content-Type: application/json" \
  -d '{
    "sn": "GAS-001",
    "list": [
      {"dict_id": "a19001", "val": 20.9, "flag": 0, "unit": "%", "time": 1780299020},
      {"dict_id": "a21005", "val": 2.5, "flag": 0, "unit": "ppm", "time": 1780299020},
      {"dict_id": "a21028", "val": 0.01, "flag": 0, "unit": "ppm", "time": 1780299020},
      {"dict_id": "qt00001", "val": 0.0, "flag": 0, "unit": "%LEL", "time": 1780299020}
    ]
  }'
```

### 21.5 打开声光报警器

```bash
curl "http://{CAPS_API_HOST}:5010/caps/api/openHuaheDevice/P10001"
```

### 21.6 发起视频通话

```bash
curl -X POST "http://{CAPS_API_HOST}:5010/caps/api/openDialog" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "stream=DISPATCH-001" \
  -d "code=USER-001" \
  -d "deviceId=CAPS-001"
```

## 22. 实施建议

第一阶段先完成网络打通和普通页面跳转。  
第二阶段封装 `/openapi/caps/**` 正式开放接口。  
第三阶段增加 SSO、接口签名、IP 白名单、审计日志和限流。  
第四阶段根据电厂侧需要扩展告警处置、视频嵌入、数据主动推送等能力。
