using HackerRank1.DTO;

namespace HackerRank1.Services;

public interface IUsuarioService
{
    Task<List<UsuarioResponse>> ObtenerTodosAsync();
    Task<UsuarioResponse?> ObtenerPorIdAsync(int id);
    Task<UsuarioResponse?> ObtenerPorEmailAsync(string email);
    Task CambiarEstadoAsync(int id, bool activo);
    Task AsignarRolAsync(int usuarioId, int rolId);
}
