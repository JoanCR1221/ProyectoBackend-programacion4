using HackerRank1.Context;
using HackerRank1.Data;
using HackerRank1.Entities;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace LibraryService.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // ============================================================
            // JWT (compartido por todos los módulos: este token sirve tanto
            // para autorización por permisos -Acceso/Roles- como por roles
            // -Authorize(Roles = ...)- usado en inventario/beneficiarios/etc.)
            // ============================================================
            var jwtSettings = new JwtSettings();
            Configuration.GetSection("JwtSettings").Bind(jwtSettings);

            

            services.AddSingleton(jwtSettings);

            // ============================================================
            // Módulo Acceso y Roles (EF Core InMemory: sin BD real, igual que
            // los demás contextos en memoria, para que el equipo levante todo
            // sin depender de PostgreSQL). El catálogo se siembra vía HasData y
            // el superusuario en Program.cs al iniciar.
            // ============================================================
            services.AddDbContext<SigacDbContext>(options =>
                options.UseInMemoryDatabase("sigacdb"));

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPermisoService, PermisoService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            // ============================================================
            // Módulo Gastos (BD principal PostgreSQL / Supabase)
            // ============================================================
            services.AddDbContext<AppDbContext>(options =>
               options.UseInMemoryDatabase("gastosdb"));
            services.AddScoped<IGastoService, GastoService>();

            // ============================================================
            // Módulo Inventario (EF Core InMemory)
            // ============================================================
            services.AddDbContext<InventarioContext>(options =>
                options.UseInMemoryDatabase("inventariodb"));
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IMovimientoService, MovimientoService>();

            // ============================================================
            // Módulo Beneficiarios y Asistencia (EF Core InMemory)
            // ============================================================
            services.AddDbContext<BeneficiariosContext>(options =>
                options.UseInMemoryDatabase("beneficiariosdb"));
            services.AddScoped<IBeneficiariosService, BeneficiariosService>();
            services.AddScoped<IAsistenciaService, AsistenciaService>();

            // ============================================================
            // CORS (una sola política para el frontend)
            // ============================================================
            var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                 ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:5219" };

            services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // ============================================================
            // Autenticación / Autorización JWT
            // ============================================================
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // ============================================================
            // Swagger
            // ============================================================
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SIGAC API",
                    Version = "v1",
                    Description = "Backend API del SIGAC: autenticación JWT, acceso/roles, inventario, beneficiarios, gastos y donaciones"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Ingrese el token como: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGAC API v1");
                });
            }

            // Sin redirección HTTPS en la fase de desarrollo: cuando la API corre sobre http
            // (sin puerto https configurado), el redirect rompe el "Try it out" de Swagger
            // y las llamadas del frontend con "Failed to fetch / URL scheme must be http or https".
            // app.UseHttpsRedirection();

            app.UseRouting();

            // Aplicar CORS antes de Auth
            app.UseCors("FrontendPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
