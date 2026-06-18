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

    public async Task<RolResponse> CrearAsync(string nombre, string descripcion, List<int> permisoIds)
    {
        var rolExistente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);
        if (rolExistente != null)
            throw new InvalidOperationException($"El rol '{nombre}' ya existe");

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = descripcion,
            CreadoEn = DateTime.UtcNow
        };

        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        // Agregar permisos
        foreach (var permisoId in permisoIds)
        {
            var permiso = await _context.Permisos.FindAsync(permisoId);
            if (permiso != null)
            {
                _context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
            }
        }

        await _context.SaveChangesAsync();

        return await ObtenerPorIdAsync(rol.Id) ?? throw new InvalidOperationException("Error al crear rol");
    }

    public async Task<RolResponse> ActualizarAsync(int id, string nombre, string descripcion, List<int> permisoIds)
    {
        var rol = await _context.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        rol.Nombre = nombre;
        rol.Descripcion = descripcion;

        // Eliminar permisos existentes
        _context.RolPermisos.RemoveRange(rol.RolPermisos);

        // Agregar nuevos permisos
        foreach (var permisoId in permisoIds)
        {
            var permiso = await _context.Permisos.FindAsync(permisoId);
            if (permiso != null)
            {
                _context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
            }
        }

        _context.Roles.Update(rol);
        await _context.SaveChangesAsync();

        return await ObtenerPorIdAsync(rol.Id) ?? throw new InvalidOperationException("Error al actualizar rol");
    }

    public async Task EliminarAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id)
            ?? throw new InvalidOperationException("Rol no encontrado");

        // Verificar que no hay usuarios con este rol
        var usuariosConRol = await _context.Usuarios.CountAsync(u => u.RolId == id);
        if (usuariosConRol > 0)
            throw new InvalidOperationException("No se puede eliminar un rol que tiene usuarios asignados");

        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync();
    }
}
