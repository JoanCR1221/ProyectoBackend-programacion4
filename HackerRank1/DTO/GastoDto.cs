using System.ComponentModel.DataAnnotations;

namespace HackerRank1.DTO;

public class CreateGastoDto
{
    [Required(ErrorMessage = "El detalle es requerido")]
    [MaxLength(100)]
    public string Detalle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha del gasto es requerida")]
    public DateOnly FechaGasto { get; set; }
}

public class UpdateGastoDto
{
    [Required(ErrorMessage = "El detalle es requerido")]
    [MaxLength(100)]
    public string Detalle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha del gasto es requerida")]
    public DateOnly FechaGasto { get; set; }
}

public class GastoResponseDto
{
    public int Id { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public DateOnly FechaGasto { get; set; }
}
