# 📋 Configuración de Desarrollo - SIGAC Backend

## 🚀 Setup inicial para nuevos desarrolladores

Este proyecto usa **User Secrets** de .NET para almacenar credenciales locales de forma segura, sin commitearlas al repositorio.

### Requisitos previos

- .NET 8 SDK instalado
- PostgreSQL 13+ (local o Supabase)
- Git
- Visual Studio 2022+ o VS Code

### 📝 Pasos de configuración

#### 1. Clonar el repositorio

```bash
git clone https://github.com/JoanCR1221/ProyectoBackend-programacion4.git
cd ProyectoBackend-programacion4
git checkout feature/acceso-roles
```

#### 2. Configurar User Secrets (automático)

**Opción A: Script automático (Recomendado)**

```powershell
cd HackerRank1
.\.\scripts\setup-secrets.ps1
```

**Opción B: Manual**

```bash
cd HackerRank1
dotnet user-secrets init

# Connection string
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=localhost;Database=sigac_dev;Username=postgres;Password=TuPassword;SSL Mode=Disable"

# JWT Secret (mínimo 32 caracteres)
dotnet user-secrets set "JwtSettings:SecretKey" "MiSecretoParaDesarrolloLocal123456789"

# Credenciales del Superusuario
dotnet user-secrets set "SuperusuarioSeed:Email" "super@sigac.cr"
dotnet user-secrets set "SuperusuarioSeed:Password" "TuPasswordSegura123!"
```

#### 3. Preparar base de datos

**Si usas PostgreSQL local:**

```bash
# Crear base de datos
createdb -U postgres sigac_dev

# O usando psql
psql -U postgres
CREATE DATABASE sigac_dev;
\q
```

**Si usas Supabase:**

1. Ir a [supabase.com](https://supabase.com)
2. Crear proyecto
3. Copiar la connection string desde Settings → Database
4. Configurarla en user-secrets como arriba

#### 4. Aplicar migraciones

```bash
cd HackerRank1

# Restaurar paquetes
dotnet restore

# Aplicar migraciones
dotnet ef database update
```

Las migraciones se aplicarán automáticamente al iniciar la app, pero puedes hacerlo manualmente así.

#### 5. Iniciar la aplicación

```bash
dotnet run

# O en VS Code/Visual Studio: F5
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger` (en desarrollo)

---

## 🔐 Seguridad - User Secrets

### ¿Dónde se almacenan los secrets?

Los User Secrets se guardan **fuera del repositorio** en:

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<project-id>\secrets.json`
- **Linux/Mac**: `~/.microsoft/usersecrets/<project-id>/secrets.json`

### Variables de configuración

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings:Supabase` | Connection string a PostgreSQL | `Host=localhost;Database=sigac_dev;...` |
| `JwtSettings:SecretKey` | Secret para firmar tokens JWT | `MiSecretoMasSeguro123456789` |
| `SuperusuarioSeed:Email` | Email del superusuario inicial | `super@sigac.cr` |
| `SuperusuarioSeed:Password` | Contraseña del superusuario | `Password123!` |

### ⚠️ IMPORTANTE

**NUNCA:**
- ✗ Commitees `appsettings.Development.json` con valores reales
- ✗ Hardcodees secrets en el código
- ✗ Compartas tus `secrets.json` en el chat
- ✗ Dejes credenciales en comentarios

**SIEMPRE:**
- ✓ Usa `dotnet user-secrets` para desarrollo local
- ✓ Usa variables de entorno o Key Vault en producción
- ✓ Revisa el `.gitignore` antes de hacer commit

---

## 📚 Estructura del proyecto

```
HackerRank1/
├── Controllers/           # Endpoints (Auth, Permisos, Roles, Usuarios)
├── Data/                 # DbContext de EF Core
├── Entities/             # Modelos de base de datos
├── Services/             # Lógica de negocio
├── Filters/              # Atributos custom ([TienePermiso])
├── Migrations/           # Migraciones de EF Core
├── DTO/                  # Data Transfer Objects
├── Helpers/              # Utilidades (TokenGenerator)
├── appsettings.json      # Configuración (sin secrets)
├── appsettings.Development.example.json  # Plantilla para dev
└── Startup.cs            # Configuración de DI
```

---

## 🧪 Pruebas de API

### Colección Postman

Descarga `SIGAC-API.postman_collection.json` en Postman y podrás testear todos los endpoints.

### Credenciales de prueba

**Superusuario (todas las operaciones):**
- Email: `super@sigac.cr` (o la que configuraste)
- Password: La que configuraste en user-secrets

**Usuario normal:**
- Email: `usuario@example.com`
- Password: `Password123!`

### Login y obtener token

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "super@sigac.cr",
    "password": "TuPassword"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "nombre": "Superusuario SIGAC",
    "email": "super@sigac.cr",
    "rol": "superusuario",
    "activo": true,
    "creadoEn": "2024-06-18T03:44:41Z"
  },
  "permisos": [
    "donaciones.crear",
    "donaciones.ver",
    ...
  ]
}
```

---

## 🐛 Troubleshooting

### "Connection string 'Supabase' not found"

Asegúrate de haber configurado los user-secrets:

```bash
dotnet user-secrets list
```

Deberías ver la connection string. Si no, configúrala de nuevo:

```bash
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=..."
```

### "Rol 'superusuario' no encontrado"

Las migraciones no se aplicaron correctamente. Ejecuta:

```bash
dotnet ef database update
```

### Puerto 5000/5001 ya en uso

Cambia el puerto en `Properties/launchSettings.json`:

```json
"applicationUrl": "http://localhost:5555;https://localhost:5556"
```

### Database connection refused

Verifica que PostgreSQL esté corriendo:

```bash
# Windows
net start postgresql-x64-15

# Linux
sudo systemctl start postgresql

# macOS
brew services start postgresql
```

---

## 📝 Flujo de desarrollo

1. **Crear rama**: `git checkout -b feature/tu-feature`
2. **Hacer cambios**: Edita código según sea necesario
3. **Commitar**: `git add . && git commit -m "feat: descripción"`
4. **Push**: `git push origin feature/tu-feature`
5. **Pull Request**: Crea PR para merge a `feature/acceso-roles`

---

## 🔄 Migraciones de base de datos

### Crear nueva migración

```bash
dotnet ef migrations add NombreMigracion
```

### Ver migraciones pendientes

```bash
dotnet ef migrations list
```

### Aplicar migraciones

```bash
dotnet ef database update
```

### Deshacer última migración

```bash
dotnet ef migrations remove
```

---

## 📞 Contacto

Si tienes problemas con la configuración, contacta a:
- **Líder técnico**: Joan C. (joanc@email.com)
- **Issues**: GitHub Issues en el repositorio

---

**Última actualización**: Junio 2024
