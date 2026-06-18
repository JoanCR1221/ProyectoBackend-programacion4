-- ================================================================
-- SIGAC Database Setup - PostgreSQL
-- ================================================================
-- Script OPCIONAL para crear la base de datos y usuario de desarrollo
-- 
-- IMPORTANTE: Este script usa credenciales por DEFECTO SOLO PARA DESARROLLO
-- En producción, usa contraseñas fuertes y gestión de secretos adecuada
-- 
-- Uso:
--   psql -U postgres -f SETUP_DATABASE.sql
-- ================================================================

-- Crear usuario sigac_dev si no existe
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_user
      WHERE usename = 'sigac_dev'
   )
   THEN
      CREATE ROLE sigac_dev WITH LOGIN PASSWORD 'sigac_dev_password_123';
   END IF;
END
$do$;

-- Crear base de datos sigac_dev
CREATE DATABASE sigac_dev
    OWNER sigac_dev
    ENCODING 'UTF8'
    LOCALE 'C'
    TEMPLATE template0;

-- Asignar privilegios
GRANT CONNECT ON DATABASE sigac_dev TO sigac_dev;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO sigac_dev;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO sigac_dev;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE ON SCHEMAS TO sigac_dev;

-- Conectarse a la BD y crear extensiones necesarias (si es necesario)
\c sigac_dev

-- UUID extension (opcional, para futuros IDs UUID)
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================================
-- FIN DEL SETUP
-- ================================================================
-- 
-- Próximos pasos:
-- 1. Configurar user-secrets: .\scripts\setup-dev-environment.ps1
-- 2. Aplicar migraciones: dotnet ef database update
-- 3. Ejecutar: dotnet run
