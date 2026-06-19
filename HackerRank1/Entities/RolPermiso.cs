namespace HackerRank1.Entities;

public class RolPermiso
{
    public int RolId { get; set; }

    public int PermisoId { get; set; }

    // Relaciones de navegación
    public Rol Rol { get; set; } = null!;

    public Permiso Permiso { get; set; } = null!;
}
