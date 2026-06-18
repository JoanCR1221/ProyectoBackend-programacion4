#!/usr/bin/env pwsh
<#
.SYNOPSIS
Script de inicialización de secretos locales para desarrollo SIGAC

.DESCRIPTION
Este script configura los User Secrets de .NET para que cada desarrollador
tenga sus propias credenciales sin committearlas al repositorio.

.EXAMPLE
.\scripts\setup-secrets.ps1
#>

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SIGAC - Configuración de Secretos Locales" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cambiar a la carpeta del proyecto
$projectPath = ".\HackerRank1"
Set-Location $projectPath

# Inicializar user-secrets si no existen
Write-Host "Inicializando User Secrets para el proyecto..." -ForegroundColor Yellow
dotnet user-secrets init --force 2>$null

Write-Host ""
Write-Host "Ingresa tus secretos locales:" -ForegroundColor Cyan
Write-Host ""

# Connection String
Write-Host "1. CONNECTION STRING (Supabase PostgreSQL)" -ForegroundColor Green
Write-Host "   Ejemplo: Host=localhost;Database=sigac_dev;Username=postgres;Password=tu_password;SSL Mode=Disable" -ForegroundColor Gray
$connectionString = Read-Host "   Ingresa la connection string"
if ($connectionString) {
    dotnet user-secrets set "ConnectionStrings:Supabase" "$connectionString"
    Write-Host "   ✓ Connection string configurado" -ForegroundColor Green
} else {
    Write-Host "   ✗ Connection string no configurado (requerido)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# JWT Secret Key
Write-Host "2. JWT SECRET KEY (mínimo 32 caracteres)" -ForegroundColor Green
Write-Host "   Ejemplo: MiSuperSecretoParaDesarrolloLocal1234567890" -ForegroundColor Gray
$jwtSecret = Read-Host "   Ingresa el JWT Secret"
if ($jwtSecret.Length -lt 32) {
    Write-Host "   ✗ El secret debe tener mínimo 32 caracteres" -ForegroundColor Red
    exit 1
} else {
    dotnet user-secrets set "JwtSettings:SecretKey" "$jwtSecret"
    Write-Host "   ✓ JWT Secret configurado" -ForegroundColor Green
}

Write-Host ""

# Email del Superusuario
Write-Host "3. EMAIL DEL SUPERUSUARIO" -ForegroundColor Green
Write-Host "   Ejemplo: super@sigac.cr" -ForegroundColor Gray
$superEmail = Read-Host "   Ingresa el email"
if ($superEmail) {
    dotnet user-secrets set "SuperusuarioSeed:Email" "$superEmail"
    Write-Host "   ✓ Email del superusuario configurado" -ForegroundColor Green
}

Write-Host ""

# Password del Superusuario
Write-Host "4. PASSWORD DEL SUPERUSUARIO" -ForegroundColor Green
Write-Host "   (Se usará al ejecutar las migraciones)" -ForegroundColor Gray
$superPassword = Read-Host "   Ingresa la password" -AsSecureString
$superPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($superPassword))
if ($superPasswordPlain) {
    dotnet user-secrets set "SuperusuarioSeed:Password" "$superPasswordPlain"
    Write-Host "   ✓ Password del superusuario configurada" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Configuración completada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Los secretos se guardaron en:" -ForegroundColor Cyan
Write-Host "  $env:APPDATA\Microsoft\UserSecrets\<project-id>\secrets.json" -ForegroundColor Gray
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Ejecuta las migraciones: dotnet ef database update" -ForegroundColor Gray
Write-Host "  2. Inicia la aplicación: dotnet run" -ForegroundColor Gray
Write-Host ""
