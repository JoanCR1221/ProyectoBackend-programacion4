# SIGAC Backend - Arquitectura de Autorización y Permisos

## 📋 Índice
1. [Modelo de Permisos](#modelo-de-permisos)
2. [Estructura de Roles](#estructura-de-roles)
3. [Flujo de Autenticación](#flujo-de-autenticación)
4. [Flujo de Autorización](#flujo-de-autorización)
5. [Anti-Escalamiento](#anti-escalamiento)
6. [Endpoints Protegidos](#endpoints-protegidos)

---

## Modelo de Permisos

### Principio: Permisos Atómicos

Cada permiso representa una **acción específica**, no un recurso completo:

```
donaciones.crear      → Crear nuevas donaciones
donaciones.ver        → Ver donaciones
donaciones.editar     → Editar donaciones existentes
donaciones.eliminar   → Eliminar donaciones

beneficiarios.gestionar    → Gestión completa de beneficiarios
inventario.gestionar       → Gestión completa de inventario
gastos.gestionar           → Gestión completa de gastos

usuarios.ver               → Ver lista de usuarios
usuarios.gestionar         → Activar/desactivar usuarios, cambiar estado
usuarios.crear_admin       → Crear nuevos administradores (EXCLUSIVO SUPERUSUARIO)

roles.ver                  → Ver roles disponibles
roles.gestionar            → Crear/editar/eliminar roles (EXCLUSIVO SUPERUSUARIO)
roles.asignar              → Asignar roles a usuarios (EXCLUSIVO SUPERUSUARIO)
```

### Total de Permisos

**13 permisos atómicos** definidos en el seed:

| ID | Clave | Descripción |
|---|---|---|
| 1 | `donaciones.crear` | Crear donaciones |
| 2 | `donaciones.ver` | Ver donaciones |
| 3 | `donaciones.editar` | Editar donaciones |
| 4 | `donaciones.eliminar` | Eliminar donaciones |
| 5 | `beneficiarios.gestionar` | Gestionar beneficiarios |
| 6 | `inventario.gestionar` | Gestionar inventario |
| 7 | `gastos.gestionar` | Gestionar gastos |
| 8 | `usuarios.ver` | Ver usuarios |
| 9 | `usuarios.gestionar` | Gestionar usuarios |
| 10 | `usuarios.crear_admin` | Crear administradores |
| 11 | `roles.ver` | Ver roles |
| 12 | `roles.gestionar` | Gestionar roles |
| 13 | `roles.asignar` | Asignar roles a usuarios |

---

## Estructura de Roles

### 3 Roles Predefinidos

#### 1. **USUARIO** (Rol Básico)
```
Permisos:
  ✓ donaciones.crear
  ✓ donaciones.ver

Características:
  - Puede registrarse automáticamente
  - Acceso limitado a donaciones
  - No puede crear otros usuarios
  - No puede acceder a administración
```

#### 2. **ADMINISTRADOR** (Operativo)
```
Permisos:
  ✓ donaciones.* (crear, ver, editar, eliminar)
  ✓ beneficiarios.gestionar
  ✓ inventario.gestionar
  ✓ gastos.gestionar
  ✓ usuarios.ver
  ✓ usuarios.gestionar
  ✓ roles.ver

Características:
  - Gestión operativa completa
  - Puede activar/desactivar usuarios
  - PUEDE VER pero NO PUEDE ASIGNAR roles
  - NUNCA puede obtener: usuarios.crear_admin, roles.gestionar, roles.asignar
```

#### 3. **SUPERUSUARIO** (Máxima Autoridad)
```
Permisos:
  ✓ Todos los 13 permisos

Características:
  - Acceso completo al sistema
  - Puede crear otros administradores
  - Puede crear/editar/eliminar roles
  - Puede asignar roles a cualquier usuario
  - Rol de bootstrap (se crea al iniciar la app)
```

### Relación N:N: Rol ↔ Permiso

```
Tabla: rol_permiso
├── rol_id (FK → rol.id)
├── permiso_id (FK → permiso.id)
└── PK: (rol_id, permiso_id)

Ejemplo:
  rol_id=2 (Administrador), permiso_id=1 (donaciones.crear)
  rol_id=2 (Administrador), permiso_id=2 (donaciones.ver)
  ...
```

---

## Flujo de Autenticación

### 1. **Registro** (`POST /api/auth/register`)

```
Cliente
  ↓ {nombre, email, password}
  ↓
AuthController.Registrar()
  ↓
AuthenticationService.RegistrarAsync()
  ├─ Validar email único
  ├─ Validar formato de email
  ├─ Obtener rol "usuario" (siempre asignado)
  ├─ Hashear contraseña con BCrypt.HashPassword()
  └─ Crear Usuario en BD
  ↓
PermisoService.ObtenerPermisosDeUsuarioAsync()
  └─ Cargar permisos del rol "usuario"
  ↓
TokenGenerator.GenerateToken()
  ├─ Crear claims:
  │  ├─ NameIdentifier: usuario.Id
  │  ├─ Email: usuario.Email
  │  ├─ Rol: "usuario"
  │  └─ Permiso: ["donaciones.crear", "donaciones.ver"]
  └─ Firmar con RSA (JWT)
  ↓
Response: {token, usuario, permisos}
```

### 2. **Login** (`POST /api/auth/login`)

```
Cliente
  ↓ {email, password}
  ↓
AuthController.Login()
  ↓
AuthenticationService.AutenticarAsync()
  ├─ Buscar usuario por email
  ├─ Verificar usuario.Activo == true
  ├─ Verificar contraseña con BCrypt.Verify()
  └─ Retornar (usuario, permisos)
  ↓
PermisoService.ObtenerPermisosDeUsuarioAsync()
  └─ SELECT permiso.clave
     FROM permiso
     JOIN rol_permiso ON permiso.id = rol_permiso.permiso_id
     WHERE rol_permiso.rol_id = usuario.rol_id
  ↓
TokenGenerator.GenerateToken()
  └─ Crear token con claims de permisos
  ↓
Response: {token, usuario, permisos}
```

### 3. **Token JWT Structure**

```
Header
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload
{
  "nameid": "1",              // Usuario ID
  "email": "user@sigac.cr",
  "rol": "administrador",
  "permiso": [                // Array de permisos
    "donaciones.crear",
    "donaciones.ver",
    "beneficiarios.gestionar",
    ...
  ],
  "iss": "SIGAC",
  "aud": "sigac-frontend",
  "iat": 1234567890,
  "exp": 1234571490          // Expira en 1 hora
}

Signature: HS256(header + payload, secretKey)
```

---

## Flujo de Autorización

### Atributo `[TienePermiso("clave")]`

El atributo implementa `IAuthorizationFilter`:

```csharp
[HttpGet]
[TienePermiso("roles.ver")]
public async Task<IActionResult> ObtenerTodos()
{
    // Solo llega aquí si el usuario tiene permiso
}
```

### Proceso de Validación

```
Request con Token JWT
  ↓
[Authorize] valida token (en Startup.cs)
  ├─ Verifica firma del token
  ├─ Verifica issuer y audience
  ├─ Verifica no esté expirado
  └─ Crea ClaimsPrincipal si es válido
  ↓
[TienePermiso("roles.ver")] valida permiso
  ├─ Lee todos los claims "permiso" del usuario
  ├─ Verifica si alguno == "roles.ver"
  ├─ Si SÍ: continúa al controller
  └─ Si NO: retorna 403 Forbidden
  ↓
Ejecuta la acción del controller
```

### Código del Filtro

```csharp
public class TienePermisoAttribute : IAuthorizationFilter
{
    private readonly string _permisoRequerido;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;

        // ¿Está autenticado?
        if (!usuario.Identity?.IsAuthenticated)
            return 401;

        // ¿Tiene el permiso?
        var tienePermiso = usuario.FindAll("permiso")
            .Any(c => c.Value == _permisoRequerido);

        if (!tienePermiso)
            return 403;
    }
}
```

---

## Anti-Escalamiento

### Problema

Un administrador podría intentar otorgarse permisos que no tiene, o asignar permisos exclusivos del superusuario.

### Solución: Validación en Servicios

#### Caso 1: Crear/Actualizar Rol

```csharp
// En RolService.CrearAsync()
public async Task<RolResponse> CrearAsync(
    string nombre, 
    string descripcion, 
    List<int> permisoIds)  // ← Los permisos a asignar
{
    // NO hacemos validación aquí
    // La validación debe venir del CONTROLLER
}
```

#### Caso 2: Validación en RolesController

```csharp
[HttpPost]
[TienePermiso("roles.gestionar")]  // ← Ya tiene permisos base
public async Task<IActionResult> Crear([FromBody] CrearRolRequest request)
{
    // ADICIONAL: verificar que no intenta asignar permisos que él NO tiene
    var permisosDelUsuarioActual = await ObtenerPermisosDelUsuarioActual();

    // Permisos exclusivos del superusuario
    var permisosExclusivos = new[] { 
        "roles.asignar", 
        "roles.gestionar", 
        "usuarios.crear_admin" 
    };

    // Validación: si intenta asignar permisos exclusivos y no los tiene
    foreach (var permisoExclusivo in permisosExclusivos)
    {
        if (request.PermisoIds.Contains(permisoExclusivo) &&
            !permisosDelUsuarioActual.Contains(permisoExclusivo))
        {
            return Forbid();  // 403
        }
    }
}
```

### Ejemplo: Intento Fallido

```
Administrador intenta:
  POST /api/roles
  {
    "nombre": "MalRol",
    "descripcion": "Rol malicioso",
    "permisoIds": [12, 13]  // roles.gestionar, roles.asignar
  }

Resultado:
  1. ✓ Pasa [TienePermiso("roles.gestionar")]? NO → 403
  2. Si lo pasara, controller valida:
     - ¿"roles.gestionar" en permisosAdministrador? NO
     - return 403 Forbidden
```

### Ejemplo: Intento Exitoso

```
Superusuario intenta:
  POST /api/roles
  {
    "nombre": "NuevoRol",
    "descripcion": "Para nuevos usuarios",
    "permisoIds": [1, 2, 5]  // donaciones.*, beneficiarios.*
  }

Resultado:
  1. ✓ Pasa [TienePermiso("roles.gestionar")]? SÍ
  2. Controller valida:
     - Todos los permisos en permisosSuperusuario? SÍ
     - Crea el rol exitosamente
```

---

## Endpoints Protegidos

### Autenticación (Sin protección)

| Método | Endpoint | Protegido | Permisos |
|--------|----------|-----------|----------|
| POST | `/api/auth/login` | ✗ | - |
| POST | `/api/auth/register` | ✗ | - |
| GET | `/api/auth/me` | ✓ Autenticado | - |

### Permisos (Solo Lectura)

| Método | Endpoint | Protegido | Permiso Requerido |
|--------|----------|-----------|-------------------|
| GET | `/api/permisos` | ✓ | `roles.ver` |
| GET | `/api/permisos/{id}` | ✓ | `roles.ver` |

### Roles (Completo CRUD)

| Método | Endpoint | Protegido | Permiso Requerido |
|--------|----------|-----------|-------------------|
| GET | `/api/roles` | ✓ | `roles.ver` |
| GET | `/api/roles/{id}` | ✓ | `roles.ver` |
| POST | `/api/roles` | ✓ | `roles.gestionar` |
| PUT | `/api/roles/{id}` | ✓ | `roles.gestionar` |
| DELETE | `/api/roles/{id}` | ✓ | `roles.gestionar` |

### Usuarios (Gestión)

| Método | Endpoint | Protegido | Permiso Requerido |
|--------|----------|-----------|-------------------|
| GET | `/api/usuarios` | ✓ | `usuarios.ver` |
| GET | `/api/usuarios/{id}` | ✓ | `usuarios.ver` |
| PATCH | `/api/usuarios/{id}/estado` | ✓ | `usuarios.gestionar` |
| PUT | `/api/usuarios/{id}/rol` | ✓ | `roles.asignar` |

---

## Base de Datos - Schema

### Tablas

```
┌─────────────────────────────────────────┐
│             usuario                     │
├─────────────────────────────────────────┤
│ id (PK)                    INTEGER      │
│ nombre                     VARCHAR(150) │
│ email (UNIQUE)             VARCHAR(150) │
│ password_hash              TEXT         │
│ rol_id (FK)                INTEGER      │
│ activo                     BOOLEAN      │
│ creado_en                  TIMESTAMP    │
└─────────────────────────────────────────┘
            ↓ N:1
            ↓
┌─────────────────────────────────────────┐
│               rol                       │
├─────────────────────────────────────────┤
│ id (PK)                    INTEGER      │
│ nombre (UNIQUE)            VARCHAR(100) │
│ descripcion                VARCHAR(255) │
│ creado_en                  TIMESTAMP    │
└─────────────────────────────────────────┘
            ↓ N:N (vía rol_permiso)
            ↓
┌─────────────────────────────────────────┐
│             permiso                     │
├─────────────────────────────────────────┤
│ id (PK)                    INTEGER      │
│ clave (UNIQUE)             VARCHAR(100) │
│ descripcion                VARCHAR(255) │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│            rol_permiso                  │
├─────────────────────────────────────────┤
│ rol_id (FK, PK)            INTEGER      │
│ permiso_id (FK, PK)        INTEGER      │
└─────────────────────────────────────────┘
```

---

## Configuración en Startup.cs

```csharp
// Autenticación JWT
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ValidateIssuer = true,
            ValidIssuer = "SIGAC",
            ValidateAudience = true,
            ValidAudience = "sigac-frontend",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Autorización (básica, solo autenticado)
services.AddAuthorization();
```

El filtro `[TienePermiso]` añade validación adicional por permiso específico.

---

## Debugging

### Ver Claims del Token

```csharp
var claims = User.Claims;
foreach (var claim in claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}

// Salida:
// http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: 1
// http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress: user@sigac.cr
// rol: administrador
// permiso: donaciones.crear
// permiso: donaciones.ver
// ...
```

### Obtener Usuario del Token

```csharp
var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var rol = User.FindFirst("rol")?.Value;
var permisos = User.FindAll("permiso").Select(c => c.Value).ToList();
```

---

## Consideraciones de Seguridad

1. **Contraseñas**: Siempre hasheadas con BCrypt (nunca se guardan en texto plano)
2. **Tokens**: Firmados con HMAC-SHA256, validan issuer, audience y tiempo
3. **Secretos**: Almacenados en user-secrets (desarrollo) o Azure Key Vault (producción)
4. **Anti-escalamiento**: Validación en múltiples capas (filtro + servicio + lógica)
5. **Permisos exclusivos**: `usuarios.crear_admin`, `roles.gestionar`, `roles.asignar` solo para superusuario
6. **Auditoría**: `creado_en` timestamp en usuarios y roles para trazabilidad

---

## Próximos Pasos (Roadmap)

- [ ] Agregar logging de acciones sensibles (quién cambió qué rol, cuándo)
- [ ] Implementar rate limiting en login
- [ ] Agregar 2FA (autenticación de dos factores)
- [ ] Implementar refresh tokens (tokens con expiración larga + refresh cortos)
- [ ] Roles dinámicos (crear roles custom sin redeploy)
- [ ] Permisos granulares por recurso (e.g., "donaciones.ver:propias")
