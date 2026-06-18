#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Script de configuración inicial para SIGAC Backend - Desarrollo Local
.DESCRIPTION
    Este script configura los user-secrets de .NET para desarrollo local,
    sin necesidad de commitear credenciales sensibles a git.
.NOTES
    Requiere .NET 8 SDK instalado
    Ejecutar desde PowerShell como: .\scripts\setup-dev-environment.ps1
#>

param(
    [string]$ProjectPath = "HackerRank1",
    [string]$DatabaseHost = "localhost",
    [string]$DatabasePort = "5432",
    [string]$DatabaseName = "sigac_dev",
    [string]$DatabaseUser = "postgres",
    [string]$DatabasePassword = "postgres",
    [string]$JwtSecretKey = "your-super-secret-key-minimum-32-characters-for-development-only!!!",
    [string]$SuperusuarioEmail = "super@sigac.cr",
    [string]$SuperusuarioPassword = "SuperusuarioSigac2024!"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SIGAC Backend - Configuración Local" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en la raíz del proyecto
if (-not (Test-Path "HackerRank1/HackerRank1.csproj")) {
    Write-Host "Error: Este script debe ejecutarse desde la raíz del proyecto" -ForegroundColor Red
    Write-Host "Ubicación esperada: ProyectoBackend-programacion4\" -ForegroundColor Red
    exit 1
}

# Inicializar user-secrets
Write-Host "[1/5] Inicializando user-secrets..." -ForegroundColor Yellow
cd $ProjectPath
$secretsInitOutput = dotnet user-secrets init 2>&1
if ($LASTEXITCODE -ne 0 -and -not ($secretsInitOutput -like "*already initialized*")) {
    Write-Host "Advertencia: " $secretsInitOutput -ForegroundColor Yellow
}
cd ..

# Configurar connection string
Write-Host "[2/5] Configurando connection string a PostgreSQL..." -ForegroundColor Yellow
$connectionString = "Host=$DatabaseHost;Port=$DatabasePort;Database=$DatabaseName;Username=$DatabaseUser;Password=$DatabasePassword;SSL Mode=Disable"
cd $ProjectPath
dotnet user-secrets set "ConnectionStrings:Supabase" $connectionString
cd ..

# Configurar JWT Secret
Write-Host "[3/5] Configurando JWT Secret Key..." -ForegroundColor Yellow
cd $ProjectPath
dotnet user-secrets set "JwtSettings:SecretKey" $JwtSecretKey
cd ..

# Configurar Superusuario
Write-Host "[4/5] Configurando credenciales de superusuario..." -ForegroundColor Yellow
cd $ProjectPath
dotnet user-secrets set "SuperusuarioSeed:Email" $SuperusuarioEmail
dotnet user-secrets set "SuperusuarioSeed:Password" $SuperusuarioPassword
cd ..

Write-Host "[5/5] Verificando configuración..." -ForegroundColor Yellow
cd $ProjectPath
$secrets = dotnet user-secrets list
cd ..

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "✓ Configuración completada" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Secrets configurados:" -ForegroundColor Cyan
Write-Host "  • ConnectionStrings:Supabase" -ForegroundColor Gray
Write-Host "  • JwtSettings:SecretKey" -ForegroundColor Gray
Write-Host "  • SuperusuarioSeed:Email" -ForegroundColor Gray
Write-Host "  • SuperusuarioSeed:Password" -ForegroundColor Gray
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Verificar que PostgreSQL esté corriendo en $DatabaseHost:$DatabasePort" -ForegroundColor Gray
Write-Host "  2. Crear la base de datos '$DatabaseName' si no existe" -ForegroundColor Gray
Write-Host "  3. Ejecutar: dotnet ef database update" -ForegroundColor Gray
Write-Host "  4. Ejecutar: dotnet run" -ForegroundColor Gray
Write-Host ""
