using GymManager.Api.Application.Interfaces;
using GymManager.Api.Application.Middleware;
using GymManager.Api.Application.Services;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Api.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace GymManager.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //SERVICIO DE SERILOG.
            builder.Host.UseSerilog((context, logConfg) => logConfg.ReadFrom.Configuration(context.Configuration));


            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

            // SERVICIOS GENERALES
            builder.Services.AddScoped<IdentitySeedService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ISocioService, SocioService>();
            builder.Services.AddScoped<ISocioStatsService, SocioStatsService>();
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IPlanStatsService, PlanStatsService>();
            builder.Services.AddScoped<IPagoService, PagoService>();
            builder.Services.AddScoped<IPagoStatsService, PagoStatsService>();
            builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
            builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
            builder.Services.AddScoped<IIntentosAccesoService, IntentosAccessoService>();
            builder.Services.AddScoped<IGeneralService, GeneralService>();
            builder.Services.AddScoped<ISucursalService, SucursalService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddDbContext<GymManagerDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("GymManagerDbContext")));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddApiExceptionHandling();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Client", policy =>
                {
                    // En lugar de una URL fija, permitimos todo en desarrollo
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins("https://localhost:7083")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            // SWAGGER
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

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    return apiDesc.ActionDescriptor is ControllerActionDescriptor;
                });
            });

            // IDENTITY CORE
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


            // JWT AUTHENTICATION
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = builder.Configuration["Jwt:Issuer"],
         ValidAudience = builder.Configuration["Jwt:Audience"],
         IssuerSigningKey = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
     };

     options.Events = new JwtBearerEvents
     {
         OnTokenValidated = context =>
         {
             var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

             var sucursalClaim = claimsIdentity?.FindFirst("SucursalId");
             if (sucursalClaim != null)
             {
                 claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sucursalClaim.Value));
             }

             return Task.CompletedTask;
         }
     };
 });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseApiExceptionHandling();
            app.UseCors("Client");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymManager API v1");
                });
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapIdentityApi<AppUser>();

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<GymManagerDbContext>();
                    await db.Database.MigrateAsync();

                    var seed = scope.ServiceProvider.GetRequiredService<IdentitySeedService>();
                    await seed.SeedAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" >>> OJO: No se pudo conectar/migrar la DB todavía: {ex.Message}");
            }

            Log.Information("API funcionando correctamente");
            app.Run();

        }
    }
}