using HackerRank1.Context;
using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public class GastoService : IGastoService
{
    private readonly AppDbContext _context;

    public GastoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GastoResponseDto>> GetAllAsync()
    {
        return await _context.Gastos
            .Select(g => ToDto(g))
            .ToListAsync();
    }

    public async Task<GastoResponseDto?> GetByIdAsync(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        return gasto is null ? null : ToDto(gasto);
    }

    public async Task<GastoResponseDto> CreateAsync(CreateGastoDto dto)
    {
        var gasto = new Gasto
        {
            Detalle = dto.Detalle,
            Monto = dto.Monto,
            Descripcion = dto.Descripcion,
            FechaGasto = dto.FechaGasto
        };

        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();
        return ToDto(gasto);
    }

    public async Task<GastoResponseDto?> UpdateAsync(int id, UpdateGastoDto dto)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto is null) return null;

        gasto.Detalle = dto.Detalle;
        gasto.Monto = dto.Monto;
        gasto.Descripcion = dto.Descripcion;
        gasto.FechaGasto = dto.FechaGasto;

        await _context.SaveChangesAsync();
        return ToDto(gasto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto is null) return false;

        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync();
        return true;
    }

    private static GastoResponseDto ToDto(Gasto g) => new()
    {
        Id = g.Id,
        Detalle = g.Detalle,
        Monto = g.Monto,
        Descripcion = g.Descripcion,
        FechaGasto = g.FechaGasto
    };
}
