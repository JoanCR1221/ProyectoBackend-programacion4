using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Data;

public class BeneficiariosContext : DbContext
{
    public BeneficiariosContext(DbContextOptions<BeneficiariosContext> options) : base(options) { }

    public DbSet<Beneficiario> Beneficiarios { get; set; }
    public DbSet<AsistenciaRegistro> AsistenciaRegistros { get; set; }
}
