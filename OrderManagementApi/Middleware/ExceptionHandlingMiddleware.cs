using OrderManagement.Domain.Entities;
using System.Net;
using System.Text.Json;

namespace OrderManagementApi.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            logger.LogError(exception, "An unexpected error occurred");

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";
            var details = exception.Message;

            if (exception is OrderManagementException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                details = null;
            }

            var response = new
            {
                status = (int)statusCode,
                message,
                details = details
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}