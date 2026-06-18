using HackerRank1.DTO;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = "admin,encargado,consultor")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productos = await _productoService.GetAllAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto is null) return NotFound();
        return Ok(producto);
    }

    [HttpPost]
    [Authorize(Roles = "admin,encargado")]
    public async Task<IActionResult> Create(ProductoForm form)
    {
        var producto = await _productoService.CreateAsync(form);
        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,encargado")]
    public async Task<IActionResult> Update(string id, ProductoForm form)
    {
        var producto = await _productoService.UpdateAsync(id, form);
        if (producto is null) return NotFound();
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _productoService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
