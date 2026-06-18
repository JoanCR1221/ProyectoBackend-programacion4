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

    public async Task AsignarRolAsync(int usuarioId, int rolId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        var rol = await _context.Roles.FindAsync(rolId)
            ?? throw new InvalidOperationException("Rol no encontrado");

        usuario.RolId = rolId;
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
