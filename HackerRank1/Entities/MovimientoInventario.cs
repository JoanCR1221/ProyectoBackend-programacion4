namespace HackerRank1.Entities;

public class MovimientoInventario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string ProductoId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "Entrada" | "Salida"
    public int Cantidad { get; set; }
    public int ExistenciaAnterior { get; set; }
    public int ExistenciaNueva { get; set; }

    public Producto Producto { get; set; } = null!;
}
