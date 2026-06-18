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
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;
    private readonly IPermisoService _permisoService;

    public RolesController(IRolService rolService, IPermisoService permisoService)
    {
        _rolService = rolService;
        _permisoService = permisoService;
    }

    [HttpGet]
    [TienePermiso("roles.ver")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _rolService.ObtenerTodosAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [TienePermiso("roles.ver")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var rol = await _rolService.ObtenerPorIdAsync(id);
        if (rol == null)
            return NotFound(new { mensaje = "Rol no encontrado" });

        return Ok(rol);
    }

    [HttpPost]
    [TienePermiso("roles.gestionar")]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest request)
    {
        if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Descripcion))
            return BadRequest(new { mensaje = "Nombre y descripción son requeridos" });

        try
        {
            var permisosDelActor = await ObtenerPermisosDelActor();
            var rol = await _rolService.CrearAsync(request.Nombre, request.Descripcion, request.PermisoIds, permisosDelActor);
            return Ok(rol);
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

    [HttpPut("{id}")]
    [TienePermiso("roles.gestionar")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolRequest request)
    {
        if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Descripcion))
            return BadRequest(new { mensaje = "Nombre y descripción son requeridos" });

        try
        {
            var permisosDelActor = await ObtenerPermisosDelActor();
            var rol = await _rolService.ActualizarAsync(id, request.Nombre, request.Descripcion, request.PermisoIds, permisosDelActor);
            return Ok(rol);
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

    [HttpDelete("{id}")]
    [TienePermiso("roles.gestionar")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _rolService.EliminarAsync(id);
            return Ok(new { mensaje = "Rol eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private async Task<List<string>> ObtenerPermisosDelActor()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var usuarioId))
            return new List<string>();

        return await _permisoService.ObtenerPermisosDeUsuarioAsync(usuarioId);
    }
}

public class CrearRolRequest
{
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public List<int> PermisoIds { get; set; } = new();
}

public class ActualizarRolRequest
{
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public List<int> PermisoIds { get; set; } = new();
}
