using HackerRank1.DTO;
using HackerRank1.Entities;

namespace HackerRank1.Services;

public interface IRolService
{
    Task<List<RolResponse>> ObtenerTodosAsync();
    Task<RolResponse?> ObtenerPorIdAsync(int id);
    Task<RolResponse> CrearAsync(string nombre, string descripcion, List<int> permisoIds);
    Task<RolResponse> ActualizarAsync(int id, string nombre, string descripcion, List<int> permisoIds);
    Task EliminarAsync(int id);
}
