using HackerRank1.Entities;

namespace HackerRank1.Data;

/// <summary>
/// Almacén EN MEMORIA que reemplaza temporalmente a SigacDbContext durante la fase
/// de desarrollo, para que el backend (y Swagger) arranque sin ninguna base de datos.
///
/// Mantiene el mismo grafo de objetos que tendría EF (Usuario N-1 Rol, Rol N-N Permiso
/// vía RolPermiso) con sus navegaciones cableadas, de modo que los servicios trabajan
/// igual que con el DbContext. Es un singleton: los datos viven mientras corra el proceso.
///
/// Para volver a EF en la fase de integración, ver HackerRank1/_EfDisabled/README.md.
/// </summary>
public class InMemoryStore
{
    private int _nextRolId;
    private int _nextUsuarioId;

    public List<Permiso> Permisos { get; } = new();
    public List<Rol> Roles { get; } = new();
    public List<RolPermiso> RolPermisos { get; } = new();
    public List<Usuario> Usuarios { get; } = new();

    public InMemoryStore(IConfiguration configuration)
    {
        SeedCatalogo();
        SeedSuperusuario(configuration);
    }

    public int NextRolId() => ++_nextRolId;
    public int NextUsuarioId() => ++_nextUsuarioId;

    /// <summary>Catálogo estático de permisos, roles y su relación (equivale al HasData del DbContext).</summary>
    private void SeedCatalogo()
    {
        // ---- Permisos ----
        var permisos = new[]
        {
            new Permiso { Id = 1, Clave = "donaciones.crear", Descripcion = "Crear donaciones" },
            new Permiso { Id = 2, Clave = "donaciones.ver", Descripcion = "Ver donaciones" },
            new Permiso { Id = 3, Clave = "donaciones.editar", Descripcion = "Editar donaciones" },
            new Permiso { Id = 4, Clave = "donaciones.eliminar", Descripcion = "Eliminar donaciones" },
            new Permiso { Id = 5, Clave = "beneficiarios.gestionar", Descripcion = "Gestionar beneficiarios" },
            new Permiso { Id = 6, Clave = "inventario.gestionar", Descripcion = "Gestionar inventario" },
            new Permiso { Id = 7, Clave = "gastos.gestionar", Descripcion = "Gestionar gastos" },
            new Permiso { Id = 8, Clave = "usuarios.ver", Descripcion = "Ver usuarios" },
            new Permiso { Id = 9, Clave = "usuarios.gestionar", Descripcion = "Gestionar usuarios" },
            new Permiso { Id = 10, Clave = "usuarios.crear_admin", Descripcion = "Crear administradores" },
            new Permiso { Id = 11, Clave = "roles.ver", Descripcion = "Ver roles" },
            new Permiso { Id = 12, Clave = "roles.gestionar", Descripcion = "Gestionar roles" },
            new Permiso { Id = 13, Clave = "roles.asignar", Descripcion = "Asignar roles a usuarios" }
        };
        Permisos.AddRange(permisos);

        // ---- Roles ----
        var fechaSeed = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rolUsuario = new Rol { Id = 1, Nombre = "usuario", Descripcion = "Rol básico de usuario", CreadoEn = fechaSeed };
        var rolAdmin = new Rol { Id = 2, Nombre = "administrador", Descripcion = "Rol de administrador operativo", CreadoEn = fechaSeed };
        var rolSuper = new Rol { Id = 3, Nombre = "superusuario", Descripcion = "Rol de superusuario con todos los permisos", CreadoEn = fechaSeed };
        Roles.AddRange(new[] { rolUsuario, rolAdmin, rolSuper });
        _nextRolId = 3;

        // ---- Relación rol -> permiso (mismas asignaciones que el seed de EF) ----
        var asignaciones = new (int RolId, int PermisoId)[]
        {
            (1, 1), (1, 2),
            (2, 1), (2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7), (2, 8), (2, 9), (2, 11),
            (3, 1), (3, 2), (3, 3), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10), (3, 11), (3, 12), (3, 13)
        };

        foreach (var (rolId, permisoId) in asignaciones)
        {
            var rol = Roles.First(r => r.Id == rolId);
            var permiso = Permisos.First(p => p.Id == permisoId);
            var rp = new RolPermiso { RolId = rolId, PermisoId = permisoId, Rol = rol, Permiso = permiso };
            RolPermisos.Add(rp);
            rol.RolPermisos.Add(rp);
            permiso.RolPermisos.Add(rp);
        }
    }

    /// <summary>
    /// Superusuario inicial QUEMADO, leído de configuración (SuperusuarioSeed) con
    /// fallback a valores por defecto. La contraseña se hashea con BCrypt igual que en
    /// producción, para que el login use exactamente la misma verificación.
    /// </summary>
    private void SeedSuperusuario(IConfiguration configuration)
    {
        const string emailPorDefecto = "super@sigac.cr";
        const string passwordPorDefecto = "Sigac.Super2024!";

        var email = configuration["SuperusuarioSeed:Email"];
        var password = configuration["SuperusuarioSeed:Password"];
        if (string.IsNullOrWhiteSpace(email)) email = emailPorDefecto;
        if (string.IsNullOrWhiteSpace(password)) password = passwordPorDefecto;

        var rolSuper = Roles.First(r => r.Nombre == "superusuario");
        var superusuario = new Usuario
        {
            Id = NextUsuarioId(),
            Nombre = "Superusuario SIGAC",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RolId = rolSuper.Id,
            Rol = rolSuper,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };
        Usuarios.Add(superusuario);
        rolSuper.Usuarios.Add(superusuario);
    }
}
