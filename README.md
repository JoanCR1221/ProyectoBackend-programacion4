# SIGAC - Backend: Acceso y Roles

## Descripción

Backend de gestión de **acceso, roles y permisos** para el sistema SIGAC, construido con **ASP.NET Core 8** y **PostgreSQL**.

### Características principales

- ✅ **Autenticación JWT** con email/contraseña
- ✅ **Autorización basada en permisos** (RBAC + ABAC)
- ✅ **Gestión de roles y permisos** (create-first con EF Core)
- ✅ **Anti-escalamiento** de privilegios
- ✅ **CORS** configurado para desarrollo y producción
- ✅ **Swagger** documentado
- ✅ **Seed automático** del superusuario al iniciar

---

## Requisitos

- **.NET 8 SDK** ([descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- **PostgreSQL 12+** ([descargar](https://www.postgresql.org/download/))
- **Git**

---

## Configuración local

### 1. Clonar el repositorio

```bash
git clone https://github.com/JoanCR1221/ProyectoBackend-programacion4.git
cd ProyectoBackend-programacion4
git checkout feature/acceso-roles
```

### 2. Crear base de datos PostgreSQL

Abre **pgAdmin** o usa `psql` para crear la BD:

```sql
CREATE DATABASE sigac_dev OWNER postgres;
CREATE DATABASE sigac_prod OWNER postgres;
```

O por terminal (psql):

```bash
psql -U postgres -c "CREATE DATABASE sigac_dev OWNER postgres;"
psql -U postgres -c "CREATE DATABASE sigac_prod OWNER postgres;"
```

### 3. Verificar/Actualizar connection strings

Verifica que en `appsettings.Development.json` los datos coincidan con tu instalación de PostgreSQL:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=localhost;Database=sigac_dev;Username=postgres;Password=postgres;SSL Mode=Disable"
  }
}
```

**Cambia:**
- `Host`: Si PostgreSQL está en otro servidor
- `Database`: Nombre de tu BD (sigac_dev para desarrollo)
- `Username`: Usuario de PostgreSQL (por defecto `postgres`)
- `Password`: Tu contraseña de PostgreSQL

### 4. Restaurar paquetes NuGet e instalar EF Core CLI

```bash
cd HackerRank1
dotnet restore
dotnet tool install --global dotnet-ef
```

### 5. Aplicar migraciones (crear tablas)

```bash
dotnet ef database update
```

Esto creará las tablas en `sigac_dev` y hará seed de:
- **Permisos** (13 permisos predefinidos)
- **Roles** (3 roles: usuario, administrador, superusuario)
- **Relaciones Rol↔Permiso**
- **Superusuario inicial** (email: `super@sigac.cr`, password: `SuperusuarioSigac2024!`)

### 6. Ejecutar el proyecto

```bash
dotnet run
```

La API estará disponible en: **http://localhost:5000** (Development) o **https://localhost:5001** (HTTPS)

Swagger disponible en: **http://localhost:5000/swagger/ui**

---

## Estructura de Entidades

### Permiso
```csharp
- Id (int, PK)
- Clave (string, UNIQUE) - "donaciones.crear", "usuarios.gestionar", etc.
- Descripcion (string)
```

### Rol
```csharp
- Id (int, PK)
- Nombre (string, UNIQUE) - "usuario", "administrador", "superusuario"
- Descripcion (string)
- CreadoEn (datetime)
- Relación N:N con Permiso via RolPermiso
```

### Usuario
```csharp
- Id (int, PK)
- Nombre (string)
- Email (string, UNIQUE)
- PasswordHash (string) - Hasheado con BCrypt
- RolId (int, FK) - Relación N:1 con Rol
- Activo (bool)
- CreadoEn (datetime)
```

### RolPermiso (Entidad Puente)
```csharp
- RolId (int, PK, FK)
- PermisoId (int, PK, FK)
```

---

## Roles y Permisos Predefinidos

### Rol "usuario"
Permisos:
- `donaciones.crear`
- `donaciones.ver`

### Rol "administrador"
Permisos operativos:
- `donaciones.crear`, `donaciones.ver`, `donaciones.editar`, `donaciones.eliminar`
- `beneficiarios.gestionar`
- `inventario.gestionar`
- `gastos.gestionar`
- `usuarios.ver`, `usuarios.gestionar`
- `roles.ver`

**NO tiene:** `roles.gestionar`, `roles.asignar`, `usuarios.crear_admin`

### Rol "superusuario"
Tiene **todos los permisos** (13 total)

---

## Endpoints principales

### Autenticación (PUBLIC)

#### POST `/api/auth/login`
```json
{
  "email": "super@sigac.cr",
  "password": "SuperusuarioSigac2024!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "nombre": "Superusuario SIGAC",
    "email": "super@sigac.cr",
    "rol": "superusuario",
    "activo": true,
    "creadoEn": "2025-06-18T03:44:41Z"
  },
  "permisos": ["roles.ver", "roles.gestionar", "usuarios.ver", "usuarios.gestionar", ...]
}
```

#### POST `/api/auth/register`
```json
{
  "nombre": "Juan Pérez",
  "email": "juan@example.com",
  "password": "MiContraseña123!"
}
```

Siempre asigna rol **"usuario"**. Response igual al login.

#### GET `/api/auth/me` (AUTENTICADO)
Devuelve el usuario actual y sus permisos.

### Permisos (AUTENTICADO)

#### GET `/api/permisos` [TienePermiso("roles.ver")]
Lista todos los permisos.

#### GET `/api/permisos/{id}` [TienePermiso("roles.ver")]
Obtiene un permiso específico.

### Roles (AUTENTICADO)

#### GET `/api/roles` [TienePermiso("roles.ver")]
Lista roles con sus permisos.

#### GET `/api/roles/{id}` [TienePermiso("roles.ver")]

#### POST `/api/roles` [TienePermiso("roles.gestionar")]
```json
{
  "nombre": "editor",
  "descripcion": "Rol de edición",
  "permisoIds": [1, 2, 3]
}
```

#### PUT `/api/roles/{id}` [TienePermiso("roles.gestionar")]

#### DELETE `/api/roles/{id}` [TienePermiso("roles.gestionar")]

### Usuarios (AUTENTICADO)

#### GET `/api/usuarios` [TienePermiso("usuarios.ver")]

#### GET `/api/usuarios/{id}` [TienePermiso("usuarios.ver")]

#### PATCH `/api/usuarios/{id}/estado` [TienePermiso("usuarios.gestionar")]
```json
{
  "activo": false
}
```

#### PUT `/api/usuarios/{id}/rol` [TienePermiso("roles.asignar")]
```json
{
  "rolId": 2
}
```

---

## Seguridad

### Anti-escalamiento
- Al asignar roles/permisos, el actor **NO puede otorgar permisos que no posea**.
- Los permisos `roles.asignar`, `roles.gestionar` y `usuarios.crear_admin` son **exclusivos del superusuario**.

### Validación
- Email único en registro
- Contraseñas hasheadas con BCrypt
- JWT expira en 1 hora
- CORS restringido a orígenes configurados

### Atributos de Autorización
```csharp
[TienePermiso("permiso.requerido")]
```
Verifica si el usuario tiene el permiso. Si no, retorna **403 Forbidden**.

---

## Migraciones y Seed

### Crear nueva migración
```bash
dotnet ef migrations add NombreMigracion
```

### Aplicar migraciones
```bash
dotnet ef database update
```

### Revertir última migración
```bash
dotnet ef migrations remove
```

### Ver migraciones aplicadas
```bash
dotnet ef migrations list
```

### Seed automático
El superusuario se crea **automáticamente** al iniciar si:
1. La BD está creada (migración aplicada)
2. No existe un usuario con email `super@sigac.cr`

---

## Troubleshooting

### Error: "Host localhost not responding"
- Verifica que PostgreSQL está corriendo: `systemctl status postgresql` (Linux) o Services (Windows)
- Por defecto escucha en puerto 5432

### Error: "Database does not exist"
- Ejecuta: `psql -U postgres -c "CREATE DATABASE sigac_dev OWNER postgres;"`

### Error: "Cannot log in to PostgreSQL"
- Verifica usuario/contraseña en `appsettings.Development.json`
- Por defecto: usuario `postgres`, sin contraseña (si instalaste PostgreSQL localmente)

### JWT Token inválido
- Verifica que la `SecretKey` en `appsettings.json` tiene mínimo 32 caracteres
- Cambia antes de ir a producción

---

## Variables de entorno (Producción)

Para Supabase en producción, usa **user secrets** o variables de entorno:

```bash
# Guardar connection string en user-secrets
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.supabase.co;..."
dotnet user-secrets set "JwtSettings:SecretKey" "tu-clave-super-secreta"
```

---

## Contribuciones

1. Crea una rama: `git checkout -b feature/nueva-funcionalidad`
2. Haz cambios y commits
3. Push a la rama: `git push origin feature/nueva-funcionalidad`
4. Abre un Pull Request

---

## Licencia

MIT

---

**Última actualización:** Junio 2025
**Rama:** `feature/acceso-roles`
**Versión:** 1.0.0
