using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Data;

public class SigacDbContext : DbContext
{
    public SigacDbContext(DbContextOptions<SigacDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Permiso> Permisos { get; set; }
    public DbSet<RolPermiso> RolPermisos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============ Configuración de Permiso ============
        modelBuilder.Entity<Permiso>()
            .ToTable("permiso")
            .HasKey(p => p.Id);

        modelBuilder.Entity<Permiso>()
            .Property(p => p.Id)
            .HasColumnName("id");

        modelBuilder.Entity<Permiso>()
            .Property(p => p.Clave)
            .HasColumnName("clave")
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Permiso>()
            .HasIndex(p => p.Clave)
            .IsUnique();

        modelBuilder.Entity<Permiso>()
            .Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .IsRequired()
            .HasMaxLength(255);

        // ============ Configuración de Rol ============
        modelBuilder.Entity<Rol>()
            .ToTable("rol")
            .HasKey(r => r.Id);

        modelBuilder.Entity<Rol>()
            .Property(r => r.Id)
            .HasColumnName("id");

        modelBuilder.Entity<Rol>()
            .Property(r => r.Nombre)
            .HasColumnName("nombre")
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .Property(r => r.Descripcion)
            .HasColumnName("descripcion")
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Rol>()
            .Property(r => r.CreadoEn)
            .HasColumnName("creado_en")
            .HasColumnType("timestamp without time zone");

        // ============ Configuración de RolPermiso (Entidad Puente) ============
        modelBuilder.Entity<RolPermiso>()
            .ToTable("rol_permiso")
            .HasKey(rp => new { rp.RolId, rp.PermisoId });

        modelBuilder.Entity<RolPermiso>()
            .Property(rp => rp.RolId)
            .HasColumnName("rol_id");

        modelBuilder.Entity<RolPermiso>()
            .Property(rp => rp.PermisoId)
            .HasColumnName("permiso_id");

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ============ Configuración de Usuario ============
        modelBuilder.Entity<Usuario>()
            .ToTable("usuario")
            .HasKey(u => u.Id);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Id)
            .HasColumnName("id");

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre)
            .HasColumnName("nombre")
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.RolId)
            .HasColumnName("rol_id");

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.CreadoEn)
            .HasColumnName("creado_en")
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        // ============ Datos de Inicialización (Seeding) ============
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Fecha fija para el seed: usar DateTime.UtcNow aquí hace que EF detecte
        // cambios pendientes en el modelo en cada build. Debe ser un valor estático.
        var fechaSeed = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Permisos
        var permisos = new[]
        {
            new Permiso { Id = 1, Clave = "donaciones.crear", Descripcion = "Crear donaciones" },
            new Permiso { Id = 2, Clave = "donaciones.ver", Descripcion = "Ver donaciones" },
            new Permiso { Id = 3, Clave = "donaciones.editar", Descripcion = "Editar donaciones" },
            new Permiso { Id = 4, Clave = "donaciones.eliminar", Descripcion = "Eliminar donaciones" },
            new Permiso { Id = 5, Clave = "beneficiarios.gestionar", Descripcion = "Gestionar beneficiarios" },
            new Permiso { Id = 6, Clave = "inventario.gestionar", Descripcion = "Gestionar inventario" },
            new Permiso { Id = 7, Clave = "gastos.gestionar", Descripcion = "Gestionar gastos" },
            new Permiso { Id = 8, Clave = "usuarios.ver", Descripcion = "Ver usuarios" },
            new Permiso { Id = 9, Clave = "usuarios.gestionar", Descripcion = "Gestionar usuarios" },
            new Permiso { Id = 10, Clave = "usuarios.crear_admin", Descripcion = "Crear administradores" },
            new Permiso { Id = 11, Clave = "roles.ver", Descripcion = "Ver roles" },
            new Permiso { Id = 12, Clave = "roles.gestionar", Descripcion = "Gestionar roles" },
            new Permiso { Id = 13, Clave = "roles.asignar", Descripcion = "Asignar roles a usuarios" }
        };

        modelBuilder.Entity<Permiso>().HasData(permisos);

        // Roles
        var roles = new[]
        {
            new Rol { Id = 1, Nombre = "usuario", Descripcion = "Rol básico de usuario", CreadoEn = fechaSeed },
            new Rol { Id = 2, Nombre = "administrador", Descripcion = "Rol de administrador operativo", CreadoEn = fechaSeed },
            new Rol { Id = 3, Nombre = "superusuario", Descripcion = "Rol de superusuario con todos los permisos", CreadoEn = fechaSeed }
        };

        modelBuilder.Entity<Rol>().HasData(roles);

        // Asignaciones de Permisos a Roles
        var rolPermisos = new[]
        {
            // Rol "usuario" - solo donaciones.crear y donaciones.ver
            new RolPermiso { RolId = 1, PermisoId = 1 }, // donaciones.crear
            new RolPermiso { RolId = 1, PermisoId = 2 }, // donaciones.ver

            // Rol "administrador" - operativo
            new RolPermiso { RolId = 2, PermisoId = 1 }, // donaciones.crear
            new RolPermiso { RolId = 2, PermisoId = 2 }, // donaciones.ver
            new RolPermiso { RolId = 2, PermisoId = 3 }, // donaciones.editar
            new RolPermiso { RolId = 2, PermisoId = 4 }, // donaciones.eliminar
            new RolPermiso { RolId = 2, PermisoId = 5 }, // beneficiarios.gestionar
            new RolPermiso { RolId = 2, PermisoId = 6 }, // inventario.gestionar
            new RolPermiso { RolId = 2, PermisoId = 7 }, // gastos.gestionar
            new RolPermiso { RolId = 2, PermisoId = 8 }, // usuarios.ver
            new RolPermiso { RolId = 2, PermisoId = 9 }, // usuarios.gestionar
            new RolPermiso { RolId = 2, PermisoId = 11 }, // roles.ver

            // Rol "superusuario" - todos los permisos
            new RolPermiso { RolId = 3, PermisoId = 1 }, // donaciones.crear
            new RolPermiso { RolId = 3, PermisoId = 2 }, // donaciones.ver
            new RolPermiso { RolId = 3, PermisoId = 3 }, // donaciones.editar
            new RolPermiso { RolId = 3, PermisoId = 4 }, // donaciones.eliminar
            new RolPermiso { RolId = 3, PermisoId = 5 }, // beneficiarios.gestionar
            new RolPermiso { RolId = 3, PermisoId = 6 }, // inventario.gestionar
            new RolPermiso { RolId = 3, PermisoId = 7 }, // gastos.gestionar
            new RolPermiso { RolId = 3, PermisoId = 8 }, // usuarios.ver
            new RolPermiso { RolId = 3, PermisoId = 9 }, // usuarios.gestionar
            new RolPermiso { RolId = 3, PermisoId = 10 }, // usuarios.crear_admin
            new RolPermiso { RolId = 3, PermisoId = 11 }, // roles.ver
            new RolPermiso { RolId = 3, PermisoId = 12 }, // roles.gestionar
            new RolPermiso { RolId = 3, PermisoId = 13 } // roles.asignar
        };

        modelBuilder.Entity<RolPermiso>().HasData(rolPermisos);
    }
}
