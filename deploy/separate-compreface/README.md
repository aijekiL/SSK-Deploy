# Separate Deployment Plan: Main Stack + CompreFace

This folder provides a split deployment model:

- `docker-compose.main.yml`: all services except CompreFace
- `docker-compose.compreface.yml`: CompreFace only

The split reduces memory contention and allows independent lifecycle management.

## Files

- `docker-compose.main.yml`
- `docker-compose.compreface.yml`
- `.env.main.example`
- `.env.compreface.example`
- `start-main.ps1` / `stop-main.ps1`
- `start-compreface.ps1` / `stop-compreface.ps1`
- `start-main.cmd` / `stop-main.cmd`
- `start-compreface.cmd` / `stop-compreface.cmd`
- `pack-images.cmd`

## 1. Prepare env files

In this folder:

```cmd
copy /Y .env.main.example .env.main
copy /Y .env.compreface.example .env.compreface
```

Required edits:

- `.env.main`
  - `SSK_COMPREFACE_API_KEY`
  - `COMPREFACE_BASE_URL`
  - DB passwords and ports
- `.env.compreface`
  - PostgreSQL password if needed
  - `COMPREFACE_UI_PORT` if `8001` is occupied

## 2. Start CompreFace first

```cmd
start-compreface.cmd
```

Check status:

```cmd
docker compose --project-directory ..\.. -f .\docker-compose.compreface.yml --env-file .\.env.compreface ps
```

After startup, open `http://<host-ip>:<COMPREFACE_UI_PORT>` and generate a Recognition API key.

## 3. Update main stack API key

Set the generated key in `.env.main`:

```text
SSK_COMPREFACE_API_KEY=<your-recognition-key>
```

If main stack and CompreFace run on the same Windows Docker Desktop host:

```text
COMPREFACE_BASE_URL=http://host.docker.internal:8001
```

If CompreFace runs on a different host:

```text
COMPREFACE_BASE_URL=http://<compreface-host-ip>:8001
```

## 4. Start main stack

```cmd
start-main.cmd
```

## 5. Stop stacks independently

```cmd
stop-main.cmd
stop-compreface.cmd
```

## Offline image packaging reference

Generate image lists:

```cmd
docker compose --project-directory ..\.. -f .\docker-compose.compreface.yml --env-file .\.env.compreface config --images
docker compose --project-directory ..\.. -f .\docker-compose.main.yml --env-file .\.env.main config --images
```

Export all required images into one tar file:

```cmd
pack-images.cmd
```

On the target machine:

```cmd
docker load -i .\images\all-images.tar
```
