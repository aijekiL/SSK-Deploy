# CAPS 应用 Docker 部署指南（达梦版）

## 概述
本指南介绍如何在树莓派服务器（示例 IP：172.28.69.229）上，通过 Docker Compose 将 CAPS Spring Boot 应用与达梦数据库、Redis 一键部署。默认情况下：
- `docker-compose.yml` 会同时启动 `caps-dmdb`（达梦）、`caps-redis` 以及 `caps-app`。
- `docker-compose-existing-dm.yml` + `deploy-existing-dm.sh` 适用于已经有外部达梦实例的场景，只部署 Redis 与应用。

## 系统要求
- Docker 20.10+
- Docker Compose 2.0+
- 树莓派或其它 Linux 服务器（ARM/RISC-V/x86 均可）
- ≥2GB 内存、≥10GB 磁盘空间
- 可用的达梦数据库镜像（`damengofficial/dm8`）或一台已就绪的达梦实例

## 部署步骤

### 1. 编译项目
```bash
mvn clean package -DskipTests
```

### 2. 上传项目
```bash
scp -r ./caps user@172.28.69.229:/home/user/
```

### 3. 登录服务器
```bash
ssh user@172.28.69.229
cd /home/user/caps
```

### 4. 默认一体化部署（内置达梦）
```bash
chmod +x deploy.sh
./deploy.sh
```

### 5. 复用已有达梦实例
```bash
chmod +x deploy-existing-dm.sh
./deploy-existing-dm.sh
```
> 通过环境变量 `SPRING_DATASOURCE_URL/USERNAME/PASSWORD/SCHEMA` 或 `.env` 文件覆盖默认连接即可。

### 6. 手动部署（可选）
```bash
docker-compose build
docker-compose up -d
docker-compose ps
docker-compose logs -f caps-app
```

## 服务配置

### caps-app（Spring Boot）
- 端口：5010
- Profile：`druid`
- 数据源：`jdbc:dm://dmdb:5236/DAMENG?schema=SYSDBA`
- 上传路径、日志通过卷挂载到宿主机

### caps-dmdb（达梦数据库）
- 端口：5236
- Schema：默认 `SYSDBA`，也可在 `application-local.yml` 中切换至业务 schema（如 `CAPS_USER`）
- 初始化脚本：`sql/dm-init-db.sql`、`sql/dm-caps.sql`
- 数据持久化卷：`dmdb_data:/opt/dmdbms/data`

### caps-redis
- 端口：6379
- 数据持久化卷：`redis_data:/data`

## 数据持久化
- 达梦数据：`dmdb_data`
- Redis 数据：`redis_data`
- 应用日志：`app_logs`
- 上传文件：`app_upload`

## 常用命令
```bash
# 查看容器状态
docker-compose ps

# 查看应用日志
docker-compose logs -f caps-app

# 重启应用
docker-compose restart caps-app

# 停止所有服务
docker-compose down

# 重新构建并启动
docker-compose up -d --build

# 查看资源占用
docker stats
```

## 故障排除

### 1. 应用无法启动
```bash
docker-compose logs caps-app
```

### 2. 达梦连接失败
```bash
# 查看数据库容器日志
docker-compose logs dmdb

# 进入容器检查实例状态
docker exec -it caps-dmdb /opt/dmdbms/bin/disql SYSDBA/SYSDBA
```
确认 `application-*.yml` 中的 URL、schema 以及账号密码与实例一致。

### 3. Redis 连接失败
```bash
docker-compose logs redis
```

### 4. 端口冲突
修改 `docker-compose*.yml` 中的端口映射，例如 `5010:5010` → `15010:5010`。

### 5. 内存不足
- 调整 `Dockerfile` 中的 `JAVA_OPTS`
- 适当降低达梦 `PAGE_SIZE`
- 通过 `docker stats` 或 `htop` 观察资源

## 备份与恢复

### 1. 备份达梦
使用 `disql`/`dmrman`/`dmexp` 等官方工具，示例：
```bash
# 导出指定 schema
docker exec caps-dmdb /opt/dmdbms/bin/dmexp CAPS_USER/CAPS_PASSWORD DIRECTORY=/opt/dmdbms/backup FILE=caps.dmp LOG=caps.log
```

### 2. 恢复达梦
```bash
docker exec caps-dmdb /opt/dmdbms/bin/dmimp CAPS_USER/CAPS_PASSWORD DIRECTORY=/opt/dmdbms/backup FILE=caps.dmp LOG=caps_restore.log
```
> 具体参数请参阅达梦官方文档，可替换为 `dmrman` 全量备份方案。

### 3. 备份应用上传文件
```bash
docker cp caps-app:/app/uploadPath ./backup_upload
```

## 安全与优化建议
1. 修改默认数据库与 Redis 密码
2. 规划专用 schema 并最小化授权
3. 在反向代理层开启 HTTPS/访问控制
4. 定期备份达梦与上传数据
5. 结合 `ruoyi` 自带监控与 `docker stats` 观察性能

如需更多支持，请联系系统管理员或查看 `docker-compose logs -f caps-app` 获取第一手日志。
