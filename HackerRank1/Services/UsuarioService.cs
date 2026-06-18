using HackerRank1.Data;
using HackerRank1.DTO;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public class UsuarioService : IUsuarioService
{
    private readonly SigacDbContext _context;

    public UsuarioService(SigacDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioResponse>> ObtenerTodosAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol.Nombre,
                Activo = u.Activo,
                CreadoEn = u.CreadoEn
            })
            .ToListAsync();

        return usuarios;
    }

    public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Include(u => u.Rol)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol.Nombre,
                Activo = u.Activo,
                CreadoEn = u.CreadoEn
            })
            .FirstOrDefaultAsync();

        return usuario;
    }

    public async Task<UsuarioResponse?> ObtenerPorEmailAsync(string email)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Email == email)
            .Include(u => u.Rol)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol.Nombre,
                Activo = u.Activo,
                CreadoEn = u.CreadoEn
            })
            .FirstOrDefaultAsync();

        return usuario;
    }

    public async Task CambiarEstadoAsync(int id, bool activo)
    {
        var usuario = await _context.Usuarios.FindAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        usuario.Activo = activo;
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task AsignarRolAsync(int usuarioId, int rolId, List<string> permisosDelActor)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        var rol = await _context.Roles
            .Include(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(r => r.Id == rolId)
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
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
