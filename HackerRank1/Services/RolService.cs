using HackerRank1.Data;
using HackerRank1.DTO;
using HackerRank1.Entities;

namespace HackerRank1.Services;

/// <summary>Implementación EN MEMORIA (sin base de datos).</summary>
public class RolService : IRolService
{
    private readonly InMemoryStore _store;

    public RolService(InMemoryStore store)
    {
        _store = store;
    }

    public Task<List<RolResponse>> ObtenerTodosAsync()
    {
        var roles = _store.Roles.Select(MapToResponse).ToList();
        return Task.FromResult(roles);
    }

    public Task<RolResponse?> ObtenerPorIdAsync(int id)
    {
        var rol = _store.Roles.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(rol == null ? null : MapToResponse(rol));
    }

    public Task<RolResponse> CrearAsync(string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor)
    {
        if (_store.Roles.Any(r => r.Nombre == nombre))
            throw new InvalidOperationException($"El rol '{nombre}' ya existe");

        var permisosValidados = ValidarPermisos(permisoIds, permisosDelActor);

        var rol = new Rol
        {
            Id = _store.NextRolId(),
            Nombre = nombre,
            Descripcion = descripcion,
            CreadoEn = DateTime.UtcNow
        };
        _store.Roles.Add(rol);

        AsignarPermisos(rol, permisosValidados);

        return Task.FromResult(MapToResponse(rol));
    }

    public Task<RolResponse> ActualizarAsync(int id, string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor)
    {
        var rol = _store.Roles.FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        var permisosValidados = ValidarPermisos(permisoIds, permisosDelActor);

        rol.Nombre = nombre;
        rol.Descripcion = descripcion;

        // Reemplazar el conjunto de permisos del rol.
        _store.RolPermisos.RemoveAll(rp => rp.RolId == rol.Id);
        rol.RolPermisos.Clear();

        AsignarPermisos(rol, permisosValidados);

        return Task.FromResult(MapToResponse(rol));
    }

    public Task EliminarAsync(int id)
    {
        var rol = _store.Roles.FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        if (_store.Usuarios.Any(u => u.RolId == id))
            throw new InvalidOperationException("No se puede eliminar un rol que tiene usuarios asignados");

        _store.RolPermisos.RemoveAll(rp => rp.RolId == id);
        _store.Roles.Remove(rol);

        return Task.CompletedTask;
    }

    private void AsignarPermisos(Rol rol, List<int> permisoIds)
    {
        foreach (var permisoId in permisoIds)
        {
            var permiso = _store.Permisos.First(p => p.Id == permisoId);
            var rp = new RolPermiso { RolId = rol.Id, PermisoId = permisoId, Rol = rol, Permiso = permiso };
            _store.RolPermisos.Add(rp);
            rol.RolPermisos.Add(rp);
            permiso.RolPermisos.Add(rp);
        }
    }

    /// <summary>
    /// Resuelve los IDs de permisos, valida que existan y aplica anti-escalamiento:
    /// el actor NO puede otorgar ningún permiso que él mismo no posea.
    /// </summary>
    private List<int> ValidarPermisos(List<int> permisoIds, List<string> permisosDelActor)
    {
        var idsUnicos = permisoIds.Distinct().ToList();
        if (idsUnicos.Count == 0)
            return idsUnicos;

        var permisos = _store.Permisos.Where(p => idsUnicos.Contains(p.Id)).ToList();

        var idsEncontrados = permisos.Select(p => p.Id).ToHashSet();
        var idsInexistentes = idsUnicos.Where(id => !idsEncontrados.Contains(id)).ToList();
        if (idsInexistentes.Count > 0)
            throw new InvalidOperationException($"Permisos inexistentes: {string.Join(", ", idsInexistentes)}");

        var permisosDelActorSet = permisosDelActor.ToHashSet();
        var noAutorizados = permisos
            .Where(p => !permisosDelActorSet.Contains(p.Clave))
            .Select(p => p.Clave)
            .ToList();

        if (noAutorizados.Count > 0)
            throw new PermisoDenegadoException(
                $"No puedes otorgar permisos que no posees: {string.Join(", ", noAutorizados)}");

        return idsUnicos;
    }

    private static RolResponse MapToResponse(Rol rol) => new()
    {
        Id = rol.Id,
        Nombre = rol.Nombre,
        Descripcion = rol.Descripcion,
        CreadoEn = rol.CreadoEn,
        Permisos = rol.RolPermisos.Select(rp => new PermisoResponse
        {
            Id = rp.Permiso.Id,
            Clave = rp.Permiso.Clave,
            Descripcion = rp.Permiso.Descripcion
        }).ToList()
    };
}
