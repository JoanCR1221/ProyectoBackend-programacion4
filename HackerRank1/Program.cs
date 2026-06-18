using HackerRank1.Data;
using HackerRank1.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<SigacDbContext>();
                    var configuration = services.GetRequiredService<IConfiguration>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Aplicando migraciones pendientes...");
                    dbContext.Database.Migrate();
                    logger.LogInformation("Migraciones aplicadas correctamente");

                    SeedSuperusuario(dbContext, configuration, logger);
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

        private static void SeedSuperusuario(SigacDbContext dbContext, IConfiguration configuration, ILogger logger)
        {
            try
            {
                var superEmail = configuration["SuperusuarioSeed:Email"];
                var superPassword = configuration["SuperusuarioSeed:Password"];

                if (string.IsNullOrEmpty(superEmail) || string.IsNullOrEmpty(superPassword))
                {
                    logger.LogWarning(
                        "SuperusuarioSeed no configurado. Configura con: " +
                        "dotnet user-secrets set SuperusuarioSeed:Email <email> " +
                        "y dotnet user-secrets set SuperusuarioSeed:Password <password>");
                    return;
                }

                var superusuarioExistente = dbContext.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefault(u => u.Email == superEmail);

                if (superusuarioExistente != null)
                {
                    logger.LogInformation("Superusuario '{Email}' ya existe. Skipping seed.", superEmail);
                    return;
                }

                var rolSuperusuario = dbContext.Roles.FirstOrDefault(r => r.Nombre == "superusuario")
                    ?? throw new InvalidOperationException("Rol 'superusuario' no encontrado. Asegúrate de haber aplicado la migración 'Inicial'.");

                var superusuario = new Usuario
                {
                    Nombre = "Superusuario SIGAC",
                    Email = superEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(superPassword),
                    RolId = rolSuperusuario.Id,
                    Activo = true,
                    CreadoEn = DateTime.UtcNow
                };

                dbContext.Usuarios.Add(superusuario);
                dbContext.SaveChanges();

                logger.LogInformation("Superusuario creado exitosamente. Email: {Email}", superEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al hacer seed del superusuario");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
