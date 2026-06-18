
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using HackerRank1.DTO;

namespace HackerRank1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonacionesController : ControllerBase
    {
        private static List<DonacionDTO> donaciones = new();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(donaciones);
        }

        [HttpPost]
        public IActionResult Create([FromBody] DonacionDTO dto)
        {
            dto.Id = donaciones.Count > 0 ? donaciones.Max(d => d.Id) + 1 : 1;
            dto.Anulado = false;

            donaciones.Add(dto);
            return Ok(dto);
        }



        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DonacionDTO dto)
        {
            var d = donaciones.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            d.Tipo = dto.Tipo;
            d.Monto = dto.Monto;
            d.Descripcion = dto.Descripcion;
            d.Cantidad = dto.Cantidad;
            d.Unidad = dto.Unidad;
            d.Donante = dto.Donante;
            d.Fecha = dto.Fecha;

            return Ok(d);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var d = donaciones.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            donaciones.Remove(d);
            return NoContent();
        }

        [HttpPut("{id}/anular")]
        public IActionResult Anular(int id)
        {
            var d = donaciones.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            d.Anulado = !d.Anulado;
            return Ok(d);
        }
    }
}