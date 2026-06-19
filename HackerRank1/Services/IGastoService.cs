using HackerRank1.DTO;

namespace HackerRank1.Services;

public interface IGastoService
{
    Task<IEnumerable<GastoResponseDto>> GetAllAsync();
    Task<GastoResponseDto?> GetByIdAsync(int id);
    Task<GastoResponseDto> CreateAsync(CreateGastoDto dto);
    Task<GastoResponseDto?> UpdateAsync(int id, UpdateGastoDto dto);
    Task<bool> DeleteAsync(int id);
}
