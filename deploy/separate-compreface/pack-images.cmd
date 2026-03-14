@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "STACK_DIR=%~dp0"
for %%I in ("%STACK_DIR%..\..") do set "ROOT_DIR=%%~fI"

set "ENV_COMP=%STACK_DIR%.env.compreface"
set "ENV_COMP_EX=%STACK_DIR%.env.compreface.example"
set "ENV_MAIN=%STACK_DIR%.env.main"
set "ENV_MAIN_EX=%STACK_DIR%.env.main.example"

if not exist "%ENV_COMP%" (
  copy "%ENV_COMP_EX%" "%ENV_COMP%" >nul
  echo Created %ENV_COMP% from template.
)

if not exist "%ENV_MAIN%" (
  copy "%ENV_MAIN_EX%" "%ENV_MAIN%" >nul
  echo Created %ENV_MAIN% from template.
)

set "OUT_DIR=%STACK_DIR%images"
set "LIST_FILE=%OUT_DIR%\images.txt"
set "LIST_UNIQ=%OUT_DIR%\images.unique.txt"
set "TAR_FILE=%OUT_DIR%\all-images.tar"

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

> "%LIST_FILE%" (
  docker compose --project-directory "%ROOT_DIR%" -f "%STACK_DIR%docker-compose.compreface.yml" --env-file "%ENV_COMP%" config --images
  docker compose --project-directory "%ROOT_DIR%" -f "%STACK_DIR%docker-compose.main.yml" --env-file "%ENV_MAIN%" config --images
)
if errorlevel 1 exit /b %errorlevel%

sort "%LIST_FILE%" /unique > "%LIST_UNIQ%"
if errorlevel 1 exit /b %errorlevel%

set "IMAGES="
for /f "usebackq delims=" %%I in ("%LIST_UNIQ%") do (
  if not "%%I"=="" (
    docker image inspect "%%I" >nul 2>&1
    if errorlevel 1 (
      echo Pulling %%I
      docker pull "%%I"
      if errorlevel 1 exit /b %errorlevel%
    )
    set "IMAGES=!IMAGES! %%I"
  )
)

if "!IMAGES!"=="" (
  echo No images resolved from docker compose files.
  exit /b 1
)

docker save -o "%TAR_FILE%" !IMAGES!
if errorlevel 1 exit /b %errorlevel%

echo Saved images to %TAR_FILE%
endlocal
