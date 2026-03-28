param(
    [switch]$Install,
    [ValidateSet("auto", "npm", "pnpm")]
    [string]$PackageManager = "auto"
)

$ErrorActionPreference = "Stop"

function Test-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontendPath = Join-Path $repoRoot "frontend"
$frontendPackageJson = Join-Path $frontendPath "package.json"

if (-not (Test-Path $frontendPackageJson)) {
    throw "Could not find frontend project at '$frontendPath'."
}

if (-not (Test-Command -Name "node")) {
    throw "Node.js is required but was not found in PATH."
}

$selectedPackageManager = $PackageManager
if ($selectedPackageManager -eq "auto") {
    $pnpmLock = Join-Path $frontendPath "pnpm-lock.yaml"
    if ((Test-Path $pnpmLock) -and (Test-Command -Name "pnpm")) {
        $selectedPackageManager = "pnpm"
    }
    else {
        $selectedPackageManager = "npm"
    }
}

if (-not (Test-Command -Name $selectedPackageManager)) {
    throw "Package manager '$selectedPackageManager' was not found in PATH."
}

Push-Location $frontendPath
try {
    Write-Host "Starting frontend from $frontendPath using $selectedPackageManager..."

    if ($Install) {
        if ($selectedPackageManager -eq "pnpm") {
            pnpm install
        }
        else {
            npm install
        }
    }

    if ($selectedPackageManager -eq "pnpm") {
        pnpm run dev
    }
    else {
        npm run dev
    }
}
finally {
    Pop-Location
}

