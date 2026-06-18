namespace HackerRank1.Services;

/// <summary>
/// Se lanza cuando un actor intenta otorgar permisos que él mismo no posee
/// (anti-escalamiento) o permisos exclusivos del superusuario.
/// Los controladores la mapean a 403 Forbidden.
/// </summary>
public class PermisoDenegadoException : Exception
{
    public PermisoDenegadoException(string message) : base(message)
    {
    }
}
