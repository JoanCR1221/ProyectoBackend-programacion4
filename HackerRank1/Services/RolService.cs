using HackerRank1.Data;
using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public class RolService : IRolService
{
    private readonly SigacDbContext _context;

    public RolService(SigacDbContext context)
    {
        _context = context;
    }

    public async Task<List<RolResponse>> ObtenerTodosAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .Select(r => new RolResponse
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CreadoEn = r.CreadoEn,
                Permisos = r.RolPermisos.Select(rp => new PermisoResponse
                {
                    Id = rp.Permiso.Id,
                    Clave = rp.Permiso.Clave,
                    Descripcion = rp.Permiso.Descripcion
                }).ToList()
            })
            .ToListAsync();

        return roles;
    }

    public async Task<RolResponse?> ObtenerPorIdAsync(int id)
    {
        var rol = await _context.Roles
            .Where(r => r.Id == id)
            .Include(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .Select(r => new RolResponse
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CreadoEn = r.CreadoEn,
                Permisos = r.RolPermisos.Select(rp => new PermisoResponse
                {
                    Id = rp.Permiso.Id,
                    Clave = rp.Permiso.Clave,
                    Descripcion = rp.Permiso.Descripcion
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return rol;
    }

    public async Task<RolResponse> CrearAsync(string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor)
    {
        var rolExistente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);
        if (rolExistente != null)
            throw new InvalidOperationException($"El rol '{nombre}' ya existe");

        // Resolver y validar los permisos solicitados (anti-escalamiento) antes de crear nada.
        var permisosValidados = await ValidarPermisosAsync(permisoIds, permisosDelActor);

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = descripcion,
            CreadoEn = DateTime.UtcNow
        };

        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        foreach (var permisoId in permisosValidados)
        {
            _context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
        }

        await _context.SaveChangesAsync();

        return await ObtenerPorIdAsync(rol.Id) ?? throw new InvalidOperationException("Error al crear rol");
    }

    public async Task<RolResponse> ActualizarAsync(int id, string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor)
    {
        var rol = await _context.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        var permisosValidados = await ValidarPermisosAsync(permisoIds, permisosDelActor);

        rol.Nombre = nombre;
        rol.Descripcion = descripcion;

        // Reemplazar el conjunto de permisos del rol.
        _context.RolPermisos.RemoveRange(rol.RolPermisos);

        foreach (var permisoId in permisosValidados)
        {
            _context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
        }

        await _context.SaveChangesAsync();

        return await ObtenerPorIdAsync(rol.Id) ?? throw new InvalidOperationException("Error al actualizar rol");
    }

    public async Task EliminarAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        var usuariosConRol = await _context.Usuarios.CountAsync(u => u.RolId == id);
        if (usuariosConRol > 0)
            throw new InvalidOperationException("No se puede eliminar un rol que tiene usuarios asignados");

        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Resuelve los IDs de permisos a sus claves, valida que existan y aplica anti-escalamiento:
    /// el actor NO puede otorgar ningún permiso que él mismo no posea.
    /// Devuelve la lista deduplicada de IDs válidos.
    /// </summary>
    private async Task<List<int>> ValidarPermisosAsync(List<int> permisoIds, List<string> permisosDelActor)
    {
        var idsUnicos = permisoIds.Distinct().ToList();
        if (idsUnicos.Count == 0)
            return idsUnicos;

        var permisos = await _context.Permisos
            .Where(p => idsUnicos.Contains(p.Id))
            .ToListAsync();

        // Validar que todos los IDs solicitados existan.
        var idsEncontrados = permisos.Select(p => p.Id).ToHashSet();
        var idsInexistentes = idsUnicos.Where(id => !idsEncontrados.Contains(id)).ToList();
        if (idsInexistentes.Count > 0)
            throw new InvalidOperationException($"Permisos inexistentes: {string.Join(", ", idsInexistentes)}");

        // Anti-escalamiento: cada permiso a otorgar debe estar dentro de los permisos del actor.
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
}
