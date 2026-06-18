using HackerRank1.Data;
using HackerRank1.DTO;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public class PermisoService : IPermisoService
{
    private readonly SigacDbContext _context;

    public PermisoService(SigacDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermisoResponse>> ObtenerTodosAsync()
    {
        var permisos = await _context.Permisos
            .Select(p => new PermisoResponse
            {
                Id = p.Id,
                Clave = p.Clave,
                Descripcion = p.Descripcion
            })
            .ToListAsync();

        return permisos;
    }

    public async Task<PermisoResponse?> ObtenerPorIdAsync(int id)
    {
        var permiso = await _context.Permisos
            .Where(p => p.Id == id)
            .Select(p => new PermisoResponse
            {
                Id = p.Id,
                Clave = p.Clave,
                Descripcion = p.Descripcion
            })
            .FirstOrDefaultAsync();

        return permiso;
    }

    public async Task<List<string>> ObtenerPermisosDeUsuarioAsync(int usuarioId)
    {
        var permisos = await _context.Usuarios
            .Where(u => u.Id == usuarioId)
            .Include(u => u.Rol)
            .ThenInclude(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .SelectMany(u => u.Rol.RolPermisos.Select(rp => rp.Permiso.Clave))
            .ToListAsync();

        return permisos;
    }
}
