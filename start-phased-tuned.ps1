param(
  [string]$EnvFile = ".env.tuned",
  [string]$ComposeFile = "docker-compose.tuned.yml"
)

$ErrorActionPreference = "Stop"

function Get-EnvValue {
  param(
    [string]$FilePath,
    [string]$Key
  )

  if (-not (Test-Path $FilePath)) {
    return $null
  }

  $line = Get-Content $FilePath | Where-Object { $_ -match "^$Key=" } | Select-Object -First 1
  if (-not $line) {
    return $null
  }
  return ($line -split "=", 2)[1].Trim()
}

function Wait-ContainersReady {
  param(
    [string[]]$ContainerNames,
    [int]$TimeoutSeconds = 180
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $pending = @()
    foreach ($name in $ContainerNames) {
      $status = docker inspect -f "{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}" $name 2>$null
      if ($LASTEXITCODE -ne 0 -or ($status -ne "healthy" -and $status -ne "running")) {
        $pending += $name
      }
    }

    if ($pending.Count -eq 0) {
      Write-Host "Ready: $($ContainerNames -join ', ')"
      return
    }

    Start-Sleep -Seconds 5
  }

  throw "Timeout waiting for containers: $($ContainerNames -join ', ')"
}

if (-not (Test-Path $ComposeFile)) {
  throw "Compose file not found: $ComposeFile"
}

if (-not (Test-Path $EnvFile)) {
  if (Test-Path ".env.windows-artifacts.tuned.example") {
    Copy-Item ".env.windows-artifacts.tuned.example" $EnvFile -Force
    Write-Host "Created $EnvFile from .env.windows-artifacts.tuned.example"
  } else {
    throw "Env file not found: $EnvFile"
  }
}

$comprefaceVolume = Get-EnvValue -FilePath $EnvFile -Key "COMPREFACE_POSTGRES_VOLUME"
if (-not $comprefaceVolume) {
  $comprefaceVolume = "compreface-120_postgres-data"
}
docker volume create $comprefaceVolume | Out-Null

Write-Host "Phase 1: base databases"
docker compose -f $ComposeFile --env-file $EnvFile up -d --pull never mysql mongodb redis compreface-postgres-db
Wait-ContainersReady -ContainerNames @("mysql", "mongodb", "redis", "compreface-postgres-db") -TimeoutSeconds 240

Write-Host "Phase 2: CompreFace core"
docker compose -f $ComposeFile --env-file $EnvFile up -d --pull never compreface-core
Wait-ContainersReady -ContainerNames @("compreface-core") -TimeoutSeconds 300

Write-Host "Phase 3: CompreFace API/UI"
docker compose -f $ComposeFile --env-file $EnvFile up -d --pull never compreface-api compreface-admin compreface-fe
Wait-ContainersReady -ContainerNames @("compreface-api", "compreface-admin", "compreface-ui") -TimeoutSeconds 240

Write-Host "Phase 4: business services"
docker compose -f $ComposeFile --env-file $EnvFile up -d --pull never zlmediakit caps netcore manage-frontend
Wait-ContainersReady -ContainerNames @("zlmediakit", "caps", "netcore", "manage-frontend") -TimeoutSeconds 240

docker compose -f $ComposeFile --env-file $EnvFile ps
