using HackerRank1.Data;
using HackerRank1.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace LibraryService.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Inicializar el módulo de Acceso y Roles.
            // SigacDbContext usa EF Core InMemory (no requiere PostgreSQL): EnsureCreated()
            // aplica el catálogo de roles/permisos (HasData) y luego se siembra el
            // superusuario quemado leído de configuración (SuperusuarioSeed).
            InicializarAccesoYRoles(host);

            host.Run();
        }

        private static void InicializarAccesoYRoles(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<SigacDbContext>();
            context.Database.EnsureCreated();

            var configuration = services.GetRequiredService<IConfiguration>();
            SembrarSuperusuario(context, configuration);
        }

        private static void SembrarSuperusuario(SigacDbContext context, IConfiguration configuration)
        {
            const string emailPorDefecto = "super@sigac.cr";
            const string passwordPorDefecto = "Sigac.Super2024!";

            var email = configuration["SuperusuarioSeed:Email"];
            var password = configuration["SuperusuarioSeed:Password"];
            if (string.IsNullOrWhiteSpace(email)) email = emailPorDefecto;
            if (string.IsNullOrWhiteSpace(password)) password = passwordPorDefecto;

            // Idempotente: no recrear si ya existe (por email).
            if (context.Usuarios.Any(u => u.Email == email))
                return;

            var rolSuper = context.Roles.First(r => r.Nombre == "superusuario");

            context.Usuarios.Add(new Usuario
            {
                Nombre = "Superusuario SIGAC",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RolId = rolSuper.Id,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
