namespace HackerRank1.DTO;

public class BeneficiarioDto
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string FechaNacimiento { get; set; } = string.Empty;
    public string TipoBeneficiario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public int PersonasACargo { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
    public string FechaRegistro { get; set; } = string.Empty;
}
