
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackOverFlowClone.Models.Entities;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StackOverFlowClone.Middleware
{
    public class CustomExceptionHandlerMiddleware : IMiddleware
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(IWebHostEnvironment env, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var endpoint = context.GetEndpoint()?.DisplayName;
            var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.Identity?.Name;
            var requestPath = context.Request.Path.Value ?? string.Empty;

            var (statusCode, title) = exception switch
            {
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Required argument was null."),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided."),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Operation is not valid in the current context."),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access denied."),
                NotImplementedException => (StatusCodes.Status501NotImplemented, "Feature not implemented."),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Database concurrency conflict."),
                DbUpdateException => (StatusCodes.Status500InternalServerError, "Database operation failed."),
                TimeoutException => (StatusCodes.Status504GatewayTimeout, "The request timed out."),
                TaskCanceledException => (StatusCodes.Status408RequestTimeout, "The task was canceled."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            var errorId = Guid.NewGuid();

            _logger.LogError(exception,
                "❌ Exception Caught | ErrorId: {ErrorId} | TraceId: {TraceId} | UserId: {UserId} | Endpoint: {Endpoint} | StatusCode: {StatusCode} | Type: {Type} | Message: {Message} | Request Path: {requestPath}",
                errorId,
                traceId,
                userId,
                endpoint,
                statusCode,
                exception.GetType().Name,
                exception.Message,
                requestPath);

            var errorResponse = new ErrorResponse
            {
                ErrorId = errorId,
                Title = title,
                StatusCode = statusCode,
                TraceId = traceId,
                Endpoint = endpoint,
                ExceptionType = exception.GetType().Name,
                UserId = userId,
                InnerException = exception.InnerException?.Message,
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null,
                RequestPath = requestPath
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(errorResponse);
  

        }
    }
}
