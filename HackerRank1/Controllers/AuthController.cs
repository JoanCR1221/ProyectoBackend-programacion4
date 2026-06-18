using HackerRank1.DTO;
using HackerRank1.Entities;
using HackerRank1.Helpers;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IPermisoService _permisoService;
    private readonly IUsuarioService _usuarioService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthenticationService authenticationService,
        IPermisoService permisoService,
        IUsuarioService usuarioService,
        JwtSettings jwtSettings)
    {
        _authenticationService = authenticationService;
        _permisoService = permisoService;
        _usuarioService = usuarioService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { mensaje = "Email y contraseña son requeridos" });

        var resultado = await _authenticationService.AutenticarAsync(request.Email, request.Password);
        if (resultado.HasValue == false)
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        var (usuario, permisos) = resultado.Value;
        var token = TokenGenerator.GenerateToken(usuario, _jwtSettings, permisos);

        return Ok(new AuthResponse
        {
            Token = token,
            Usuario = usuario,
            Permisos = permisos
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { mensaje = "Nombre, email y contraseña son requeridos" });

        if (!EsEmailValido(request.Email))
            return BadRequest(new { mensaje = "El formato del email no es válido" });

        if (request.Password.Length < 8)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 8 caracteres" });

        try
        {
            var usuario = await _authenticationService.RegistrarAsync(request.Nombre, request.Email, request.Password);
            var permisos = await _permisoService.ObtenerPermisosDeUsuarioAsync(usuario.Id);

            // Generar token para el nuevo usuario
            var token = TokenGenerator.GenerateToken(usuario, _jwtSettings, permisos);

            return Ok(new AuthResponse
            {
                Token = token,
                Usuario = usuario,
                Permisos = permisos
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ObtenerUsuarioActual()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var usuarioId))
            return Unauthorized(new { mensaje = "Usuario no identificado en el token" });

        var usuario = await _usuarioService.ObtenerPorIdAsync(usuarioId);
        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        var permisos = await _permisoService.ObtenerPermisosDeUsuarioAsync(usuarioId);

        return Ok(new
        {
            usuario,
            permisos
        });
    }

    private static bool EsEmailValido(string email)
    {
        try
        {
            var direccion = new System.Net.Mail.MailAddress(email);
            return direccion.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
