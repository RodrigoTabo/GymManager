
using GymManager.Api.Application.Interfaces;
using GymManager.Api.Application.Middleware;
using GymManager.Api.Application.Services;
using GymManager.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //TODOS LOS SERVICIOS
            //Socios
            builder.Services.AddScoped<ISocioService, SocioService>();
            builder.Services.AddScoped<ISocioStatsService, SocioStatsService>();
            //Planes
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IPlanStatsService, PlanStatsService>();
            //Pagos
            builder.Services.AddScoped<IPagoService, PagoService>();
            builder.Services.AddScoped<IPagoStatsService, PagoStatsService>();
            //Asistencias
            builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
            //Metodos de pagos
            builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
            //Intentos de Acceso
            builder.Services.AddScoped<IIntentosAccesoService, IntentosAccessoService>();
            //Stats General
            builder.Services.AddScoped<IGeneralService, GeneralService>();
            //Contexto y conexion a la DB.
            builder.Services.AddDbContext<GymManagerDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("GymManagerDbContext")));

            builder.Services.AddControllers();
            builder.Services.AddApiExceptionHandling();


            builder.Services.AddEndpointsApiExplorer();
            // Swagger
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Client", policy =>
                    policy.WithOrigins("https://localhost:7083")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            var app = builder.Build();

            app.UseApiExceptionHandling();

            app.UseCors("Client");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();



            app.MapControllers();

            app.Run();
        }
    }
}

