using HackerRank1.Data;
using HackerRank1.DTO;

namespace HackerRank1.Services;

/// <summary>Implementación EN MEMORIA (sin base de datos).</summary>
public class UsuarioService : IUsuarioService
{
    private readonly InMemoryStore _store;

    public UsuarioService(InMemoryStore store)
    {
        _store = store;
    }

    public Task<List<UsuarioResponse>> ObtenerTodosAsync()
    {
        var usuarios = _store.Usuarios.Select(MapToResponse).ToList();
        return Task.FromResult(usuarios);
    }

    public Task<UsuarioResponse?> ObtenerPorIdAsync(int id)
    {
        var usuario = _store.Usuarios.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(usuario == null ? null : MapToResponse(usuario));
    }

    public Task<UsuarioResponse?> ObtenerPorEmailAsync(string email)
    {
        var usuario = _store.Usuarios
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(usuario == null ? null : MapToResponse(usuario));
    }

    public Task CambiarEstadoAsync(int id, bool activo)
    {
        var usuario = _store.Usuarios.FirstOrDefault(u => u.Id == id)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        usuario.Activo = activo;
        return Task.CompletedTask;
    }

    public Task AsignarRolAsync(int usuarioId, int rolId, List<string> permisosDelActor)
    {
        var usuario = _store.Usuarios.FirstOrDefault(u => u.Id == usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        var rol = _store.Roles.FirstOrDefault(r => r.Id == rolId)
            ?? throw new InvalidOperationException("Rol no encontrado");

        // Anti-escalamiento: asignar un rol otorga al usuario todos los permisos de ese rol.
        // El actor no puede asignar un rol que contenga permisos que él mismo no posee.
        var permisosDelActorSet = permisosDelActor.ToHashSet();
        var permisosNoAutorizados = rol.RolPermisos
            .Select(rp => rp.Permiso.Clave)
            .Where(clave => !permisosDelActorSet.Contains(clave))
            .ToList();

        if (permisosNoAutorizados.Count > 0)
            throw new PermisoDenegadoException(
                $"No puedes asignar el rol '{rol.Nombre}' porque incluye permisos que no posees: " +
                string.Join(", ", permisosNoAutorizados));

        usuario.RolId = rolId;
        usuario.Rol = rol;
        return Task.CompletedTask;
    }

    private static UsuarioResponse MapToResponse(Entities.Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Email = usuario.Email,
        Rol = usuario.Rol.Nombre,
        Activo = usuario.Activo,
        CreadoEn = usuario.CreadoEn
    };
}
