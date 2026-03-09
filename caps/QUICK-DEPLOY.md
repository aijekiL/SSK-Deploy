# CAPS 应用快速部署指引（达梦版）

## 前提条件
- 树莓派服务器 172.28.69.229 已安装 Docker 与 Docker Compose
- 可访问的达梦数据库（容器 `damengofficial/dm8` 或外部实例）
- 项目已编译生成可运行的 JAR 包

## 快速部署步骤

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

### 4. 默认部署（含达梦容器）
```bash
chmod +x deploy.sh
./deploy.sh
```

### 5. 复用现有达梦实例
```bash
chmod +x deploy-existing-dm.sh
./deploy-existing-dm.sh
```
> 通过 `.env` 或导出环境变量覆盖 `SPRING_DATASOURCE_*`，即可指向已有库。

### 6. 验证部署
```bash
# 查看容器
docker ps

# 查看应用日志
docker logs caps-app

# 健康检查
curl http://172.28.69.229:5010
```

## 服务访问地址
- 应用：http://172.28.69.229:5010
- 达梦：172.28.69.229:5236（缺省 schema：SYSDBA）
- Redis：172.28.69.229:6379

## 常用管理命令（使用外部达梦场景）
```bash
docker-compose -f docker-compose-existing-dm.yml ps
docker-compose -f docker-compose-existing-dm.yml logs -f caps-app
docker-compose -f docker-compose-existing-dm.yml restart caps-app
docker-compose -f docker-compose-existing-dm.yml down
docker-compose -f docker-compose-existing-dm.yml up -d --build
```

## 故障排除

### 应用无法启动
```bash
docker-compose -f docker-compose-existing-dm.yml logs caps-app
```

### 达梦连接失败
```bash
# 确认数据库端口可达
nc -vz 172.28.69.229 5236

# 检查容器日志（如本地达梦）
docker-compose logs dmdb
```
确认 `SPRING_DATASOURCE_URL/USERNAME/PASSWORD/SCHEMA` 与实例一致。

### 端口冲突
```bash
netstat -tlnp | grep -E '5010|5236|6379'
```
若被占用，可在 compose 文件中修改宿主机端口。

## 数据备份
- 使用达梦 `dmexp/dmimp` 或 `dmrman` 工具备份业务 schema
- 示例：
```bash
docker exec caps-dmdb /opt/dmdbms/bin/dmexp CAPS_USER/CAPS_PASSWORD DIRECTORY=/opt/dmdbms/backup FILE=caps.dmp
```
- 应用上传目录：`docker cp caps-app:/app/uploadPath ./backup_upload`

## 性能监控
```bash
docker stats
htop
```

## 安全建议
1. 修改默认数据库、Redis 口令
2. 仅暴露必要端口，结合防火墙或安全组
3. 定期同步 SQL 脚本（`sql/dm-*.sql`）与备份
4. 监控系统资源与应用日志
