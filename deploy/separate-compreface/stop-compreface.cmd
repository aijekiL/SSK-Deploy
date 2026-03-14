@echo off
setlocal EnableExtensions

set "STACK_DIR=%~dp0"
for %%I in ("%STACK_DIR%..\..") do set "ROOT_DIR=%%~fI"
set "ENV_FILE=%STACK_DIR%.env.compreface"
set "ENV_TEMPLATE=%STACK_DIR%.env.compreface.example"
set "COMPOSE_FILE=%STACK_DIR%docker-compose.compreface.yml"

if not exist "%ENV_FILE%" (
  copy "%ENV_TEMPLATE%" "%ENV_FILE%" >nul
)

docker compose --project-directory "%ROOT_DIR%" -f "%COMPOSE_FILE%" --env-file "%ENV_FILE%" down

endlocal
