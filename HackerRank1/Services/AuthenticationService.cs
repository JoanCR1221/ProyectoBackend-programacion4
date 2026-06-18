using HackerRank1.Data;
using HackerRank1.DTO;

namespace HackerRank1.Services;

public interface IAuthenticationService
{
    Task<(UsuarioResponse usuario, List<string> permisos)?> AutenticarAsync(string email, string password);
    Task<UsuarioResponse> RegistrarAsync(string nombre, string email, string password);
}

/// <summary>
/// Implementación EN MEMORIA (sin base de datos). Valida contra el superusuario quemado
/// y cualquier usuario creado en runtime via /register. La verificación de contraseña usa
/// BCrypt igual que la versión con EF.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly InMemoryStore _store;

    public AuthenticationService(InMemoryStore store)
    {
        _store = store;
    }

    public Task<(UsuarioResponse usuario, List<string> permisos)?> AutenticarAsync(string email, string password)
    {
        var usuario = _store.Usuarios
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (usuario == null || !usuario.Activo)
            return Task.FromResult<(UsuarioResponse, List<string>)?>(null);

        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return Task.FromResult<(UsuarioResponse, List<string>)?>(null);

        var usuarioResponse = MapToResponse(usuario);
        var permisos = usuario.Rol.RolPermisos.Select(rp => rp.Permiso.Clave).ToList();

        return Task.FromResult<(UsuarioResponse, List<string>)?>((usuarioResponse, permisos));
    }

    public Task<UsuarioResponse> RegistrarAsync(string nombre, string email, string password)
    {
        var existente = _store.Usuarios
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        if (existente != null)
            throw new InvalidOperationException("El email ya está registrado");

        // register SIEMPRE crea rol "usuario" (se ignora cualquier rol del body).
        var rolUsuario = _store.Roles.FirstOrDefault(r => r.Nombre == "usuario")
            ?? throw new InvalidOperationException("Rol 'usuario' no encontrado");

        var usuario = new Entities.Usuario
        {
            Id = _store.NextUsuarioId(),
            Nombre = nombre,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RolId = rolUsuario.Id,
            Rol = rolUsuario,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _store.Usuarios.Add(usuario);
        rolUsuario.Usuarios.Add(usuario);

        return Task.FromResult(MapToResponse(usuario));
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
