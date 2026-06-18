using HackerRank1.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsistenciaController : ControllerBase
{
    private readonly IAsistenciaService _service;

    public AsistenciaController(IAsistenciaService service)
    {
        _service = service;
    }

    [HttpGet("{fecha}")]
    public async Task<IActionResult> GetByFecha(string fecha)
    {
        var presentes = await _service.GetByFecha(fecha);
        return Ok(presentes);
    }

    [HttpPut("{fecha}/{beneficiarioId}/toggle")]
    public async Task<IActionResult> ToggleAsistencia(string fecha, string beneficiarioId)
    {
        var presentes = await _service.ToggleAsistencia(fecha, beneficiarioId);
        return Ok(presentes);
    }
}
