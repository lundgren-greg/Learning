param(
    [switch]$Restore
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$backendPath = Join-Path $repoRoot "backend\PermissionsApi"
$backendProjectFile = Join-Path $backendPath "PermissionsApi.csproj"

if (-not (Test-Path $backendProjectFile)) {
    throw "Could not find backend project at '$backendPath'."
}

if ($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    throw "The .NET SDK is required but was not found in PATH."
}

Push-Location $backendPath
try {
    Write-Host "Starting backend from $backendPath with launch profile 'http'..."

    if ($Restore) {
        dotnet restore
    }

    dotnet run --launch-profile http
}
finally {
    Pop-Location
}

