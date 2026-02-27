using System.Text.Json;

namespace GymManager.Api.Application.Middleware
{

    //Biblioteca middleware para Rodri.
    public static class ApiExceptionHandling
    {
        public abstract class ApiException : Exception
        {
            public int StatusCode { get; }
            public string Title { get; }

            protected ApiException(int statusCode, string title, string? message = null)
                : base(message ?? title)
            {
                StatusCode = statusCode;
                Title = title;
            }
        }

        public sealed class BadRequestException : ApiException
        {
            public BadRequestException(string title, string? message = null)
                : base(StatusCodes.Status400BadRequest, title, message) { }
        }

        public sealed class NotFoundException : ApiException
        {
            public NotFoundException(string title, string? message = null)
                : base(StatusCodes.Status404NotFound, title, message) { }
        }

        public sealed class ConflictException : ApiException
        {
            public ConflictException(string title, string? message = null)
                : base(StatusCodes.Status409Conflict, title, message) { }
        }

        public sealed class UnprocessableEntityException : ApiException
        {
            public UnprocessableEntityException(string title, string? message = null)
                : base(StatusCodes.Status422UnprocessableEntity, title, message) { }
        }


        public sealed class Middleware : IMiddleware
        {
            private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                try
                {
                    await next(context);
                }
                catch (ApiException ex)
                {
                    await WriteProblem(context, ex.StatusCode, ex.Title, ex.Message);
                }
                catch (Exception)
                {
                    await WriteProblem(context, StatusCodes.Status500InternalServerError,
                        "Ocurrió un error inesperado.", "Ocurrió un error inesperado.");
                }
            }

            private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = status;

                var body = new
                {
                    title,
                    status,
                    detail
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            }
        }

        public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
        {
            // IMiddleware necesita DI
            services.AddScoped<Middleware>();
            return services;
        }

        public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Middleware>();
        }
    }

}
