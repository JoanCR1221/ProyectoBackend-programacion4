namespace HackerRank1.Services;

/// <summary>
/// Permisos que solo el superusuario puede poseer y otorgar.
/// Ningún administrador puede obtenerlos ni asignarlos a roles/usuarios.
/// </summary>
public static class PermisosExclusivos
{
    public static readonly HashSet<string> Claves = new()
    {
        "roles.asignar",
        "roles.gestionar",
        "usuarios.crear_admin"
    };
}
