namespace HackerRank1.DTO;

public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
}
