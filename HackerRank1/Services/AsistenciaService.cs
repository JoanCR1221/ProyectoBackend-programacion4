using HackerRank1.Data;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public interface IAsistenciaService
{
    Task<IEnumerable<string>> GetByFecha(string fecha);
    Task<IEnumerable<string>> ToggleAsistencia(string fecha, string beneficiarioId);
}

public class AsistenciaService : IAsistenciaService
{
    private readonly BeneficiariosContext _context;

    public AsistenciaService(BeneficiariosContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> GetByFecha(string fecha)
    {
        return await _context.AsistenciaRegistros
            .Where(a => a.Fecha == fecha)
            .Select(a => a.BeneficiarioId)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> ToggleAsistencia(string fecha, string beneficiarioId)
    {
        var registro = await _context.AsistenciaRegistros
            .FirstOrDefaultAsync(a => a.Fecha == fecha && a.BeneficiarioId == beneficiarioId);

        if (registro is not null)
            _context.AsistenciaRegistros.Remove(registro);
        else
            _context.AsistenciaRegistros.Add(new AsistenciaRegistro
            {
                Fecha = fecha,
                BeneficiarioId = beneficiarioId
            });

        await _context.SaveChangesAsync();

        return await GetByFecha(fecha);
    }
}
