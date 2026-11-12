# Auto-start on boot: start services then open the dashboard.
# Opens the URL in the DEFAULT browser profile, so the cached login is reused (no manual login).
# Keep this file in the same folder as the .bat, run via Task Scheduler at logon.

# ---- Config: adjust to your deployment ----
$Homepage    = 'https://192.168.2.234:3000/#/board'  # homepage URL
$ServerHost  = '192.168.2.234'                        # server IP
$ServerPort  = 3000                                   # frontend HTTPS port
$DeployDir   = 'E:\ssk'                               # docker-compose folder
$BrowserExe  = $null                                  # auto-detect Edge/Chrome if empty
$Fullscreen  = $false                                 # $true = open fullscreen (F11 style, still keeps profile & cache)
$StartDocker = $false                                 # $true = let this script run docker compose up
# ------------------------------------------

# 1) start docker stack (optional)
if ($StartDocker) {
    try {
        Set-Location $DeployDir
        docker compose -f docker-compose.main.yml --env-file .env.main up -d
        docker compose -f docker-compose.compreface.yml --env-file .env.compreface up -d
    } catch {
        Write-Output "docker start failed: $_"
    }
}

# 2) wait until homepage port is reachable (up to ~5 min)
$ready = $false
for ($i = 0; $i -lt 60; $i++) {
    $ok = Test-NetConnection -ComputerName $ServerHost -Port $ServerPort `
        -InformationLevel Quiet -WarningAction SilentlyContinue
    if ($ok) { $ready = $true; break }
    Start-Sleep -Seconds 5
}
if (-not $ready) { Write-Output "warning: homepage port $ServerPort not ready, opening anyway" }

# 3) locate browser
if (-not $BrowserExe) {
    $edgeCandidates = @(
        "$env:ProgramFiles(x86)\Microsoft\Edge\Application\msedge.exe",
        "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
    ) | Where-Object { Test-Path $_ }
    $chrome = "$env:ProgramFiles\Google\Chrome\Application\chrome.exe"
    if ($edgeCandidates) { $BrowserExe = $edgeCandidates[0] }
    elseif (Test-Path $chrome) { $BrowserExe = $chrome }
    else { $BrowserExe = 'msedge.exe' }
}

# 4) open homepage in the default profile (keeps cached login)
if ($Fullscreen) {
    # --start-fullscreen = F11-style fullscreen on the normal profile (does NOT isolate the session)
    Start-Process $BrowserExe -ArgumentList '--start-fullscreen', $Homepage
} else {
    Start-Process $BrowserExe -ArgumentList $Homepage
}