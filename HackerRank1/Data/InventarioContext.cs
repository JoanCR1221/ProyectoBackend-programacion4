using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Data;

public class InventarioContext : DbContext
{
    public InventarioContext(DbContextOptions<InventarioContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<MovimientoInventario> Movimientos { get; set; }
}
