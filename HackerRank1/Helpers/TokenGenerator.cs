using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HackerRank1.Helpers;

public static class TokenGenerator
{
    // Mapeo de los nombres de rol internos (snake/minuscula) a los alias que usan
    // los demas modulos del equipo en sus [Authorize(Roles = "...")].
    // Asi el mismo token funciona con autorizacion por permisos (este modulo) y
    // por roles (modulos de inventario, beneficiarios, asistencia, etc.).
    private static readonly Dictionary<string, string> AliasRol = new(StringComparer.OrdinalIgnoreCase)
    {
        ["superusuario"] = "Super usuario",
        ["administrador"] = "Administrador",
        ["usuario"] = "Usuario"
    };

    public static string GenerateToken(UsuarioResponse user, JwtSettings jwtSettings, IEnumerable<string> permisos)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            // Claim propio de este modulo (valor interno del rol).
            new Claim("rol", user.Rol)
        };

        // Compatibilidad con [Authorize(Roles = ...)] de los otros modulos:
        // emitir ClaimTypes.Role con el alias y tambien con el valor interno.
        if (AliasRol.TryGetValue(user.Rol, out var alias))
            claims.Add(new Claim(ClaimTypes.Role, alias));
        claims.Add(new Claim(ClaimTypes.Role, user.Rol));

        // Autorizacion por permisos (este modulo): un claim "permiso" por cada permiso.
        foreach (var permiso in permisos)
        {
            claims.Add(new Claim("permiso", permiso));
        }

        // Crear una llave simétrica para cifrar el token
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        // Crear la llave que garantiza que el Token fue emitido por este servicio
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Construir el token
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: cred
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
