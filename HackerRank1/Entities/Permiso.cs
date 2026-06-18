namespace HackerRank1.Entities;

public class Permiso
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    // Relación N:N con Rol
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
