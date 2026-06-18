namespace HackerRank1.DTO;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public UsuarioResponse Usuario { get; set; } = null!;
    public List<string> Permisos { get; set; } = new();
}
