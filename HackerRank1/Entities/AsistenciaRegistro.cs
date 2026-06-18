using System.ComponentModel.DataAnnotations;

namespace HackerRank1.Entities;

public class AsistenciaRegistro
{
    [Key]
    public int Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string BeneficiarioId { get; set; } = string.Empty;
}
