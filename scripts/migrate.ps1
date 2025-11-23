#!/usr/bin/env pwsh
# Database migration script for .NET Boilerplate

param(
    [string]$Action = "update",
    [string]$Name = ""
)

$InfraProject = "src/Infrastructure/Infrastructure.csproj"
$StartupProject = "src/Web/Web.csproj"

Write-Host "Database Migration Tool" -ForegroundColor Green

switch ($Action.ToLower()) {
    "add" {
        if ([string]::IsNullOrWhiteSpace($Name)) {
            Write-Host "Error: Migration name is required for 'add' action" -ForegroundColor Red
            Write-Host "Usage: .\scripts\migrate.ps1 -Action add -Name YourMigrationName" -ForegroundColor Yellow
            exit 1
        }
        Write-Host "`nAdding migration: $Name" -ForegroundColor Yellow
        dotnet ef migrations add $Name --project $InfraProject --startup-project $StartupProject
    }
    "remove" {
        Write-Host "`nRemoving last migration..." -ForegroundColor Yellow
        dotnet ef migrations remove --project $InfraProject --startup-project $StartupProject
    }
    "update" {
        Write-Host "`nApplying migrations to database..." -ForegroundColor Yellow
        dotnet ef database update --project $InfraProject --startup-project $StartupProject
    }
    "list" {
        Write-Host "`nListing migrations..." -ForegroundColor Yellow
        dotnet ef migrations list --project $InfraProject --startup-project $StartupProject
    }
    default {
        Write-Host "Invalid action: $Action" -ForegroundColor Red
        Write-Host "Valid actions: add, remove, update, list" -ForegroundColor Yellow
        exit 1
    }
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nMigration operation failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nMigration completed successfully!" -ForegroundColor Green
