# Capa EF Core — DESACTIVADA (fase sin base de datos)

Durante la fase de desarrollo, el backend corre **sin base de datos** para que cualquier
integrante levante la API y Swagger sin necesitar PostgreSQL/Supabase. La persistencia real
con Entity Framework Core quedó **preservada aquí**, no eliminada.

## Qué hay en esta carpeta
- `Data/SigacDbContext.cs` — DbContext con el mapeo Fluent API (snake_case, índices únicos)
  y el seed de catálogo (`HasData`).
- `Migrations/` — migración `Inicial` (esquema + seed de roles/permisos).
- `Services/` — implementaciones de los servicios **basadas en EF** (versión original).

Estos archivos están **excluidos de la compilación** mediante el `csproj`:

```xml
<ItemGroup>
  <Compile Remove="_EfDisabled\**\*.cs" />
</ItemGroup>
```

## Reemplazo temporal en uso
- `Data/InMemoryStore.cs` — almacén en memoria (singleton) con el mismo grafo de objetos.
- `Services/*.cs` — implementaciones en memoria de las **mismas interfaces**.
- `Startup.cs` registra `InMemoryStore` en vez de `AddDbContext`.

## Cómo restaurar EF (fase de integración)
1. **Paquetes** — volver a agregar en `HackerRank1.csproj`:
   ```xml
   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
   <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
   ```
2. **Quitar la exclusión** `<Compile Remove="_EfDisabled\**\*.cs" />` y mover de vuelta:
   - `_EfDisabled/Data/SigacDbContext.cs` → `Data/`
   - `_EfDisabled/Migrations/*` → `Migrations/`
   - `_EfDisabled/Services/*.cs` → `Services/` (sobrescribiendo las versiones en memoria).
3. **Eliminar** `Data/InMemoryStore.cs`.
4. **Startup.cs** — reemplazar `services.AddSingleton<InMemoryStore>()` por:
   ```csharp
   var connectionString = Configuration.GetConnectionString("Supabase")
       ?? throw new InvalidOperationException("Connection string 'Supabase' not found");
   services.AddDbContext<SigacDbContext>(o => o.UseNpgsql(connectionString));
   ```
   (y volver a `using Microsoft.EntityFrameworkCore;`).
5. **Program.cs** — restaurar `Database.Migrate()` + el seed del superusuario (ver historial git
   del commit anterior a la fase sin BD).
6. **appsettings** — restaurar `ConnectionStrings:Supabase`.

> Las interfaces (`IAuthenticationService`, `IPermisoService`, `IRolService`, `IUsuarioService`),
> los controllers, DTOs, filtros, JWT y Swagger **no cambian** entre ambas fases.
