using HackerRank1.DTO;
using HackerRank1.Entities;

namespace HackerRank1.Services;

public interface IRolService
{
    Task<List<RolResponse>> ObtenerTodosAsync();
    Task<RolResponse?> ObtenerPorIdAsync(int id);
    Task<RolResponse> CrearAsync(string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor);
    Task<RolResponse> ActualizarAsync(int id, string nombre, string descripcion, List<int> permisoIds, List<string> permisosDelActor);
    Task EliminarAsync(int id);
}
