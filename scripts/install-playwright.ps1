#!/usr/bin/env pwsh
param()

Write-Host "Creating/ensuring a local dotnet tool manifest and installing Microsoft.Playwright.CLI..."

# Ensure we run from the repository root (script expects to be run from solution root)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Move to repository root (parent of scripts folder)
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
Set-Location -Path $repoRoot

if (-not (Test-Path .config)) {
	New-Item -ItemType Directory -Path .config | Out-Null
}

dotnet new tool-manifest --force

Write-Host "Installing Microsoft.Playwright.CLI (local tool)..."
try {
	# Latest available CLI on nuget.org is 1.2.3 (the CLI has a different versioning scheme)
	dotnet tool install Microsoft.Playwright.CLI --version 1.2.3 --add-source https://api.nuget.org/v3/index.json --local
} catch {
	Write-Host "Tool may already be installed or installation failed; continuing..."
}

Write-Host "Running Playwright install to download browser binaries..."
try {
	dotnet tool run playwright install
} catch {
	Write-Host "playwright install failed. If the local tool is installed, try: dotnet tool run playwright install" -ForegroundColor Yellow
}

Write-Host "Playwright installation step finished."
