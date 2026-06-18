using HackerRank1.Data;
using HackerRank1.DTO;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public interface IAuthenticationService
{
    Task<(UsuarioResponse usuario, List<string> permisos)?> AutenticarAsync(string email, string password);
    Task<UsuarioResponse> RegistrarAsync(string nombre, string email, string password);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly SigacDbContext _context;

    public AuthenticationService(SigacDbContext context)
    {
        _context = context;
    }

    public async Task<(UsuarioResponse usuario, List<string> permisos)?> AutenticarAsync(string email, string password)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .ThenInclude(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !usuario.Activo)
            return null;

        // Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return null;

        var usuarioResponse = new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.Nombre,
            Activo = usuario.Activo,
            CreadoEn = usuario.CreadoEn
        };

        var permisos = usuario.Rol.RolPermisos
            .Select(rp => rp.Permiso.Clave)
            .ToList();

        return (usuarioResponse, permisos);
    }

    public async Task<UsuarioResponse> RegistrarAsync(string nombre, string email, string password)
    {
        // Validar email único
        var existente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (existente != null)
            throw new InvalidOperationException("El email ya está registrado");

        // Obtener rol "usuario" (siempre se asigna este rol)
        var rolUsuario = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "usuario")
            ?? throw new InvalidOperationException("Rol 'usuario' no encontrado");

        // Hashear contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var usuario = new Entities.Usuario
        {
            Nombre = nombre,
            Email = email,
            PasswordHash = passwordHash,
            RolId = rolUsuario.Id,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = rolUsuario.Nombre,
            Activo = usuario.Activo,
            CreadoEn = usuario.CreadoEn
        };
    }
}
