namespace HackerRank1.DTO;

/// <summary>
/// DTO legacy - Mantener solo para compatibilidad temporal
/// Use LoginRequest o RegisterRequest en su lugar
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}

