#!/usr/bin/env pwsh
<#
.SYNOPSIS
Script para crear y configurar la base de datos PostgreSQL local para SIGAC

.DESCRIPTION
Crea la base de datos sigac_dev y la estructura inicial en PostgreSQL local.

.EXAMPLE
.\scripts\setup-database.ps1
#>

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SIGAC - Setup de Base de Datos PostgreSQL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Variables
$dbName = "sigac_dev"
$dbUser = "postgres"
$dbHost = "localhost"
$dbPort = 5432

Write-Host "Conectando a PostgreSQL en $dbHost`:$dbPort..." -ForegroundColor Yellow
Write-Host ""

# Solicitar password de postgres
$dbPassword = Read-Host "Ingresa la password del usuario postgres" -AsSecureString
$dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($dbPassword))

Write-Host ""
Write-Host "Creando base de datos '$dbName'..." -ForegroundColor Yellow

# Crear la base de datos usando psql
$env:PGPASSWORD = $dbPasswordPlain

try {
    # Verificar si psql está disponible
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        Write-Host "ERROR: psql no se encontró en el PATH" -ForegroundColor Red
        Write-Host "Asegúrate de que PostgreSQL esté instalado y añadido al PATH" -ForegroundColor Yellow
        exit 1
    }

    # Verificar conexión a PostgreSQL
    psql -h $dbHost -U $dbUser -p $dbPort -c "SELECT 1" | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Conexión a PostgreSQL establecida" -ForegroundColor Green
    } else {
        Write-Host "✗ No se pudo conectar a PostgreSQL" -ForegroundColor Red
        exit 1
    }

    # Verificar si la base de datos ya existe
    $dbExists = psql -h $dbHost -U $dbUser -p $dbPort -lqt | Select-String $dbName

    if ($dbExists) {
        Write-Host "✓ Base de datos '$dbName' ya existe" -ForegroundColor Green
    } else {
        # Crear la base de datos
        psql -h $dbHost -U $dbUser -p $dbPort -c "CREATE DATABASE $dbName;" | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Base de datos '$dbName' creada" -ForegroundColor Green
        } else {
            Write-Host "✗ Error al crear la base de datos" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✓ Base de datos configurada exitosamente" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Yellow
    Write-Host "  1. Ejecuta: dotnet ef database update (en HackerRank1/)" -ForegroundColor Gray
    Write-Host "  2. Inicia la app: dotnet run" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Connection string: Host=$dbHost;Database=$dbName;Username=$dbUser;Password=TU_PASSWORD;SSL Mode=Disable" -ForegroundColor Cyan
    Write-Host ""

} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    exit 1
} finally {
    $env:PGPASSWORD = $null
}
