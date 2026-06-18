using HackerRank1.Data;
using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public interface IProductoService
{
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<Producto?> GetByIdAsync(string id);
    Task<Producto> CreateAsync(ProductoForm form);
    Task<Producto?> UpdateAsync(string id, ProductoForm form);
    Task<bool> DeleteAsync(string id);
}

public class ProductoService : IProductoService
{
    private readonly InventarioContext _context;

    public ProductoService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
        => await _context.Productos.ToListAsync();

    public async Task<Producto?> GetByIdAsync(string id)
        => await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Producto> CreateAsync(ProductoForm form)
    {
        var producto = new Producto
        {
            Id = form.Id,
            Nombre = form.Nombre,
            Existencia = form.Existencia,
            Minimo = form.Minimo,
            Ubicacion = form.Ubicacion,
            FechaRegistro = DateTime.UtcNow,
            UltimaActualizacion = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task<Producto?> UpdateAsync(string id, ProductoForm form)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return null;

        producto.Nombre = form.Nombre;
        producto.Existencia = form.Existencia;
        producto.Minimo = form.Minimo;
        producto.Ubicacion = form.Ubicacion;
        producto.UltimaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return false;

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return true;
    }
}
