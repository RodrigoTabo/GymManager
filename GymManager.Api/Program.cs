
using GymManager.Api.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<GymManagerDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("GymManagerDbContext")));

            builder.Services.AddControllers();

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

