namespace HackerRank1.DTO;

public class MovimientoForm
{
    public string ProductoId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "Entrada" | "Salida"
    public int Cantidad { get; set; }
}
