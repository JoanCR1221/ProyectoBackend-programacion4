using HackerRank1.DTO;
using HackerRank1.Filters;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IPermisoService _permisoService;

    public UsuariosController(IUsuarioService usuarioService, IPermisoService permisoService)
    {
        _usuarioService = usuarioService;
        _permisoService = permisoService;
    }

    [HttpGet]
    [TienePermiso("usuarios.ver")]
    public async Task<IActionResult> ObtenerTodos()
    {
        try
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [TienePermiso("usuarios.ver")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id}/estado")]
    [TienePermiso("usuarios.gestionar")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
    {
        try
        {
            await _usuarioService.CambiarEstadoAsync(id, request.Activo);
            var estado = request.Activo ? "activado" : "desactivado";
            return Ok(new { mensaje = $"Usuario {estado} correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/rol")]
    [TienePermiso("roles.asignar")]
    public async Task<IActionResult> AsignarRol(int id, [FromBody] AsignarRolRequest request)
    {
        try
        {
            // Anti-escalamiento: el actor no puede otorgar (vía el rol) permisos que no posee.
            var permisosDelActor = await ObtenerPermisosDelUsuarioActual();
            await _usuarioService.AsignarRolAsync(id, request.RolId, permisosDelActor);
            return Ok(new { mensaje = "Rol asignado correctamente" });
        }
        catch (PermisoDenegadoException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private async Task<List<string>> ObtenerPermisosDelUsuarioActual()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var usuarioId))
            return new List<string>();

        return await _permisoService.ObtenerPermisosDeUsuarioAsync(usuarioId);
    }
}

public class CambiarEstadoRequest
{
    public bool Activo { get; set; }
}

public class AsignarRolRequest
{
    public int RolId { get; set; }
}
