using HackerRank1.DTO;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/movimientos")]
[Authorize(Roles = "superusuario,administrador,usuario")]
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;

    public MovimientosController(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movimientos = await _movimientoService.GetAllAsync();
        return Ok(movimientos);
    }

    [HttpPost]
    [Authorize(Roles = "superusuario,administrador")]
    public async Task<IActionResult> Registrar(MovimientoForm form)
    {
        var movimiento = await _movimientoService.RegistrarAsync(form);
        if (movimiento is null)
            return NotFound(new { message = "Producto no encontrado" });
        return Ok(movimiento);
    }
}
