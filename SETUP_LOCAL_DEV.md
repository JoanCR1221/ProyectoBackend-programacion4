# SIGAC Backend - Configuración para Desarrollo Local

## 🔧 Requisitos Previos

- **.NET 8 SDK** - [Descargar](https://dotnet.microsoft.com/download)
- **PostgreSQL 14+** - [Descargar](https://www.postgresql.org/download/)
- **Git** - [Descargar](https://git-scm.com/download)

## ⚡ Configuración Rápida (Recomendado)

### 1. Clonar el repositorio

```bash
git clone https://github.com/JoanCR1221/ProyectoBackend-programacion4.git
cd ProyectoBackend-programacion4
git checkout feature/acceso-roles
```

### 2. Ejecutar script de configuración

**Windows (PowerShell):**
```powershell
cd C:\Users\[tu-usuario]\source\repos\ProyectoBackend-programacion4
.\scripts\setup-dev-environment.ps1
```

**macOS/Linux (Bash):**
```bash
chmod +x scripts/setup-dev-environment.ps1
pwsh scripts/setup-dev-environment.ps1
```

Este script:
- Inicializa los user-secrets de .NET (local y nunca en git)
- Configura la connection string a PostgreSQL
- Configura las claves JWT
- Configura credenciales del superusuario

### 3. Crear la base de datos

```bash
# Conectarse a PostgreSQL como admin
psql -U postgres

# En psql:
CREATE DATABASE sigac_dev;
\q
```

### 4. Aplicar migraciones

```bash
cd HackerRank1
dotnet ef database update
cd ..
```

### 5. Ejecutar la aplicación

```bash
cd HackerRank1
dotnet run
```

La API estará disponible en: `https://localhost:7042` (o el puerto que muestre)

---

## 🔐 Gestión de Secretos (Importante)

### ¿Por qué usar user-secrets?

Los `user-secrets` de .NET almacenan credenciales **localmente en tu máquina**, NO en git. Esto es la forma segura de manejar secrets en desarrollo.

**Nunca** hagas commit de:
- `appsettings.Development.json` con valores reales
- Contraseñas o connection strings en código

### Ver los secrets configurados

```bash
cd HackerRank1
dotnet user-secrets list
```

### Modificar un secret

```bash
cd HackerRank1
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=...;..."
```

### Limpiar todos los secrets (si algo sale mal)

```bash
cd HackerRank1
dotnet user-secrets clear
```

---

## 📊 Estructura de Base de Datos

Las migraciones crean automáticamente las tablas:

- `usuario` - Usuarios del sistema
- `rol` - Roles disponibles
- `permiso` - Permisos atomicos
- `rol_permiso` - Relación N:N entre roles y permisos

### Datos de seed

Se crean automáticamente:
- **3 Roles base**: usuario, administrador, superusuario
- **13 Permisos base**: donaciones.*, beneficiarios.*, inventario.*, gastos.*, usuarios.*, roles.*
- **1 Superusuario inicial**: email `super@sigac.cr` (contraseña configurable)

---

## 🔐 Credenciales por Defecto (Desarrollo)

⚠️ **SOLO PARA DESARROLLO LOCAL - CAMBIAR EN PRODUCCIÓN**

```
Email: super@sigac.cr
Password: SuperusuarioSigac2024!
```

Para cambiar en desarrollo:
```bash
cd HackerRank1
dotnet user-secrets set "SuperusuarioSeed:Email" "tu@email.com"
dotnet user-secrets set "SuperusuarioSeed:Password" "tu-contraseña-segura"
```

---

## 🧪 Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Autenticarse
- `POST /api/auth/register` - Registrarse
- `GET /api/auth/me` - Datos del usuario autenticado

### Gestión de Permisos
- `GET /api/permisos` - Listar permisos
- `GET /api/permisos/{id}` - Obtener permiso

### Gestión de Roles
- `GET /api/roles` - Listar roles
- `GET /api/roles/{id}` - Obtener rol
- `POST /api/roles` - Crear rol
- `PUT /api/roles/{id}` - Actualizar rol
- `DELETE /api/roles/{id}` - Eliminar rol

### Gestión de Usuarios
- `GET /api/usuarios` - Listar usuarios
- `GET /api/usuarios/{id}` - Obtener usuario
- `PATCH /api/usuarios/{id}/estado` - Cambiar estado (activo/inactivo)
- `PUT /api/usuarios/{id}/rol` - Asignar rol

---

## 🐛 Troubleshooting

### Error: "Could not connect to localhost:5432"
- Verificar que PostgreSQL está corriendo
- En Windows: Services → PostgreSQL → iniciar
- En macOS: `brew services start postgresql`

### Error: "Database 'sigac_dev' does not exist"
```bash
psql -U postgres
CREATE DATABASE sigac_dev;
\q
```

### Error: "ConnectionStrings:Supabase not found"
```bash
cd HackerRank1
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=localhost;Port=5432;Database=sigac_dev;Username=postgres;Password=postgres;SSL Mode=Disable"
```

### Error en migraciones
```bash
# Limpiar y reiniciar
cd HackerRank1
dotnet ef database drop --force
dotnet ef database update
```

---

## 📚 Documentación Adicional

- [EF Core Migraciones](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets/)
- [JWT Authentication en ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data)

---

## 👥 Soporte

Si tienes problemas:
1. Verifica los logs: `dotnet run` mostrará errores detallados
2. Revisa el .gitignore para asegurar que secrets no se comitearon
3. Pregunta en el PR o crea un issue

---

**¡Buena suerte! 🚀**
