namespace HackerRank1.DTO;

public class RolResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public DateTime CreadoEn { get; set; }
    public List<PermisoResponse> Permisos { get; set; } = new();
}
