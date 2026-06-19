using System.ComponentModel.DataAnnotations;

namespace HackerRank1.Entities;

public class Beneficiario
{
    [Key]
    public int Id { get; set; }
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
