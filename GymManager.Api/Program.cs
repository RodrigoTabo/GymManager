using GymManager.Api.Application.Interfaces;
using GymManager.Api.Application.Middleware;
using GymManager.Api.Application.Services;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Api.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace GymManager.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // TODOS LOS SERVICIOS
            builder.Services.AddScoped<IdentitySeedService>();
            builder.Services.AddHttpContextAccessor();


            // Socios
            builder.Services.AddScoped<ISocioService, SocioService>();
            builder.Services.AddScoped<ISocioStatsService, SocioStatsService>();

            // Planes
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IPlanStatsService, PlanStatsService>();

            // Pagos
            builder.Services.AddScoped<IPagoService, PagoService>();
            builder.Services.AddScoped<IPagoStatsService, PagoStatsService>();

            // Asistencias
            builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();

            // Métodos de pago
            builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();

            // Intentos de acceso
            builder.Services.AddScoped<IIntentosAccesoService, IntentosAccessoService>();

            // Stats general
            builder.Services.AddScoped<IGeneralService, GeneralService>();

            // sucursal
            builder.Services.AddScoped<ISucursalService, SucursalService>();

            //CurrentUser & CurrentSucursal
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ICurrentSucursalService, CurrentSucursalService>();

            // DbContext
            builder.Services.AddDbContext<GymManagerDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("GymManagerDbContext")));

            builder.Services.AddControllers();
            builder.Services.AddApiExceptionHandling();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Client", policy =>
                    policy.WithOrigins("https://localhost:7083")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GymManager API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresá el token JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // Identity
            builder.Services
                .AddIdentityCore<AppUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<GymManagerDbContext>()
                .AddApiEndpoints();

            builder.Services.AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseApiExceptionHandling();
            app.UseCors("Client");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapIdentityApi<AppUser>();


            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GymManagerDbContext>();
                await db.Database.MigrateAsync();

                var seed = scope.ServiceProvider.GetRequiredService<IdentitySeedService>();
                await seed.SeedAsync();
            }

            app.Run();
        }
    }
}