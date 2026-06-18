using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HackerRank1.Entities;

public class Producto
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Existencia { get; set; }
    public int Minimo { get; set; }

    [NotMapped]
    public string Estado => Existencia == 0 ? "Agotado"
                          : Existencia <= Minimo ? "Mínimos"
                          : "Normal";

    public string Ubicacion { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}
