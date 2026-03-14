$ErrorActionPreference = "Stop"

$stackDir = $PSScriptRoot
$rootDir = (Resolve-Path (Join-Path $stackDir "..\..")).Path
$envFile = Join-Path $stackDir ".env.compreface"
$envTemplate = Join-Path $stackDir ".env.compreface.example"
$composeFile = Join-Path $stackDir "docker-compose.compreface.yml"

if (-not (Test-Path $envFile)) {
  Copy-Item $envTemplate $envFile
}

docker compose `
  --project-directory $rootDir `
  -f $composeFile `
  --env-file $envFile `
  down
