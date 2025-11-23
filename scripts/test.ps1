#!/usr/bin/env pwsh
# Test script for .NET Boilerplate

param(
    [string]$Configuration = "Release",
    [switch]$UnitOnly,
    [switch]$IntegrationOnly
)

Write-Host "Running tests for .NET Boilerplate..." -ForegroundColor Green

if ($UnitOnly) {
    Write-Host "`nRunning unit tests..." -ForegroundColor Yellow
    dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --configuration $Configuration --verbosity normal
} elseif ($IntegrationOnly) {
    Write-Host "`nRunning integration tests..." -ForegroundColor Yellow
    dotnet test tests/Web.IntegrationTests/Web.IntegrationTests.csproj --configuration $Configuration --verbosity normal
} else {
    Write-Host "`nRunning all tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --verbosity normal
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nTests failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nAll tests passed!" -ForegroundColor Green
