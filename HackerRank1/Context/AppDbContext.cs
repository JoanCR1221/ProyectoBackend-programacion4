using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Gasto> Gastos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.ToTable("gastos");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(g => g.Detalle).HasColumnName("detalle").IsRequired().HasMaxLength(100);
            entity.Property(g => g.Monto).HasColumnName("monto").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(g => g.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
            entity.Property(g => g.FechaGasto).HasColumnName("fecha_gasto").IsRequired();
        });
    }
}
