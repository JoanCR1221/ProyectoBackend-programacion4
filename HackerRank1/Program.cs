using HackerRank1.Data;
using HackerRank1.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryService.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Aplicar migraciones y seed al iniciar
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<SigacDbContext>();

                    // Aplicar migraciones pendientes
                    dbContext.Database.Migrate();

                    // Hacer seed del superusuario si no existe
                    SeedSuperusuario(dbContext, services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error al aplicar migraciones o seed");
                    throw;
                }
            }

            host.Run();
        }

        private static void SeedSuperusuario(SigacDbContext dbContext, IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();

            // Verificar si ya existe el superusuario
            var superusuarioExistente = dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Email == "super@sigac.cr");

            if (superusuarioExistente != null)
            {
                logger.LogInformation("Superusuario ya existe. Skipping seed.");
                return;
            }

            // Obtener o crear el rol superusuario
            var rolSuperusuario = dbContext.Roles.FirstOrDefault(r => r.Nombre == "superusuario")
                ?? throw new InvalidOperationException("Rol 'superusuario' no encontrado en la base de datos");

            // Crear superusuario con contraseña hasheada
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("SuperusuarioSigac2024!");
            var superusuario = new Usuario
            {
                Nombre = "Superusuario SIGAC",
                Email = "super@sigac.cr",
                PasswordHash = passwordHash,
                RolId = rolSuperusuario.Id,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            };

            dbContext.Usuarios.Add(superusuario);
            dbContext.SaveChanges();

            logger.LogInformation("Superusuario creado exitosamente. Email: super@sigac.cr");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
