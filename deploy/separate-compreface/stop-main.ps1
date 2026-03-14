$ErrorActionPreference = "Stop"

$stackDir = $PSScriptRoot
$rootDir = (Resolve-Path (Join-Path $stackDir "..\..")).Path
$envFile = Join-Path $stackDir ".env.main"
$envTemplate = Join-Path $stackDir ".env.main.example"
$composeFile = Join-Path $stackDir "docker-compose.main.yml"

if (-not (Test-Path $envFile)) {
  Copy-Item $envTemplate $envFile
}

docker compose `
  --project-directory $rootDir `
  -f $composeFile `
  --env-file $envFile `
  down
