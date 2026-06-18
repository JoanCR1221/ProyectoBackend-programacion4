using HackerRank1.Data;
using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public interface IMovimientoService
{
    Task<IEnumerable<MovimientoInventario>> GetAllAsync();
    Task<MovimientoInventario?> RegistrarAsync(MovimientoForm form);
}

public class MovimientoService : IMovimientoService
{
    private readonly InventarioContext _context;

    public MovimientoService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MovimientoInventario>> GetAllAsync()
        => await _context.Movimientos
            .Include(m => m.Producto)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

    public async Task<MovimientoInventario?> RegistrarAsync(MovimientoForm form)
    {
        var producto = await _context.Productos.FindAsync(form.ProductoId);
        if (producto is null) return null;

        var existenciaAnterior = producto.Existencia;

        producto.Existencia = form.Tipo == "Entrada"
            ? producto.Existencia + form.Cantidad
            : Math.Max(0, producto.Existencia - form.Cantidad);

        producto.UltimaActualizacion = DateTime.UtcNow;

        var movimiento = new MovimientoInventario
        {
            Fecha = DateTime.UtcNow,
            ProductoId = form.ProductoId,
            Tipo = form.Tipo,
            Cantidad = form.Cantidad,
            ExistenciaAnterior = existenciaAnterior,
            ExistenciaNueva = producto.Existencia
        };

        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();
        return movimiento;
    }
}
