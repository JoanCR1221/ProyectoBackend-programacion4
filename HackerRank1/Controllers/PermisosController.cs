using HackerRank1.DTO;
using HackerRank1.Filters;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _permisoService;

    public PermisosController(IPermisoService permisoService)
    {
        _permisoService = permisoService;
    }

    [HttpGet]
    [TienePermiso("roles.ver")]
    public async Task<IActionResult> ObtenerTodos()
    {
        try
        {
            var permisos = await _permisoService.ObtenerTodosAsync();
            return Ok(permisos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [TienePermiso("roles.ver")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var permiso = await _permisoService.ObtenerPorIdAsync(id);
            if (permiso == null)
                return NotFound(new { mensaje = "Permiso no encontrado" });

            return Ok(permiso);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
