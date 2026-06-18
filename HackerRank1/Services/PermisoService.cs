using HackerRank1.Data;
using HackerRank1.DTO;

namespace HackerRank1.Services;

/// <summary>Implementación EN MEMORIA (sin base de datos).</summary>
public class PermisoService : IPermisoService
{
    private readonly InMemoryStore _store;

    public PermisoService(InMemoryStore store)
    {
        _store = store;
    }

    public Task<List<PermisoResponse>> ObtenerTodosAsync()
    {
        var permisos = _store.Permisos
            .Select(p => new PermisoResponse
            {
                Id = p.Id,
                Clave = p.Clave,
                Descripcion = p.Descripcion
            })
            .ToList();

        return Task.FromResult(permisos);
    }

    public Task<PermisoResponse?> ObtenerPorIdAsync(int id)
    {
        var permiso = _store.Permisos
            .Where(p => p.Id == id)
            .Select(p => new PermisoResponse
            {
                Id = p.Id,
                Clave = p.Clave,
                Descripcion = p.Descripcion
            })
            .FirstOrDefault();

        return Task.FromResult(permiso);
    }

    public Task<List<string>> ObtenerPermisosDeUsuarioAsync(int usuarioId)
    {
        var usuario = _store.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        var permisos = usuario?.Rol.RolPermisos.Select(rp => rp.Permiso.Clave).ToList()
            ?? new List<string>();

        return Task.FromResult(permisos);
    }
}
