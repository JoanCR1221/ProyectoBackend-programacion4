using HackerRank1.DTO;
using HackerRank1.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GastosController : ControllerBase
{
    private readonly IGastoService _gastoService;

    public GastosController(IGastoService gastoService)
    {
        _gastoService = gastoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GastoResponseDto>>> GetAll()
    {
        var gastos = await _gastoService.GetAllAsync();
        return Ok(gastos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GastoResponseDto>> GetById(int id)
    {
        var gasto = await _gastoService.GetByIdAsync(id);
        if (gasto is null) return NotFound(new { message = $"Gasto con id {id} no encontrado" });
        return Ok(gasto);
    }

    [HttpPost]
    public async Task<ActionResult<GastoResponseDto>> Create([FromBody] CreateGastoDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _gastoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GastoResponseDto>> Update(int id, [FromBody] UpdateGastoDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _gastoService.UpdateAsync(id, dto);
        if (updated is null) return NotFound(new { message = $"Gasto con id {id} no encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _gastoService.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Gasto con id {id} no encontrado" });
        return NoContent();
    }
}
