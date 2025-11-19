using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Buyly.API.Models;
using Buyly.Application.Exceptions;
using ValidationException = Buyly.Application.Exceptions.ValidationException;

namespace Buyly.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            context.Response.Headers["X-Correlation-ID"] = traceId;

            var response = new ApiResponse
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later.",
                Timestamp = DateTime.UtcNow,
                CorrelationId = traceId
            };

            void LogWarning(string template) =>
                _logger.LogWarning(exception, template + " TraceId: {TraceId}", traceId);

            void LogError(string template) =>
                _logger.LogError(exception, template + " TraceId: {TraceId}", traceId);

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundEx.Message;
                    response.Errors = notFoundEx.Errors;
                    LogWarning("NotFoundException handled.");
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = validationEx.Message;
                    response.Errors = validationEx.Errors;
                    LogWarning("ValidationException handled.");
                    break;

                case BadRequestException badRequestEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = badRequestEx.Message;
                    response.Errors = badRequestEx.Errors;
                    LogWarning("BadRequestException handled.");
                    break;

                case BadHttpRequestException badHttpRequestException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = badHttpRequestException.Message;
                    LogWarning("BadHttpRequestException handled.");
                    break;

                case UnauthorizedException unauthorizedEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = unauthorizedEx.Message;
                    response.Errors = unauthorizedEx.Errors;
                    LogWarning("UnauthorizedException handled.");
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Resource not found";
                    LogWarning("KeyNotFoundException handled.");
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access";
                    LogWarning("UnauthorizedAccessException handled.");
                    break;

                // More specific exception first
                case ArgumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    LogWarning("ArgumentNullException handled.");
                    break;

                // Less specific exception after
                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    LogWarning("ArgumentException handled.");
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = _env.IsDevelopment()
                        ? exception.Message
                        : "An internal server error occurred. Please try again later.";

                    if (_env.IsDevelopment() && !string.IsNullOrEmpty(exception.StackTrace))
                    {
                        response.Errors = new[] { exception.StackTrace };
                    }

                    LogError("Unhandled exception occurred.");
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}