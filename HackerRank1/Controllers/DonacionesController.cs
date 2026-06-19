
using HackerRank1.DTO;
using HackerRank1.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace HackerRank1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [Authorize]
    public class DonacionesController : ControllerBase
    {
        private static List<DonacionDTO> donaciones = new();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(donaciones);
        }

        [HttpPost]
        [TienePermiso("donaciones.crear")]
        public IActionResult Create([FromBody] DonacionDTO dto)
        {

            var permisos = User.FindAll("permiso").Select(p => p.Value).ToList();

            //  usuario normal -- solo transferencia
            if (permisos.Contains("donaciones.crear") &&
                !permisos.Contains("donaciones.gestionar"))
            {
                if (dto.Tipo != "Transferencia")
                {
                    return StatusCode(403, new { mensaje = "Solo puede realizar transferencias" });
                }
            }


            // Validaciones básicas
            if (string.IsNullOrEmpty(dto.Tipo))
                return BadRequest("El tipo es obligatorio");

            if (string.IsNullOrEmpty(dto.Donante))
                return BadRequest("El donante es obligatorio");

            if (dto.Fecha == default)
                return BadRequest("La fecha es obligatoria");


            // Validaciones por tipo

            // DINERO
            if (dto.Tipo == "Dinero")
            {
                if (dto.Monto == null || dto.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a 0");

                // limpiar campos innecesarios
                dto.Descripcion = null;
                dto.Cantidad = null;
                dto.Unidad = null;
                dto.Banco = null;
                dto.NumeroTransaccion = null;
            }

            // ESPECIE
            else if (dto.Tipo == "Especie")
            {
                if (string.IsNullOrEmpty(dto.Descripcion) || dto.Cantidad == null || dto.Cantidad <= 0)
                    return BadRequest("Datos de especie incompletos");

                dto.Monto = null;
                dto.Banco = null;
                dto.NumeroTransaccion = null;
            }

            // TRANSFERENCIA
            else if (dto.Tipo == "Transferencia")
            {
                if (dto.Monto == null || dto.Monto <= 0 ||
                    string.IsNullOrEmpty(dto.Banco) ||
                    string.IsNullOrEmpty(dto.NumeroTransaccion))
                {
                    return BadRequest("Datos de transferencia incompletos");
                }

                dto.Descripcion = null;
                dto.Cantidad = null;
                dto.Unidad = null;
            }

            else
            {
                return BadRequest("Tipo de donación no válido");
            }


            // Generar ID
            dto.Id = donaciones.Count > 0 ? donaciones.Max(d => d.Id) + 1 : 1;

            // Estado inicial
            dto.Anulado = false;

            // Guardar
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