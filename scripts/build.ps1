#!/usr/bin/env pwsh
# Build script for .NET Boilerplate

param(
    [string]$Configuration = "Release"
)

Write-Host "Building .NET Boilerplate..." -ForegroundColor Green

# Restore dependencies
Write-Host "`nRestoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore dependencies" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
