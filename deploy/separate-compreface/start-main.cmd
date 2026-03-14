@echo off
setlocal EnableExtensions

set "STACK_DIR=%~dp0"
for %%I in ("%STACK_DIR%..\..") do set "ROOT_DIR=%%~fI"
set "ENV_FILE=%STACK_DIR%.env.main"
set "ENV_TEMPLATE=%STACK_DIR%.env.main.example"
set "COMPOSE_FILE=%STACK_DIR%docker-compose.main.yml"

if not exist "%ENV_FILE%" (
  copy "%ENV_TEMPLATE%" "%ENV_FILE%" >nul
  echo Created %ENV_FILE% from template. Update credentials before production use.
)

docker compose --project-directory "%ROOT_DIR%" -f "%COMPOSE_FILE%" --env-file "%ENV_FILE%" up -d --pull never
if errorlevel 1 exit /b %errorlevel%

docker compose --project-directory "%ROOT_DIR%" -f "%COMPOSE_FILE%" --env-file "%ENV_FILE%" ps

endlocal
