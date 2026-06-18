# Guía Rápida de Inicio - SIGAC Backend

## ⚡ Inicio Rápido (5 minutos)

### 1️⃣ Requisitos previos
- **.NET 8 SDK** instalado
- **PostgreSQL** corriendo localmente (puerto 5432)
- **Git**

### 2️⃣ Clonar y configurar

```bash
# Clonar repo
git clone https://github.com/JoanCR1221/ProyectoBackend-programacion4.git
cd ProyectoBackend-programacion4
git checkout feature/acceso-roles

# Crear BD (en psql o pgAdmin)
# CREATE DATABASE sigac_dev OWNER postgres;
```

### 3️⃣ Restaurar y migrar

```bash
cd HackerRank1
dotnet restore
dotnet tool install --global dotnet-ef  # Si no lo tienes
dotnet ef database update
```

### 4️⃣ Ejecutar

```bash
dotnet run
```

✅ **API corriendo en:** http://localhost:5000

---

## 🔑 Credenciales de prueba

### Superusuario (todos los permisos)
```
Email:    super@sigac.cr
Password: SuperusuarioSigac2024!
```

### Crear usuarios de prueba
```
POST /api/auth/register
{
  "nombre": "Juan Pérez",
  "email": "juan@test.com",
  "password": "Test123456!"
}
```

---

## 🧪 Primeros pasos

### 1. Login y obtén token
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "super@sigac.cr",
    "password": "SuperusuarioSigac2024!"
  }'
```

Copiar el `token` de la respuesta.

### 2. Listar permisos (requiere token)
```bash
curl -X GET http://localhost:5000/api/permisos \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Listar roles
```bash
curl -X GET http://localhost:5000/api/roles \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 📚 Documentación interactiva

- **Swagger UI:** http://localhost:5000/swagger/ui
- **Postman Collection:** Importar `SIGAC-API.postman_collection.json`
- **README completo:** `README.md`

---

## 🚨 Troubleshooting

### "Cannot connect to PostgreSQL"
```bash
# Verificar que PostgreSQL está corriendo
psql -U postgres -c "SELECT version();"

# Crear BD si no existe
psql -U postgres -c "CREATE DATABASE sigac_dev OWNER postgres;"
```

### "Database does not exist"
```bash
dotnet ef database update
```

### "Port 5000 already in use"
```bash
# Cambiar puerto en launchSettings.json
# O matar proceso en puerto 5000
netstat -ano | findstr :5000  # Windows
lsof -i :5000                  # Mac/Linux
```

### JWT errors
- Verificar que `SecretKey` en `appsettings.json` tiene 32+ caracteres
- Verificar que el token no ha expirado (1 hora)

---

## 📦 Stack tecnológico

- **ASP.NET Core 8**
- **Entity Framework Core 9** (Code-First)
- **PostgreSQL**
- **JWT** (autenticación)
- **BCrypt** (hash de contraseñas)
- **Swagger** (documentación)
- **CORS** (control de orígenes)

---

## 📝 Próximos pasos

1. Crear más roles personalizados
2. Asignar usuarios a diferentes roles
3. Probar endpoints con permisos faltantes (403 Forbidden)
4. Integrar con frontend en Vue.js / React

---

**¿Necesitas ayuda?** Revisa el README.md completo o abre un issue en GitHub.
