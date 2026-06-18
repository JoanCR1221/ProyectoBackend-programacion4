using HackerRank1.DTO;

namespace HackerRank1.Services;

public interface IPermisoService
{
    Task<List<PermisoResponse>> ObtenerTodosAsync();
    Task<PermisoResponse?> ObtenerPorIdAsync(int id);
    Task<List<string>> ObtenerPermisosDeUsuarioAsync(int usuarioId);
}
