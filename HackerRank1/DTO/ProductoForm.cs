namespace HackerRank1.DTO;

public class ProductoForm
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Existencia { get; set; }
    public int Minimo { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
}
