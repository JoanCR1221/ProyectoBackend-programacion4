namespace HackerRank1.Entities;

public class Rol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Relación con Usuario (1:N)
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    // Relación N:N con Permiso
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
