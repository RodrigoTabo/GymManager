using FluentValidation;
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
            public string? Detail { get; }

            protected ApiException(int statusCode, string title, string? detail = null)
                : base(detail ?? title)
            {
                StatusCode = statusCode;
                Title = title;
                Detail = detail;
            }
        }

        public sealed class BadRequestException : ApiException
        {
            public BadRequestException(string title, string? detail = null)
                : base(StatusCodes.Status400BadRequest, title, detail) { }
        }

        public sealed class NotFoundException : ApiException
        {
            public NotFoundException(string title, string? detail = null)
                : base(StatusCodes.Status404NotFound, title, detail) { }
        }

        public sealed class ConflictException : ApiException
        {
            public ConflictException(string title, string? detail = null)
                : base(StatusCodes.Status409Conflict, title, detail) { }
        }

        public sealed class UnprocessableEntityException : ApiException
        {
            public UnprocessableEntityException(string title, string? detail = null)
                : base(StatusCodes.Status422UnprocessableEntity, title, detail) { }
        }

        public sealed class ExceptionHandlingMiddleware : IMiddleware
        {
            private readonly IWebHostEnvironment _env;
            private readonly ILogger<ExceptionHandlingMiddleware> _logger;

            private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

            public ExceptionHandlingMiddleware(IWebHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
            {
                _env = env;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {

                var traceId = context.TraceIdentifier;

                try
                {
                    await next(context);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning(ex, "Validation error");

                    await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation Error", "Uno o más errores de validación ocurrieron", traceId: traceId, errors: errors);
                }
                catch (ApiException ex)
                {
                    _logger.LogWarning(ex, "Business error");

                    await WriteProblem(context, ex.StatusCode, ex.Title, ex.Detail, traceId: traceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception");

                    var detail = _env.IsDevelopment()
                        ? ex.ToString()
                        : "Ocurrió un error inesperado.";

                    await WriteProblem(context, StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.", detail, traceId: traceId);
                }
            }

            private static async Task WriteProblem(HttpContext context, int status, string title, string? detail = null, string? traceId = null, object? errors = null)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = status;

                var body = new
                {
                    title,
                    status,
                    detail,
                    traceId,
                    errors
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            }
        }

        public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
        {
            // IMiddleware necesita DI
            services.AddScoped<ExceptionHandlingMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}