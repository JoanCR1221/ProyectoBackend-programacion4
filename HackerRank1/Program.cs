using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LibraryService.WebAPI
{
    public class Program
    {
        // FASE SIN BASE DE DATOS:
        // No se aplican migraciones ni se conecta a PostgreSQL. El catálogo de roles/permisos
        // y el superusuario quemado se siembran en memoria (ver Data/InMemoryStore.cs), así que
        // el backend y Swagger arrancan sin requerir ninguna conexión.
        // Para reactivar EF en la integración: ver HackerRank1/_EfDisabled/README.md
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
