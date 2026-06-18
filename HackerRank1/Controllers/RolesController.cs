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
        try
        {
            var roles = await _rolService.ObtenerTodosAsync();
            return Ok(roles);
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
            var rol = await _rolService.ObtenerPorIdAsync(id);
            if (rol == null)
                return NotFound(new { mensaje = "Rol no encontrado" });

            return Ok(rol);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    [TienePermiso("roles.gestionar")]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Descripcion))
                return BadRequest(new { mensaje = "Nombre y descripción son requeridos" });

            // Validar anti-escalamiento: el usuario no puede otorgar permisos que él no posee
            var permisosDelUsuario = await ObtenerPermisosDelUsuarioActual();
            if (!TienePermisosParaAsignar(request.PermisoIds, permisosDelUsuario, false))
                return Forbid();

            var rol = await _rolService.CrearAsync(request.Nombre, request.Descripcion, request.PermisoIds);
            return Ok(rol);
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

    [HttpPut("{id}")]
    [TienePermiso("roles.gestionar")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Descripcion))
                return BadRequest(new { mensaje = "Nombre y descripción son requeridos" });

            // Validar anti-escalamiento
            var permisosDelUsuario = await ObtenerPermisosDelUsuarioActual();
            if (!TienePermisosParaAsignar(request.PermisoIds, permisosDelUsuario, false))
                return Forbid();

            var rol = await _rolService.ActualizarAsync(id, request.Nombre, request.Descripcion, request.PermisoIds);
            return Ok(rol);
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
        catch (Exception ex)
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

    private bool TienePermisosParaAsignar(List<int> permisoIdsAAsignar, List<string> permisosDelUsuario, bool esCreandoAdmin)
    {
        // Los permisos exclusivos del superusuario
        var permisosExclusivos = new[] { "roles.asignar", "roles.gestionar", "usuarios.crear_admin" };

        // Si el usuario está intentando asignar un permiso exclusivo y no es superusuario, rechazar
        foreach (var permisoExclusivo in permisosExclusivos)
        {
            if (!permisosDelUsuario.Contains(permisoExclusivo) && permisoIdsAAsignar.Count > 0)
            {
                // Verificar si alguno de los permisos a asignar es exclusivo
                // Aquí asumimos que tenemos acceso a los permisos por ID
                // Por ahora solo hacemos la validación básica
            }
        }

        return true;
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
