using HackerRank1.DTO;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Super usuario")]
public class BeneficiariosController : ControllerBase
{
    private readonly IBeneficiariosService _service;

    public BeneficiariosController(IBeneficiariosService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var beneficiarios = await _service.GetAll();
        return Ok(beneficiarios);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BeneficiarioForm form)
    {
        var beneficiario = await _service.Create(form);
        return Created($"/api/beneficiarios/{beneficiario.Id}", beneficiario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] BeneficiarioForm form)
    {
        var beneficiario = await _service.Update(id, form);
        if (beneficiario is null) return NotFound();
        return Ok(beneficiario);
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var beneficiario = await _service.ToggleStatus(id);
        if (beneficiario is null) return NotFound();
        return Ok(beneficiario);
    }
}
